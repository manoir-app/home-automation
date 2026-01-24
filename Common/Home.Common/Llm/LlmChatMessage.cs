using System;
using Newtonsoft.Json;

namespace Home.Common.Llm
{
    /// <summary>
    /// Représente un message dans une conversation avec un LLM
    /// </summary>
    public class LlmChatMessage
    {
        /// <summary>
        /// Rôle du message : "system", "user", "assistant"
        /// </summary>
        [JsonProperty("role")]
        public string Role { get; set; }

        /// <summary>
        /// Contenu du message
        /// </summary>
        [JsonProperty("content")]
        public string Content { get; set; }

        /// <summary>
        /// Nom optionnel de l'émetteur (utile pour différencier plusieurs agents)
        /// </summary>
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        public LlmChatMessage()
        {
        }

        public LlmChatMessage(string role, string content, string name = null)
        {
            Role = role ?? throw new ArgumentNullException(nameof(role));
            Content = content ?? throw new ArgumentNullException(nameof(content));
            Name = name;
        }
    }
}
