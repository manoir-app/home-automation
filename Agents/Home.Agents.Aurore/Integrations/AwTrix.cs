using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Home.Agents.Aurore.Integrations
{
    /// <summary>
    /// Client pour l'intégration avec Awtrix 3
    /// Documentation API: https://blueforcer.github.io/awtrix3/
    /// </summary>
    public class AwTrix : IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = false
        };

        public AwTrix(string ipAddress, int port = 80)
        {
            _baseUrl = $"http://{ipAddress}:{port}";
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(_baseUrl),
                Timeout = TimeSpan.FromSeconds(10)
            };
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Home-Automation-Agent");
        }

        #region Notify API

        /// <summary>
        /// Envoie une notification à Awtrix
        /// </summary>
        public async Task<bool> SendNotificationAsync(AwtrixNotification notification, CancellationToken cancellationToken = default)
        {
            try
            {
                var json = JsonSerializer.Serialize(notification, _jsonOptions);
                Console.WriteLine($"[Awtrix] Envoi notification: {json}");
                
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("/api/notify", content, cancellationToken);
                
                if (!response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
                    Console.WriteLine($"[Awtrix] Erreur HTTP {response.StatusCode}: {responseBody}");
                }
                
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Awtrix] Exception lors de l'envoi de la notification: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[Awtrix] Inner exception: {ex.InnerException.Message}");
                }
                return false;
            }
        }

        /// <summary>
        /// Envoie un message texte simple
        /// </summary>
        public async Task<bool> SendTextAsync(string text, int? duration = null, CancellationToken cancellationToken = default)
        {
            var notification = new AwtrixNotification
            {
                Text = text,
                Duration = duration
            };
            return await SendNotificationAsync(notification, cancellationToken);
        }

        /// <summary>
        /// Envoie un message avec une icône
        /// </summary>
        public async Task<bool> SendTextWithIconAsync(string text, string icon, int? duration = null, CancellationToken cancellationToken = default)
        {
            var notification = new AwtrixNotification
            {
                Text = text,
                Icon = icon,
                Duration = duration
            };
            return await SendNotificationAsync(notification, cancellationToken);
        }

        #endregion

        #region Custom App API

        /// <summary>
        /// Crée ou met à jour une application personnalisée
        /// </summary>
        public async Task<bool> SetCustomAppAsync(string appName, AwtrixCustomApp app, CancellationToken cancellationToken = default)
        {
            try
            {
                var json = JsonSerializer.Serialize(app, _jsonOptions);
                Console.WriteLine($"[Awtrix] Envoi custom app '{appName}': {json.Substring(0, Math.Min(200, json.Length))}...");
                
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"/api/custom?name={appName}", content, cancellationToken);
                
                if (!response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
                    Console.WriteLine($"[Awtrix] Erreur HTTP {response.StatusCode}: {responseBody}");
                }
                else
                {
                    Console.WriteLine($"[Awtrix] Custom app '{appName}' créée avec succès");
                }
                
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"[Awtrix] Erreur HTTP lors de la création de l'app: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[Awtrix] Inner exception: {ex.InnerException.Message}");
                }
                return false;
            }
            catch (TaskCanceledException ex)
            {
                Console.WriteLine($"[Awtrix] Timeout lors de la création de l'app: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Awtrix] Exception lors de la création de l'app: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[Awtrix] Inner exception: {ex.InnerException.Message}");
                }
                return false;
            }
        }

        /// <summary>
        /// Supprime une application personnalisée
        /// </summary>
        public async Task<bool> DeleteCustomAppAsync(string appName, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"/api/custom?name={appName}", cancellationToken);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Awtrix] Erreur lors de la suppression de l'app: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region Autres commandes

        /// <summary>
        /// Active ou désactive l'écran
        /// </summary>
        public async Task<bool> SetPowerAsync(bool on, CancellationToken cancellationToken = default)
        {
            try
            {
                var json = JsonSerializer.Serialize(new { power = on }, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("/api/power", content, cancellationToken);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Awtrix] Erreur lors du changement d'état de l'écran: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Passe à l'application suivante
        /// </summary>
        public async Task<bool> NextAppAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.PostAsync("/api/nextapp", null, cancellationToken);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Awtrix] Erreur lors du passage à l'app suivante: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Passe à l'application précédente
        /// </summary>
        public async Task<bool> PreviousAppAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.PostAsync("/api/previousapp", null, cancellationToken);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Awtrix] Erreur lors du passage à l'app précédente: {ex.Message}");
                return false;
            }
        }

        #endregion

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }

    #region Modèles de données

    /// <summary>
    /// Modèle pour une notification Awtrix
    /// </summary>
    public class AwtrixNotification
    {
        /// <summary>
        /// Le texte à afficher
        /// </summary>
        [JsonPropertyName("text")]
        public string Text { get; set; }

        /// <summary>
        /// Sets an offset for the x position of a starting text
        /// </summary>
        [JsonPropertyName("textOffset")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? TextOffset { get; set; }

        /// <summary>
        /// ID de l'icône (ex: "1234") ou URL
        /// </summary>
        [JsonPropertyName("icon")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Icon { get; set; }

        /// <summary>
        /// Couleur du texte au format hexadécimal (ex: "#FF0000")
        /// </summary>
        [JsonPropertyName("color")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Color { get; set; }

        /// <summary>
        /// Déplace le texte de X pixels
        /// </summary>
        [JsonPropertyName("pushIcon")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? PushIcon { get; set; }

        /// <summary>
        /// Nombre de répétitions (0 = infini)
        /// </summary>
        [JsonPropertyName("repeat")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? Repeat { get; set; }

        /// <summary>
        /// Durée d'affichage en secondes
        /// </summary>
        [JsonPropertyName("duration")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? Duration { get; set; }

        /// <summary>
        /// Maintient la notification même après le changement d'app
        /// </summary>
        [JsonPropertyName("hold")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? Hold { get; set; }

        /// <summary>
        /// Joue un son RTTTL
        /// </summary>
        [JsonPropertyName("sound")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Sound { get; set; }

        /// <summary>
        /// Active l'indicateur de progression (0-100)
        /// </summary>
        [JsonPropertyName("progress")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? Progress { get; set; }

        /// <summary>
        /// Couleur de la barre de progression
        /// </summary>
        [JsonPropertyName("progressC")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string ProgressColor { get; set; }

        /// <summary>
        /// Couleur de fond de la barre de progression
        /// </summary>
        [JsonPropertyName("progressBC")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string ProgressBackgroundColor { get; set; }

        /// <summary>
        /// Active l'arc-en-ciel sur le texte
        /// </summary>
        [JsonPropertyName("rainbow")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? Rainbow { get; set; }

        /// <summary>
        /// Vitesse de défilement du texte en millisecondes (défaut: 100)
        /// </summary>
        [JsonPropertyName("scrollSpeed")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? ScrollSpeed { get; set; }

        /// <summary>
        /// Lifetime of the app
        /// </summary>
        [JsonPropertyName("lifetime")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? Lifetime { get; set; }
    }

    /// <summary>
    /// Modèle pour une application personnalisée Awtrix
    /// </summary>
    public class AwtrixCustomApp
    {
        /// <summary>
        /// Le texte à afficher
        /// </summary>
        [JsonPropertyName("text")]
        public string Text { get; set; }

        /// <summary>
        /// ID de l'icône ou URL
        /// </summary>
        [JsonPropertyName("icon")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Icon { get; set; }

        /// <summary>
        /// Couleur du texte
        /// </summary>
        [JsonPropertyName("color")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Color { get; set; }

        /// <summary>
        /// Sets an offset for the x position of a starting text
        /// </summary>
        [JsonPropertyName("textOffset")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? TextOffset { get; set; }

        /// <summary>
        /// Centers a short, non-scrollable text
        /// </summary>
        [JsonPropertyName("center")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? Center { get; set; }

        /// <summary>
        /// 0 = Icon doesn't move. 1 = Icon moves with text and will not appear again. 
        /// 2 = Icon moves with text but appears again when the text starts to scroll again
        /// </summary>
        [JsonPropertyName("pushIcon")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? PushIcon { get; set; }

        /// <summary>
        /// Nombre de répétitions (-1 = infini)
        /// </summary>
        [JsonPropertyName("repeat")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? Repeat { get; set; }

        /// <summary>
        /// Durée d'affichage en secondes (0 = utilise le réglage global)
        /// </summary>
        [JsonPropertyName("duration")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? Duration { get; set; }

        /// <summary>
        /// Dessin personnalisé (tableau d'entiers RGB)
        /// </summary>
        [JsonPropertyName("draw")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<DrawCommand> Draw { get; set; }

        /// <summary>
        /// Active l'arc-en-ciel
        /// </summary>
        [JsonPropertyName("rainbow")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? Rainbow { get; set; }

        /// <summary>
        /// Vitesse de défilement
        /// </summary>
        [JsonPropertyName("scrollSpeed")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? ScrollSpeed { get; set; }

        /// <summary>
        /// Active la barre de progression (0-100)
        /// </summary>
        [JsonPropertyName("progress")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? Progress { get; set; }

        /// <summary>
        /// Couleur de la barre de progression
        /// </summary>
        [JsonPropertyName("progressC")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string ProgressColor { get; set; }

        /// <summary>
        /// Couleur de fond de la barre de progression
        /// </summary>
        [JsonPropertyName("progressBC")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string ProgressBackgroundColor { get; set; }

        /// <summary>
        /// Disables the text scrolling
        /// </summary>
        [JsonPropertyName("noScroll")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? NoScroll { get; set; }

        /// <summary>
        /// Lifetime of the app
        /// </summary>
        [JsonPropertyName("lifetime")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? Lifetime { get; set; }

    }

    /// <summary>
    /// Commande de dessin pour les applications personnalisées
    /// </summary>
    public class DrawCommand
    {
        [JsonPropertyName("x")]
        public int X { get; set; }

        [JsonPropertyName("y")]
        public int Y { get; set; }

        [JsonPropertyName("c")]
        public string Color { get; set; }

        [JsonPropertyName("s")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Text { get; set; }
    }

    #endregion
}
