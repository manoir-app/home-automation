using Home.Agents.Aurore.Llm.Tools;
using Home.Common.Llm;
using Home.Graph.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Home.Agents.Aurore.Llm
{
    /// <summary>
    /// Service LLM spécialisé pour l'agent Aurore
    /// Gère les conversations avec le LLM avec le contexte d'Aurore
    /// </summary>
    public class AuroreLlmService
    {
        private readonly LlmClient _llmClient;
        private readonly AuroreLlmToolsManager _toolsManager;
        private string _defaultModel = "qwen2.5-14b-instruct"; // Fallback
        
        // Prompt système qui définit le rôle d'Aurore
        private string SystemPrompt => @"Tu es Aurore, un agent automatisé du système domotique Manoir.

CONTEXTE :
- Tu N'ES PAS un chatbot conversationnel
- Tu es un AGENT AUTOMATIQUE qui fournit des informations factuelles
- Tu NE dois PAS engager la conversation ni poser de questions

RÈGLES ABSOLUES :
1. UNE SEULE phrase courte qui répond à la question
2. SANS balises, SANS réflexion, SANS liste à puces
3. NE JAMAIS dire 'Si vous avez besoin', 'N'hésitez pas', 'Voici les'
4. Format de tool : [TOOL:nom_outil]

EXEMPLES CORRECTS :
Q: Qui est présent ?
R: Michael CARBENAY et Mégane WAROQUIER sont présents.

Q: Combien de personnes ?
R: 3 personnes sont présentes.

EXEMPLES INCORRECTS (ne jamais faire) :
MAUVAIS : Les utilisateurs présents sont : -

 Michael - Mégane
MAUVAIS : Voici la liste...
MAUVAIS : Si vous avez besoin...

RAPPEL : UNE phrase simple et directe. RIEN D'AUTRE.";

        public AuroreLlmService()
        {
            // URL du serveur LLM (Lemonade Server)
            string llmBaseUrl = "http://192.168.2.108:8040";
            _llmClient = new LlmClient(llmBaseUrl);
            _toolsManager = new AuroreLlmToolsManager();
            
            // Récupérer le meilleur modèle disponible de façon asynchrone
            InitializeDefaultModelAsync().GetAwaiter().GetResult();
        }

        /// <summary>
        /// Initialise le modèle par défaut en récupérant le plus gros modèle disponible
        /// </summary>
        private async Task InitializeDefaultModelAsync()
        {
            try
            {
                // Récupérer le plus gros modèle disponible (configuré côté serveur LLM)
                var largestModel = await _llmClient.GetSmallestModelAsync();
                if (!string.IsNullOrEmpty(largestModel))
                {
                    _defaultModel = largestModel;
                    Console.WriteLine($"Aurore LLM : Modèle sélectionné automatiquement : {_defaultModel}");
                }
                else
                {
                    Console.WriteLine($"Aurore LLM : Aucun modèle trouvé, utilisation du fallback : {_defaultModel}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Aurore LLM : Erreur lors de la récupération du modèle, utilisation du fallback : {_defaultModel}");
                Console.WriteLine($"Erreur : {ex.Message}");
            }
        }

        /// <summary>
        /// Accès au gestionnaire d'outils
        /// </summary>
        public AuroreLlmToolsManager Tools => _toolsManager;

        /// <summary>
        /// Modèle actuellement utilisé
        /// </summary>
        public string DefaultModel => _defaultModel;

        /// <summary>
        /// Génère un message personnalisé pour un utilisateur
        /// </summary>
        /// <param name="userName">Nom de l'utilisateur</param>
        /// <param name="context">Contexte du message</param>
        /// <param name="tone">Ton souhaité (optionnel)</param>
        /// <returns>Message généré</returns>
        public async Task<string> GeneratePersonalizedMessageAsync(
            string userName, 
            string context, 
            string tone = "amical",
            CancellationToken cancellationToken = default)
        {
            var messages = new List<LlmChatMessage>
            {
                new LlmChatMessage("system", SystemPrompt),
                new LlmChatMessage("user", $@"Génère un message {tone} pour {userName}.

Contexte : {context}

Génère uniquement le message, sans introduction ni explication.")
            };

            try
            {
                var response = await _llmClient.ChatAsync(
                    _defaultModel, 
                    messages, 
                    temperature: 0.7,
                    cancellationToken: cancellationToken);

                return response?.Trim() ?? string.Empty;
            }
            catch (Exception ex)
            {
                LogHelper.LogException("aurore", "llm-generation", ex);
                return $"Bonjour {userName}, {context}"; // Fallback basique
            }
        }

        /// <summary>
        /// Résume une liste d'actualités
        /// </summary>
        /// <param name="newsItems">Liste des actualités</param>
        /// <param name="maxLength">Longueur maximale du résumé en caractères</param>
        /// <returns>Résumé des actualités</returns>
        public async Task<string> SummarizeNewsAsync(
            List<string> newsItems, 
            int maxLength = 500,
            CancellationToken cancellationToken = default)
        {
            if (newsItems == null || newsItems.Count == 0)
                return string.Empty;

            var newsText = string.Join("\n- ", newsItems);

            var messages = new List<LlmChatMessage>
            {
                new LlmChatMessage("system", SystemPrompt),
                new LlmChatMessage("user", $@"Résume les actualités suivantes en {maxLength} caractères maximum :

{newsText}

Crée un résumé concis et informatif, en français.")
            };

            try
            {
                var response = await _llmClient.ChatAsync(
                    _defaultModel, 
                    messages, 
                    temperature: 0.5,
                    maxTokens: maxLength / 2, // Approximation tokens/caractères
                    cancellationToken: cancellationToken);

                return response?.Trim() ?? string.Empty;
            }
            catch (Exception ex)
            {
                LogHelper.LogException("aurore", "llm-summarize", ex);
                return "Actualités disponibles."; // Fallback
            }
        }

        /// <summary>
        /// Génère une salutation personnalisée selon le contexte
        /// </summary>
        /// <param name="userName">Nom de l'utilisateur</param>
        /// <param name="timeOfDay">Moment de la journée (matin, après-midi, soir)</param>
        /// <param name="context">Contexte additionnel (météo, événements, etc.)</param>
        /// <returns>Salutation générée</returns>
        public async Task<string> GenerateGreetingAsync(
            string userName,
            string timeOfDay,
            string context = null,
            CancellationToken cancellationToken = default)
        {
            var contextPart = string.IsNullOrEmpty(context) ? "" : $"\nContexte : {context}";

            var messages = new List<LlmChatMessage>
            {
                new LlmChatMessage("system", SystemPrompt),
                new LlmChatMessage("user", $@"Génère une salutation chaleureuse pour {userName}.

Moment : {timeOfDay}{contextPart}

Génère uniquement la salutation, courte et naturelle (1-2 phrases maximum).")
            };

            try
            {
                var response = await _llmClient.ChatAsync(
                    _defaultModel, 
                    messages, 
                    temperature: 0.8,
                    maxTokens: 100,
                    cancellationToken: cancellationToken);

                return response?.Trim() ?? string.Empty;
            }
            catch (Exception ex)
            {
                LogHelper.LogException("aurore", "llm-greeting", ex);
                
                // Fallback selon le moment de la journée
                return timeOfDay.ToLower() switch
                {
                    "matin" => $"Bonjour {userName} !",
                    "après-midi" => $"Bon après-midi {userName} !",
                    "soir" => $"Bonsoir {userName} !",
                    _ => $"Bonjour {userName} !"
                };
            }
        }

        /// <summary>
        /// Conversation libre avec le LLM (pour des cas d'usage personnalisés)
        /// </summary>
        /// <param name="userMessage">Message de l'utilisateur</param>
        /// <param name="conversationHistory">Historique de la conversation (optionnel)</param>
        /// <returns>Réponse du LLM</returns>
        public async Task<string> ChatAsync(
            string userMessage,
            List<LlmChatMessage> conversationHistory = null,
            CancellationToken cancellationToken = default)
        {
            var messages = new List<LlmChatMessage>
            {
                new LlmChatMessage("system", SystemPrompt)
            };

            // Ajouter l'historique si fourni
            if (conversationHistory != null && conversationHistory.Count > 0)
            {
                messages.AddRange(conversationHistory);
            }

            // Ajouter le message actuel
            messages.Add(new LlmChatMessage("user", userMessage));

            try
            {
                var response = await _llmClient.ChatAsync(
                    _defaultModel, 
                    messages, 
                    temperature: 0.7,
                    cancellationToken: cancellationToken);

                return response?.Trim() ?? string.Empty;
            }
            catch (Exception ex)
            {
                LogHelper.LogException("aurore", "llm-chat", ex);
                return "Je suis désolée, je rencontre un problème technique.";
            }
        }

        /// <summary>
        /// Conversation avec appel d'outils (tools) - Mode avancé
        /// Le LLM peut demander à utiliser des tools, qui seront exécutés, puis la réponse sera fournie au LLM
        /// </summary>
        /// <param name="userMessage">Message de l'utilisateur</param>
        /// <param name="conversationHistory">Historique de la conversation (optionnel)</param>
        /// <param name="maxToolCalls">Nombre maximum d'appels d'outils (pour éviter les boucles infinies)</param>
        /// <returns>Réponse finale du LLM après exécution des outils</returns>
        public async Task<string> ChatWithToolsAsync(
            string userMessage,
            List<LlmChatMessage> conversationHistory = null,
            int maxToolCalls = 5,
            CancellationToken cancellationToken = default)
        {
            var messages = new List<LlmChatMessage>
            {
                new LlmChatMessage("system", SystemPrompt)
            };

            // Ajouter l'historique si fourni
            if (conversationHistory != null && conversationHistory.Count > 0)
            {
                messages.AddRange(conversationHistory);
            }

            // Ajouter le message actuel
            messages.Add(new LlmChatMessage("user", userMessage));

            // Stocker la question initiale pour la rappeler après les tool calls
            string initialUserMessage = userMessage;

            int toolCallsCount = 0;

            while (toolCallsCount < maxToolCalls)
            {
                try
                {
                    // Séquences d'arrêt pour éviter le "thinking" ET les formats interdits
                    var stopSequences = new List<string>
                    {
                        "<think>",        // Bloque la réflexion interne
                        "<|start|>",      // Début de réflexion interne (Gemma)
                        "<|channel|>",    // Canaux de pensée
                        "assistant<|",    // Début de métadonnées
                        "\n-",            // Bloque les listes à puces avec tiret
                        "\n*",            // Bloque les listes à puces avec astérisque
                        "\n1.",           // Bloque les listes numérotées
                        "Si vous",        // Bloque "Si vous avez besoin..."
                        "N'hésitez",      // Bloque "N'hésitez pas..."
                        "Voici",          // Bloque "Voici les..."
                    };

                    // Récupérer le schéma des tools au format OpenAI
                    var toolsSchema = _toolsManager.GetToolsSchemaForApi();
                    
                    Console.WriteLine($"[DEBUG] Envoi de {toolsSchema?.Count ?? 0} tools au LLM (itération {toolCallsCount + 1}/{maxToolCalls})");

                    // Appeler le LLM avec une température plus basse pour les appels de tools
                    // (plus déterministe, moins de hallucinations)
                    var response = await _llmClient.ChatAsync(
                        _defaultModel,
                        messages,
                        temperature: 0.1, // TRÈS bas = maximum de déterminisme
                        maxTokens: 50,    // Limite stricte pour forcer la concision
                        stopSequences: stopSequences,
                        tools: toolsSchema,
                        cancellationToken: cancellationToken);

                    Console.WriteLine($"LLM Response (raw): {response}");

                    // Nettoyer les balises techniques si présentes (certains modèles les génèrent quand même)
                    response = CleanLlmResponse(response);
                    
                    Console.WriteLine($"LLM Response (cleaned): {response}");

                    // Vérifier si le LLM demande à utiliser un outil
                    // Format attendu : [TOOL:nom_outil|{paramètres}]
                    if (response.Contains("[TOOL:") && response.Contains("]"))
                    {
                        var toolCall = ExtractToolCall(response);
                        if (toolCall != null)
                        {
                            toolCallsCount++;

                            Console.WriteLine($"Tool Call Detected: {toolCall.Value.toolName}");

                            // Exécuter l'outil et obtenir le résultat en JSON
                            var toolResultJson = await _toolsManager.ExecuteToolAsync(
                                toolCall.Value.toolName,
                                Newtonsoft.Json.JsonConvert.SerializeObject(toolCall.Value.parameters),
                                cancellationToken);

                            Console.WriteLine($"Tool Result (JSON): {toolResultJson}");

                            // Ajouter la réponse du LLM et le résultat de l'outil à l'historique
                            messages.Add(new LlmChatMessage("assistant", response));
                            
                            // Message USER (pas system !) avec le résultat du tool + contexte
                            // Gemma/Qwen imposent une alternance stricte user/assistant
                            messages.Add(new LlmChatMessage("user", 
                                $"Résultat de l'outil {toolCall.Value.toolName} :\n```json\n{toolResultJson}\n```\n\n" +
                                $"Utilise ces données pour répondre à ma question : {initialUserMessage}"));

                            // Continuer la boucle pour obtenir une réponse finale
                            continue;
                        }
                    }

                    // Pas d'appel d'outil détecté, retourner la réponse
                    return response?.Trim() ?? string.Empty;
                }
                catch (Exception ex)
                {
                    LogHelper.LogException("aurore", "llm-chat-tools", ex);
                    Console.WriteLine($"Error in ChatWithToolsAsync: {ex.Message}");
                    Console.WriteLine($"StackTrace: {ex.StackTrace}");
                    return "Je suis désolée, je rencontre un problème technique.";
                }
            }

            return "J'ai atteint la limite d'utilisation des outils. Pouvez-vous reformuler votre demande ?";
        }

        /// <summary>
        /// Nettoie la réponse du LLM en supprimant les balises techniques internes
        /// </summary>
        private string CleanLlmResponse(string response)
        {
            if (string.IsNullOrEmpty(response))
                return response;

            // Certains modèles (Gemma, GPT-OSS, Qwen, etc.) exposent leur réflexion interne avec des balises
            
            // 1. Supprimer les blocs <think>...</think> (réflexion interne)
            response = System.Text.RegularExpressions.Regex.Replace(
                response,
                @"<think>.*?</think>",
                string.Empty,
                System.Text.RegularExpressions.RegexOptions.Singleline | System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            
            // 2. Essayer d'extraire uniquement la partie "final message" pour Gemma/GPT-OSS
            var finalMessageMatch = System.Text.RegularExpressions.Regex.Match(
                response,
                @"<\|channel\|>final<\|message\|>(.+?)(?:<\|end\|>|$)",
                System.Text.RegularExpressions.RegexOptions.Singleline);

            if (finalMessageMatch.Success)
            {
                return finalMessageMatch.Groups[1].Value.Trim();
            }

            // 3. Supprimer toutes les balises <|...|>
            var cleanedFromTags = System.Text.RegularExpressions.Regex.Replace(
                response,
                @"<\|[^|]+\|>",
                string.Empty);

            // 4. Si le texte contient toujours des artefacts de réflexion, essayer autre chose
            if (cleanedFromTags.Contains("assistant") && cleanedFromTags.Contains("analysis"))
            {
                // Extraire tout après le dernier "final" ou "message"
                var lines = cleanedFromTags.Split('\n');
                var finalContent = new System.Text.StringBuilder();
                bool inFinalSection = false;

                foreach (var line in lines)
                {
                    if (line.Contains("final") || line.Contains("commentary"))
                    {
                        inFinalSection = true;
                        continue;
                    }

                    if (inFinalSection && !string.IsNullOrWhiteSpace(line))
                    {
                        // Ignorer les lignes avec des métadonnées
                        if (!line.Contains("channel") && 
                            !line.Contains("constrain") && 
                            !line.Contains("call") &&
                            !line.Contains("analysis"))
                        {
                            finalContent.AppendLine(line);
                        }
                    }
                }

                var extracted = finalContent.ToString().Trim();
                if (!string.IsNullOrEmpty(extracted))
                    return extracted;
            }

            // 5. Fallback : retourner le texte nettoyé des balises et espaces multiples
            cleanedFromTags = System.Text.RegularExpressions.Regex.Replace(cleanedFromTags, @"\s+", " ");
            return cleanedFromTags.Trim();
        }

        /// <summary>
        /// Extrait un appel d'outil depuis la réponse du LLM
        /// Format : [TOOL:nom_outil|{json_params}] ou [TOOL:nom_outil]
        /// </summary>
        private (string toolName, Dictionary<string, object> parameters)? ExtractToolCall(string response)
        {
            try
            {
                int start = response.IndexOf("[TOOL:");
                int end = response.IndexOf("]", start);

                if (start == -1 || end == -1)
                    return null;

                string toolContent = response.Substring(start + 6, end - start - 6);
                var parts = toolContent.Split('|');

                string toolName = parts[0].Trim();
                Dictionary<string, object> parameters = new Dictionary<string, object>();

                if (parts.Length > 1 && !string.IsNullOrWhiteSpace(parts[1]))
                {
                    parameters = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(parts[1]);
                }

                return (toolName, parameters);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Récupère la liste des modèles LLM disponibles
        /// </summary>
        public async Task<List<string>> GetAvailableModelsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await _llmClient.GetModelsAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                LogHelper.LogException("aurore", "llm-models", ex);
                return new List<string>();
            }
        }
    }
}
