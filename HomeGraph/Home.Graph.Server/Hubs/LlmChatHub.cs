using Home.Graph.Server.Models.Llm;
using Home.Graph.Server.Services.Llm;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace Home.Graph.Server.Hubs
{
    /// <summary>
    /// Hub SignalR pour la communication en temps réel avec le LLM
    /// </summary>
    [Authorize(Roles = "User,Admin")]
    public class LlmChatHub : Hub
    {
        private readonly UserLlmService _llmService;

        public LlmChatHub(UserLlmService llmService)
        {
            _llmService = llmService;
        }

        /// <summary>
        /// Envoie un message au LLM et reçoit la réponse
        /// </summary>
        /// <param name="request">Requête de chat</param>
        public async Task SendMessage(ChatRequest request)
        {
            var userId = Context.User?.Identity?.Name;
            
            if (string.IsNullOrEmpty(userId))
            {
                await Clients.Caller.SendAsync("Error", new
                {
                    message = "Utilisateur non identifié"
                });
                return;
            }

            try
            {
                // Notifier que le traitement commence
                await Clients.Caller.SendAsync("ChatStarted", new
                {
                    conversationId = request.ConversationId
                });

                // Appeler le service LLM avec callback de progression
                var response = await _llmService.ChatAsync(
                    userId,
                    request.Message,
                    request.ConversationId,
                    async (progress) =>
                    {
                        // Envoyer les notifications de progression au client
                        await Clients.Caller.SendAsync("ChatProgress", progress);
                    });

                // Envoyer la réponse finale
                await Clients.Caller.SendAsync("ChatResponse", response);
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("Error", new
                {
                    message = $"Erreur lors du traitement: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Récupère l'historique d'une conversation
        /// </summary>
        public async Task GetConversation(string conversationId)
        {
            var userId = Context.User?.Identity?.Name;

            if (string.IsNullOrEmpty(userId))
            {
                await Clients.Caller.SendAsync("Error", new { message = "Utilisateur non identifié" });
                return;
            }

            try
            {
                var conversation = await _llmService.GetConversationAsync(conversationId, userId);
                
                if (conversation == null)
                {
                    await Clients.Caller.SendAsync("Error", new { message = "Conversation introuvable" });
                    return;
                }

                await Clients.Caller.SendAsync("ConversationLoaded", conversation);
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("Error", new { message = ex.Message });
            }
        }

        /// <summary>
        /// Liste les conversations de l'utilisateur
        /// </summary>
        public async Task GetConversations(int limit = 20)
        {
            var userId = Context.User?.Identity?.Name;

            if (string.IsNullOrEmpty(userId))
            {
                await Clients.Caller.SendAsync("Error", new { message = "Utilisateur non identifié" });
                return;
            }

            try
            {
                var conversations = await _llmService.GetUserConversationsAsync(userId, limit);
                await Clients.Caller.SendAsync("ConversationsList", conversations);
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("Error", new { message = ex.Message });
            }
        }

        /// <summary>
        /// Supprime une conversation
        /// </summary>
        public async Task DeleteConversation(string conversationId)
        {
            var userId = Context.User?.Identity?.Name;

            if (string.IsNullOrEmpty(userId))
            {
                await Clients.Caller.SendAsync("Error", new { message = "Utilisateur non identifié" });
                return;
            }

            try
            {
                var deleted = await _llmService.DeleteConversationAsync(conversationId, userId);
                
                await Clients.Caller.SendAsync("ConversationDeleted", new
                {
                    conversationId = conversationId,
                    success = deleted
                });
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("Error", new { message = ex.Message });
            }
        }

        /// <summary>
        /// Récupère la liste des modèles disponibles
        /// </summary>
        public async Task GetAvailableModels()
        {
            try
            {
                var models = await _llmService.GetAvailableModelsAsync();
                await Clients.Caller.SendAsync("ModelsList", models);
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("Error", new { message = ex.Message });
            }
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.Identity?.Name;
            await Clients.Caller.SendAsync("Connected", new
            {
                userId = userId,
                connectionId = Context.ConnectionId
            });

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await base.OnDisconnectedAsync(exception);
        }
    }
}
