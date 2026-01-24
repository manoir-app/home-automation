using Home.Common.Llm;
using Home.Common.Model;
using Home.Graph.Common;
using Home.Graph.Server.Models.Llm;
using MongoDB.Driver;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Home.Graph.Server.Services.Llm
{
    /// <summary>
    /// Service de gestion des conversations LLM pour les utilisateurs
    /// </summary>
    public class UserLlmService
    {
        private readonly LlmClient _llmClient;
        private readonly LlmToolsManager _toolsManager;
        private readonly IMongoCollection<LlmConversation> _conversationsCollection;

        // Paramètres en dur pour cette itération
        private const string LLM_SERVER_URL = "http://192.168.2.108:8040";
        private const string DEFAULT_MODEL = "qwen2.5-14b-instruct";
        private const double DEFAULT_TEMPERATURE = 0.7;
        private const int DEFAULT_MAX_TOKENS = 2048;
        private const int MAX_TOOL_CALLS = 5;
        private const int CONVERSATION_RETENTION_DAYS = 90;

        public UserLlmService()
        {
            _llmClient = new LlmClient(LLM_SERVER_URL);
            _toolsManager = new LlmToolsManager();
            
            var mongoClient = MongoDbHelper.GetClient<LlmConversation>();
            _conversationsCollection = mongoClient.Database.GetCollection<LlmConversation>("llm_conversations");

            RegisterDefaultTools();
        }

        /// <summary>
        /// Enregistre les outils par défaut
        /// </summary>
        private void RegisterDefaultTools()
        {
            // Enregistrement des tools de base
            _toolsManager.RegisterTool(new Tools.GetPresentUsersTool());
            _toolsManager.RegisterTool(new Tools.SendNotificationTool());
            _toolsManager.RegisterTool(new Tools.GetWeatherTool());
            _toolsManager.RegisterTool(new Tools.ListScenesTool());

            LogHelper.Log("llm-service", "tools-registered", $"4 outils enregistrés avec succès");
        }

        /// <summary>
        /// Enregistre un outil supplémentaire
        /// </summary>
        public void RegisterTool(ILlmTool tool)
        {
            _toolsManager.RegisterTool(tool);
        }

        /// <summary>
        /// Envoie un message au LLM et obtient une réponse
        /// </summary>
        public async Task<ChatResponse> ChatAsync(
            string userId,
            string message,
            string conversationId = null,
            Action<ChatProgressNotification> onProgress = null,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("userId est requis", nameof(userId));

            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentException("message est requis", nameof(message));

            // Récupérer ou créer la conversation
            var conversation = await GetOrCreateConversationAsync(userId, conversationId);

            // Ajouter le message utilisateur
            conversation.Messages.Add(new LlmConversationMessage("user", message));
            conversation.UpdatedAt = DateTime.UtcNow;

            try
            {
                onProgress?.Invoke(new ChatProgressNotification
                {
                    Type = "thinking",
                    Message = "Analyse de votre demande..."
                });

                // Préparer les messages pour le LLM
                var llmMessages = PrepareMessagesForLlm(conversation);

                // Appeler le LLM avec support des tools
                var response = await ChatWithToolsAsync(
                    llmMessages,
                    conversation,
                    onProgress,
                    cancellationToken);

                // Ajouter la réponse à la conversation
                conversation.Messages.Add(new LlmConversationMessage("assistant", response.Response));
                conversation.UpdatedAt = DateTime.UtcNow;

                // Générer un sujet si c'est le premier échange
                if (conversation.Messages.Count <= 2 && string.IsNullOrEmpty(conversation.Subject))
                {
                    conversation.Subject = GenerateSubject(message, response.Response);
                }

                // Sauvegarder la conversation
                await SaveConversationAsync(conversation);

                response.ConversationId = conversation.Id;
                return response;
            }
            catch (Exception ex)
            {
                LogHelper.Log("llm-service", "chat-error", $"Erreur lors du chat: {ex.Message}");
                
                return new ChatResponse
                {
                    Response = "Désolé, je rencontre un problème technique. Pourriez-vous réessayer ?",
                    ConversationId = conversation.Id
                };
            }
        }

        /// <summary>
        /// Chat avec support des tools (boucle de traitement)
        /// </summary>
        private async Task<ChatResponse> ChatWithToolsAsync(
            List<LlmChatMessage> messages,
            LlmConversation conversation,
            Action<ChatProgressNotification> onProgress,
            CancellationToken cancellationToken)
        {
            var response = new ChatResponse();
            var toolCallsCount = 0;

            while (toolCallsCount < MAX_TOOL_CALLS)
            {
                // Appeler le LLM
                var llmResponse = await _llmClient.ChatAsync(
                    DEFAULT_MODEL,
                    messages,
                    DEFAULT_TEMPERATURE,
                    DEFAULT_MAX_TOKENS,
                    cancellationToken);

                // Vérifier si le LLM demande d'utiliser un tool
                var toolCall = ExtractToolCall(llmResponse);
                
                if (toolCall == null)
                {
                    // Pas d'appel de tool, c'est la réponse finale
                    response.Response = llmResponse?.Trim() ?? string.Empty;
                    break;
                }

                toolCallsCount++;

                // Notifier le client de l'exécution du tool
                onProgress?.Invoke(new ChatProgressNotification
                {
                    Type = "tool_calling",
                    Message = $"Exécution de l'outil {toolCall.Value.toolName}...",
                    ToolName = toolCall.Value.toolName
                });

                response.ToolsExecuted.Add(toolCall.Value.toolName);

                // Exécuter le tool
                var toolResult = await _toolsManager.ExecuteToolAsync(
                    toolCall.Value.toolName,
                    toolCall.Value.parameters,
                    cancellationToken);

                // Ajouter le résultat dans la conversation
                messages.Add(new LlmChatMessage("assistant", llmResponse));
                messages.Add(new LlmChatMessage("system", $"Résultat de {toolCall.Value.toolName}: {toolResult}"));

                // Enregistrer dans les métadonnées
                if (!conversation.Metadata.ContainsKey("toolCalls"))
                    conversation.Metadata["toolCalls"] = new List<object>();

                ((List<object>)conversation.Metadata["toolCalls"]).Add(new
                {
                    tool = toolCall.Value.toolName,
                    parameters = toolCall.Value.parameters,
                    result = toolResult,
                    timestamp = DateTime.UtcNow
                });
            }

            if (toolCallsCount >= MAX_TOOL_CALLS)
            {
                response.Response = "J'ai effectué plusieurs actions, mais je rencontre une limitation. Puis-je vous aider autrement ?";
            }

            return response;
        }

        /// <summary>
        /// Extrait un appel de tool depuis la réponse du LLM
        /// Format: [TOOL:nom_outil] ou [TOOL:nom_outil|{json_params}]
        /// </summary>
        private (string toolName, Dictionary<string, object> parameters)? ExtractToolCall(string response)
        {
            if (string.IsNullOrWhiteSpace(response))
                return null;

            // Regex pour capturer [TOOL:nom] ou [TOOL:nom|{...}]
            var regex = new Regex(@"\[TOOL:([a-zA-Z0-9_]+)(?:\|(.+?))?\]", RegexOptions.IgnoreCase);
            var match = regex.Match(response);

            if (!match.Success)
                return null;

            var toolName = match.Groups[1].Value;
            var paramsJson = match.Groups.Count > 2 ? match.Groups[2].Value : null;

            Dictionary<string, object> parameters = null;

            if (!string.IsNullOrWhiteSpace(paramsJson))
            {
                try
                {
                    parameters = JsonConvert.DeserializeObject<Dictionary<string, object>>(paramsJson);
                }
                catch
                {
                    // Paramètres invalides, on continue sans paramètres
                }
            }

            return (toolName, parameters);
        }

        /// <summary>
        /// Prépare les messages pour l'envoi au LLM
        /// </summary>
        private List<LlmChatMessage> PrepareMessagesForLlm(LlmConversation conversation)
        {
            var messages = new List<LlmChatMessage>();

            // Message système avec le prompt et la liste des tools
            var systemPrompt = GetSystemPrompt();
            messages.Add(new LlmChatMessage("system", systemPrompt));

            // Ajouter l'historique de la conversation (limité aux derniers messages)
            var recentMessages = conversation.Messages
                .Where(m => m.Role != "system") // Exclure les anciens messages système
                .TakeLast(20) // Garder les 20 derniers messages
                .ToList();

            foreach (var msg in recentMessages)
            {
                messages.Add(new LlmChatMessage(msg.Role, msg.Content, msg.Name));
            }

            return messages;
        }

        /// <summary>
        /// Génère le prompt système
        /// </summary>
        private string GetSystemPrompt()
        {
            return $@"Tu es l'assistant intelligent du système domotique Manoir.

Tu peux :
- Répondre aux questions sur l'état de la maison
- Contrôler les appareils connectés (lumières, thermostats, etc.)
- Exécuter des scènes domotiques
- Gérer le calendrier et les tâches
- Envoyer des notifications aux membres du foyer

{_toolsManager.GetToolsDescriptionForPrompt()}

Ton ton est amical et proactif. 
Tu donnes des réponses claires et actionables.
Langue : Français (sauf demande contraire).";
        }

        /// <summary>
        /// Récupère ou crée une conversation
        /// </summary>
        private async Task<LlmConversation> GetOrCreateConversationAsync(string userId, string conversationId)
        {
            if (!string.IsNullOrEmpty(conversationId))
            {
                var existing = await _conversationsCollection
                    .Find(c => c.Id == conversationId && c.Initiator == userId)
                    .FirstOrDefaultAsync();

                if (existing != null)
                    return existing;
            }

            // Créer une nouvelle conversation
            var conversation = new LlmConversation
            {
                Id = Guid.NewGuid().ToString("N"),
                ConversationType = "user-to-llm",
                Initiator = userId,
                Model = DEFAULT_MODEL,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(CONVERSATION_RETENTION_DAYS)
            };

            return conversation;
        }

        /// <summary>
        /// Sauvegarde une conversation
        /// </summary>
        private async Task SaveConversationAsync(LlmConversation conversation)
        {
            var filter = Builders<LlmConversation>.Filter.Eq(c => c.Id, conversation.Id);
            await _conversationsCollection.ReplaceOneAsync(
                filter,
                conversation,
                new ReplaceOptions { IsUpsert = true });
        }

        /// <summary>
        /// Génère un sujet pour la conversation
        /// </summary>
        private string GenerateSubject(string firstMessage, string firstResponse)
        {
            // Prendre les premiers mots du message utilisateur
            var words = firstMessage.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var subject = string.Join(" ", words.Take(5));
            
            if (subject.Length > 50)
                subject = subject.Substring(0, 47) + "...";

            return subject;
        }

        /// <summary>
        /// Récupère une conversation par son ID
        /// </summary>
        public async Task<LlmConversation> GetConversationAsync(string conversationId, string userId)
        {
            return await _conversationsCollection
                .Find(c => c.Id == conversationId && c.Initiator == userId)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Liste les conversations d'un utilisateur
        /// </summary>
        public async Task<List<ConversationSummary>> GetUserConversationsAsync(string userId, int limit = 20)
        {
            var conversations = await _conversationsCollection
                .Find(c => c.Initiator == userId)
                .SortByDescending(c => c.UpdatedAt)
                .Limit(limit)
                .ToListAsync();

            return conversations.Select(c => new ConversationSummary
            {
                Id = c.Id,
                Subject = c.Subject ?? "Nouvelle conversation",
                LastMessage = c.Messages.LastOrDefault()?.Content ?? "",
                UpdatedAt = c.UpdatedAt,
                MessageCount = c.Messages.Count
            }).ToList();
        }

        /// <summary>
        /// Supprime une conversation
        /// </summary>
        public async Task<bool> DeleteConversationAsync(string conversationId, string userId)
        {
            var result = await _conversationsCollection.DeleteOneAsync(
                c => c.Id == conversationId && c.Initiator == userId);

            return result.DeletedCount > 0;
        }

        /// <summary>
        /// Récupère la liste des modèles disponibles
        /// </summary>
        public async Task<List<string>> GetAvailableModelsAsync()
        {
            try
            {
                return await _llmClient.GetModelsAsync();
            }
            catch
            {
                return new List<string> { DEFAULT_MODEL };
            }
        }
    }
}
