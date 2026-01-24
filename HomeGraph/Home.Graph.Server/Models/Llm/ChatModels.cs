using System;
using System.Collections.Generic;

namespace Home.Graph.Server.Models.Llm
{
    /// <summary>
    /// Requête de chat envoyée par le client
    /// </summary>
    public class ChatRequest
    {
        /// <summary>
        /// Message de l'utilisateur
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// ID de conversation existante (optionnel)
        /// </summary>
        public string ConversationId { get; set; }
    }

    /// <summary>
    /// Réponse de chat renvoyée au client
    /// </summary>
    public class ChatResponse
    {
        /// <summary>
        /// Réponse générée par le LLM
        /// </summary>
        public string Response { get; set; }

        /// <summary>
        /// ID de la conversation
        /// </summary>
        public string ConversationId { get; set; }

        /// <summary>
        /// Outils qui ont été exécutés
        /// </summary>
        public List<string> ToolsExecuted { get; set; }

        /// <summary>
        /// Informations d'utilisation des tokens
        /// </summary>
        public TokenUsageInfo TokenUsage { get; set; }

        public ChatResponse()
        {
            ToolsExecuted = new List<string>();
        }
    }

    /// <summary>
    /// Information sur l'utilisation des tokens
    /// </summary>
    public class TokenUsageInfo
    {
        public int PromptTokens { get; set; }
        public int CompletionTokens { get; set; }
        public int TotalTokens { get; set; }
    }

    /// <summary>
    /// Notification de progression pendant le traitement
    /// </summary>
    public class ChatProgressNotification
    {
        /// <summary>
        /// Type de progression : "thinking", "tool_calling", "responding"
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Message de progression
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Nom de l'outil en cours d'exécution (si applicable)
        /// </summary>
        public string ToolName { get; set; }
    }

    /// <summary>
    /// Résumé d'une conversation
    /// </summary>
    public class ConversationSummary
    {
        public string Id { get; set; }
        public string Subject { get; set; }
        public string LastMessage { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int MessageCount { get; set; }
    }
}
