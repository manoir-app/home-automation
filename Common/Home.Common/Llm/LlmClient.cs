using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Home.Common.Llm
{
    /// <summary>
    /// Client HTTP pour communiquer avec un serveur LLM compatible OpenAI (Lemonade Server)
    /// </summary>
    public class LlmClient : IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private bool _disposed = false;

        /// <summary>
        /// Constructeur du client LLM
        /// </summary>
        /// <param name="baseUrl">URL de base du serveur LLM (ex: "http://192.168.2.108:8040")</param>
        /// <param name="httpClient">Client HTTP optionnel (si null, en crée un nouveau)</param>
        public LlmClient(string baseUrl, HttpClient httpClient = null)
        {
            _baseUrl = baseUrl?.TrimEnd('/') ?? throw new ArgumentNullException(nameof(baseUrl));
            _httpClient = httpClient ?? new HttpClient();
            _httpClient.Timeout = TimeSpan.FromMinutes(5); // Timeout généreux pour la génération
        }

        /// <summary>
        /// Envoie une requête de chat completion au serveur LLM
        /// </summary>
        /// <param name="request">Requête de chat completion</param>
        /// <param name="cancellationToken">Token d'annulation</param>
        /// <returns>Réponse du LLM</returns>
        public async Task<LlmChatCompletionResponse> ChatCompletionAsync(
            LlmChatCompletionRequest request,
            CancellationToken cancellationToken = default)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (string.IsNullOrEmpty(request.Model))
                throw new ArgumentException("Le modèle doit être spécifié", nameof(request));

            if (request.Messages == null || request.Messages.Count == 0)
                throw new ArgumentException("Au moins un message doit être fourni", nameof(request));

            var url = $"{_baseUrl}/api/v1/chat/completions";

            var json = JsonConvert.SerializeObject(request, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync(url, content, cancellationToken);
                
                // Si erreur HTTP, récupérer le corps de la réponse pour diagnostic
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception(
                        $"Erreur HTTP {(int)response.StatusCode} ({response.StatusCode}) lors de la communication avec le serveur LLM à {url}\n" +
                        $"Réponse du serveur : {errorContent}");
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<LlmChatCompletionResponse>(responseContent);
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Erreur lors de la communication avec le serveur LLM à {url}: {ex.Message}", ex);
            }
            catch (TaskCanceledException ex)
            {
                throw new Exception($"La requête au serveur LLM a expiré: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Méthode simplifiée pour envoyer une requête de chat
        /// </summary>
        /// <param name="model">Nom du modèle</param>
        /// <param name="messages">Liste des messages</param>
        /// <param name="temperature">Température optionnelle</param>
        /// <param name="maxTokens">Nombre maximum de tokens</param>
        /// <param name="stopSequences">Séquences d'arrêt (pour stopper avant le "thinking")</param>
        /// <param name="tools">Liste des outils disponibles (function calling)</param>
        /// <param name="cancellationToken">Token d'annulation</param>
        /// <returns>Texte de la réponse générée</returns>
        public async Task<string> ChatAsync(
            string model,
            List<LlmChatMessage> messages,
            double? temperature = null,
            int? maxTokens = null,
            List<string> stopSequences = null,
            List<object> tools = null,
            CancellationToken cancellationToken = default)
        {
            var request = new LlmChatCompletionRequest
            {
                Model = model,
                Messages = messages,
                Temperature = temperature,
                MaxTokens = maxTokens,
                Stop = stopSequences,
                Tools = tools
            };

            var response = await ChatCompletionAsync(request, cancellationToken);

            if (response.Choices == null || response.Choices.Count == 0)
                throw new Exception("Aucune réponse générée par le LLM");

            return response.Choices[0].Message?.Content ?? string.Empty;
        }

        /// <summary>
        /// Récupère la liste des modèles disponibles
        /// </summary>
        /// <param name="cancellationToken">Token d'annulation</param>
        /// <returns>Liste des modèles</returns>
        public async Task<List<string>> GetModelsAsync(CancellationToken cancellationToken = default)
        {
            var url = $"{_baseUrl}/api/v1/models";

            try
            {
                var response = await _httpClient.GetAsync(url, cancellationToken);
                
                // Si erreur HTTP, récupérer le corps de la réponse pour diagnostic
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception(
                        $"Erreur HTTP {(int)response.StatusCode} ({response.StatusCode}) lors de la récupération des modèles à {url}\n" +
                        $"Réponse du serveur : {errorContent}");
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var data = JsonConvert.DeserializeObject<ModelListResponse>(responseContent);

                var models = new List<string>();
                if (data?.Data != null)
                {
                    foreach (var model in data.Data)
                    {
                        if (!string.IsNullOrEmpty(model?.Id))
                            models.Add(model.Id);
                    }
                }

                return models;
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Erreur lors de la récupération des modèles: {ex.Message}", ex);
            }
            catch (TaskCanceledException ex)
            {
                throw new Exception($"La requête de récupération des modèles a expiré: {ex.Message}", ex);
            }
        }

        private class ModelListResponse
        {
            public List<ModelInfo> Data { get; set; }
        }

        private class ModelInfo
        {
            public string Id { get; set; }
            public string Object { get; set; }
            public long? Size { get; set; }
        }

        /// <summary>
        /// Récupère le modèle avec la plus grosse taille (size)
        /// Utile pour sélectionner automatiquement le meilleur modèle disponible
        /// </summary>
        /// <param name="cancellationToken">Token d'annulation</param>
        /// <returns>Nom du modèle le plus gros, ou null si aucun modèle disponible</returns>
        public async Task<string> GetLargestModelAsync(CancellationToken cancellationToken = default)
        {
            var url = $"{_baseUrl}/api/v1/models";

            try
            {
                var response = await _httpClient.GetAsync(url, cancellationToken);
                
                // Si erreur HTTP, récupérer le corps de la réponse pour diagnostic
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception(
                        $"Erreur HTTP {(int)response.StatusCode} ({response.StatusCode}) lors de la récupération du modèle le plus gros à {url}\n" +
                        $"Réponse du serveur : {errorContent}");
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var data = JsonConvert.DeserializeObject<ModelListResponse>(responseContent);

                if (data?.Data == null || data.Data.Count == 0)
                    return null;

                // Trier par size décroissant et prendre le premier
                var largestModel = data.Data
                    .Where(m => !string.IsNullOrEmpty(m?.Id) && m.Size.HasValue)
                    .OrderByDescending(m => m.Size)
                    .FirstOrDefault();

                return largestModel?.Id;
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Erreur lors de la récupération du modèle le plus gros: {ex.Message}", ex);
            }
            catch (TaskCanceledException ex)
            {
                throw new Exception($"La requête de récupération du modèle le plus gros a expiré: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Récupère le modèle avec la plus grosse taille (size)
        /// Utile pour sélectionner automatiquement le meilleur modèle disponible
        /// </summary>
        /// <param name="cancellationToken">Token d'annulation</param>
        /// <returns>Nom du modèle le plus gros, ou null si aucun modèle disponible</returns>
        public async Task<string> GetSmallestModelAsync(CancellationToken cancellationToken = default)
        {
            var url = $"{_baseUrl}/api/v1/models";

            try
            {
                var response = await _httpClient.GetAsync(url, cancellationToken);

                // Si erreur HTTP, récupérer le corps de la réponse pour diagnostic
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception(
                        $"Erreur HTTP {(int)response.StatusCode} ({response.StatusCode}) lors de la récupération du modèle le plus petit à {url}\n" +
                        $"Réponse du serveur : {errorContent}");
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var data = JsonConvert.DeserializeObject<ModelListResponse>(responseContent);

                if (data?.Data == null || data.Data.Count == 0)
                    return null;

                // Trier par size décroissant et prendre le premier
                var largestModel = data.Data
                    .Where(m => !string.IsNullOrEmpty(m?.Id) && m.Size.HasValue)
                    .OrderBy(m => m.Size)
                    .FirstOrDefault();

                return largestModel?.Id;
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Erreur lors de la récupération du modèle le plus petit: {ex.Message}", ex);
            }
            catch (TaskCanceledException ex)
            {
                throw new Exception($"La requête de récupération du modèle le plus petit a expiré: {ex.Message}", ex);
            }
        }


        public void Dispose()
        {
            if (!_disposed)
            {
                _httpClient?.Dispose();
                _disposed = true;
            }
        }
    }
}
