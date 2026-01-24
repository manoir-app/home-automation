using System;
using System.Collections.Generic;

namespace Home.Common.Model
{
    /// <summary>
    /// Représente une conversation avec un LLM stockée en base de données
    /// </summary>
    public class LlmConversation
    {
        /// <summary>
        /// Identifiant unique de la conversation
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Type de conversation : "agent-to-llm" ou "user-to-llm"
        /// </summary>
        public string ConversationType { get; set; }

        /// <summary>
        /// Nom de l'agent (pour "agent-to-llm") ou nom d'utilisateur (pour "user-to-llm")
        /// </summary>
        public string Initiator { get; set; }

        /// <summary>
        /// Modèle LLM utilisé
        /// </summary>
        public string Model { get; set; }

        /// <summary>
        /// Titre ou sujet de la conversation
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// Messages de la conversation
        /// </summary>
        public List<LlmConversationMessage> Messages { get; set; }

        /// <summary>
        /// Date de création de la conversation
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Date de dernière mise à jour
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Date d'expiration (30 jours pour agent-to-llm, 90 jours pour user-to-llm)
        /// </summary>
        public DateTime ExpiresAt { get; set; }

        /// <summary>
        /// Métadonnées personnalisées
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; }

        public LlmConversation()
        {
            Messages = new List<LlmConversationMessage>();
            Metadata = new Dictionary<string, object>();
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Représente un message dans une conversation LLM stockée
    /// </summary>
    public class LlmConversationMessage
    {
        /// <summary>
        /// Rôle du message : "system", "user", "assistant"
        /// </summary>
        public string Role { get; set; }

        /// <summary>
        /// Contenu du message
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Nom optionnel de l'émetteur
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Timestamp du message
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Informations d'utilisation des tokens (si applicable)
        /// </summary>
        public LlmTokenUsage TokenUsage { get; set; }

        public LlmConversationMessage()
        {
            Timestamp = DateTime.UtcNow;
        }

        public LlmConversationMessage(string role, string content, string name = null)
        {
            Role = role;
            Content = content;
            Name = name;
            Timestamp = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Statistiques d'utilisation des tokens
    /// </summary>
    public class LlmTokenUsage
    {
        public int PromptTokens { get; set; }
        public int CompletionTokens { get; set; }
        public int TotalTokens { get; set; }
    }
}
