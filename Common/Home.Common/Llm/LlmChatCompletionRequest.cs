using System.Collections.Generic;
using Newtonsoft.Json;

namespace Home.Common.Llm
{
    /// <summary>
    /// Configuration pour une requête de chat completion
    /// </summary>
    public class LlmChatCompletionRequest
    {
        /// <summary>
        /// Modèle à utiliser (ex: "qwen2.5-14b", "llama3.1", etc.)
        /// </summary>
        [JsonProperty("model")]
        public string Model { get; set; }

        /// <summary>
        /// Liste des messages de la conversation
        /// </summary>
        [JsonProperty("messages")]
        public List<LlmChatMessage> Messages { get; set; }

        /// <summary>
        /// Température (0.0 à 2.0, défaut: 0.7)
        /// Plus élevé = plus créatif, plus bas = plus déterministe
        /// </summary>
        [JsonProperty("temperature")]
        public double? Temperature { get; set; }

        /// <summary>
        /// Nombre maximum de tokens à générer
        /// </summary>
        [JsonProperty("max_tokens")]
        public int? MaxTokens { get; set; }

        /// <summary>
        /// Top-p sampling (0.0 à 1.0)
        /// </summary>
        [JsonProperty("top_p")]
        public double? TopP { get; set; }

        /// <summary>
        /// Mode streaming activé
        /// </summary>
        [JsonProperty("stream")]
        public bool? Stream { get; set; }

        /// <summary>
        /// Séquences qui arrêteront la génération si détectées
        /// Utile pour arrêter avant le "thinking" ou les balises internes
        /// </summary>
        [JsonProperty("stop", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> Stop { get; set; }

        /// <summary>
        /// Fréquence penalty (-2.0 à 2.0)
        /// Pénalise les tokens déjà utilisés pour éviter la répétition
        /// </summary>
        [JsonProperty("frequency_penalty", NullValueHandling = NullValueHandling.Ignore)]
        public double? FrequencyPenalty { get; set; }

        /// <summary>
        /// Présence penalty (-2.0 à 2.0)
        /// Encourage le modèle à parler de nouveaux sujets
        /// </summary>
        [JsonProperty("presence_penalty", NullValueHandling = NullValueHandling.Ignore)]
        public double? PresencePenalty { get; set; }

        /// <summary>
        /// Seed pour la génération déterministe (si supporté par le modèle)
        /// </summary>
        [JsonProperty("seed", NullValueHandling = NullValueHandling.Ignore)]
        public int? Seed { get; set; }

        /// <summary>
        /// Format de réponse attendu (ex: "json_object" pour forcer du JSON)
        /// </summary>
        [JsonProperty("response_format", NullValueHandling = NullValueHandling.Ignore)]
        public object ResponseFormat { get; set; }

        /// <summary>
        /// Liste des outils (functions/tools) disponibles pour le LLM
        /// Format OpenAI function calling
        /// </summary>
        [JsonProperty("tools", NullValueHandling = NullValueHandling.Ignore)]
        public List<object> Tools { get; set; }

        /// <summary>
        /// Contrôle comment le modèle doit appeler les functions
        /// "none" = ne jamais appeler, "auto" = décider automatiquement, {"type":"function","function":{"name":"..."}} = forcer un appel
        /// </summary>
        [JsonProperty("tool_choice", NullValueHandling = NullValueHandling.Ignore)]
        public object ToolChoice { get; set; }

        public LlmChatCompletionRequest()
        {
            Messages = new List<LlmChatMessage>();
        }
    }
}
