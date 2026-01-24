using System.Collections.Generic;

namespace Home.Common.Llm
{
    /// <summary>
    /// Réponse d'une requête de chat completion
    /// </summary>
    public class LlmChatCompletionResponse
    {
        /// <summary>
        /// ID unique de la réponse
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Type d'objet ("chat.completion")
        /// </summary>
        public string Object { get; set; }

        /// <summary>
        /// Timestamp de création
        /// </summary>
        public long Created { get; set; }

        /// <summary>
        /// Modèle utilisé
        /// </summary>
        public string Model { get; set; }

        /// <summary>
        /// Choix de réponses générées
        /// </summary>
        public List<LlmChoice> Choices { get; set; }

        /// <summary>
        /// Informations d'utilisation des tokens
        /// </summary>
        public LlmUsage Usage { get; set; }

        public LlmChatCompletionResponse()
        {
            Choices = new List<LlmChoice>();
        }
    }

    public class LlmChoice
    {
        /// <summary>
        /// Index du choix
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// Message généré
        /// </summary>
        public LlmChatMessage Message { get; set; }

        /// <summary>
        /// Raison de l'arrêt de génération ("stop", "length", etc.)
        /// </summary>
        public string FinishReason { get; set; }
    }

    public class LlmUsage
    {
        /// <summary>
        /// Nombre de tokens du prompt
        /// </summary>
        public int PromptTokens { get; set; }

        /// <summary>
        /// Nombre de tokens générés
        /// </summary>
        public int CompletionTokens { get; set; }

        /// <summary>
        /// Total de tokens utilisés
        /// </summary>
        public int TotalTokens { get; set; }
    }
}
