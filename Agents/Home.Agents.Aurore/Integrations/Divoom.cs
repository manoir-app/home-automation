using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Home.Agents.Aurore.Integrations
{
    /// <summary>
    /// Client pour l'intégration avec Divoom Pixoo-64
    /// Documentation API: https://github.com/RomRider/node-divoom-timebox-evo
    /// </summary>
    public class Divoom : IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = false
        };

        public Divoom(string ipAddress, int port = 80)
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

        #region Channel Management

        /// <summary>
        /// Sélectionne un canal d'affichage
        /// </summary>
        public async Task<bool> SelectChannelAsync(DivoomChannel channel, CancellationToken cancellationToken = default)
        {
            try
            {
                var payload = new
                {
                    Command = "Channel/SetIndex",
                    SelectIndex = (int)channel
                };

                return await SendCommandAsync(payload, cancellationToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Divoom] Erreur lors de la sélection du canal: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region Text & Animation

        /// <summary>
        /// Envoie un texte à afficher
        /// </summary>
        public async Task<bool> SendTextAsync(string text, DivoomTextSettings settings = null, CancellationToken cancellationToken = default)
        {
            try
            {
                settings = settings ?? new DivoomTextSettings();

                // Convertir la couleur hex en RGB décimal
                var color = ParseHexColor(settings.Color);

                var payload = new
                {
                    Command = "Draw/SendHttpText",
                    TextId = settings.TextId,
                    x = settings.X,
                    y = settings.Y,
                    dir = (int)settings.Direction,
                    font = settings.Font,
                    TextWidth = settings.TextWidth,
                    TextString = text,
                    speed = settings.ScrollSpeed,
                    color = $"#{color.R:X2}{color.G:X2}{color.B:X2}",
                    align = (int)settings.Alignment
                };

                Console.WriteLine($"[Divoom] Envoi texte: '{text}' à ({settings.X},{settings.Y}) couleur={color.R},{color.G},{color.B}");

                return await SendCommandAsync(payload, cancellationToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Divoom] Erreur lors de l'envoi du texte: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Efface un texte affiché
        /// </summary>
        public async Task<bool> ClearTextAsync(int textId, CancellationToken cancellationToken = default)
        {
            try
            {
                var payload = new
                {
                    Command = "Draw/ClearHttpText",
                    TextId = textId
                };

                return await SendCommandAsync(payload, cancellationToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Divoom] Erreur lors de l'effacement du texte: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region Image Display

        /// <summary>
        /// Affiche une image 64x64 pixel par pixel
        /// </summary>
        public async Task<bool> SendPixelImageAsync(Image<Rgba32> image, CancellationToken cancellationToken = default)
        {
            try
            {
                // Convertir l'image en liste de pixels avec leurs couleurs
                var pixelList = new List<object>();

                for (int y = 0; y < 64; y++)
                {
                    for (int x = 0; x < 64; x++)
                    {
                        var pixel = image[x, y];
                        
                        // IMPORTANT: Ignorer les pixels noirs/transparents (fond)
                        // On ne dessine QUE les pixels qui ont de la couleur
                        if ((pixel.R == 0 && pixel.G == 0 && pixel.B == 0) || pixel.A < 128)
                            continue;

                        pixelList.Add(new
                        {
                            x = x,
                            y = y,
                            color = $"#{pixel.R:X2}{pixel.G:X2}{pixel.B:X2}"
                        });
                    }
                }

                Console.WriteLine($"[Divoom] Envoi de {pixelList.Count} pixels colorés (ignoré {(64*64 - pixelList.Count)} pixels noirs)");

                if (pixelList.Count == 0)
                {
                    Console.WriteLine($"[Divoom] Aucun pixel à envoyer!");
                    return false;
                }

                // Diviser en chunks de 100 pixels max pour éviter des payloads trop grands
                int chunkSize = 100;
                for (int i = 0; i < pixelList.Count; i += chunkSize)
                {
                    var chunk = pixelList.Skip(i).Take(chunkSize).ToList();
                    
                    var payload = new
                    {
                        Command = "Draw/SendHttpItemList",
                        ItemList = chunk.Select(p => new
                        {
                            type = 0, // Pixel type
                            x = ((dynamic)p).x,
                            y = ((dynamic)p).y,
                            color = ((dynamic)p).color
                        }).ToArray()
                    };

                    bool success = await SendCommandAsync(payload, cancellationToken);
                    if (!success)
                    {
                        Console.WriteLine($"[Divoom] Échec envoi chunk {i / chunkSize + 1}");
                        return false;
                    }

                    await Task.Delay(50, cancellationToken); // Petit délai entre les chunks
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Divoom] Erreur lors de l'affichage pixel par pixel: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Affiche une image 64x64 depuis une URL
        /// </summary>
        public async Task<bool> SendImageFromUrlAsync(string url, CancellationToken cancellationToken = default)
        {
            try
            {
                var payload = new
                {
                    Command = "Draw/SendHttpItemList",
                    ItemList = new[]
                    {
                        new
                        {
                            type = 2, // Image type
                            url = url
                        }
                    }
                };

                return await SendCommandAsync(payload, cancellationToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Divoom] Erreur lors de l'affichage de l'image: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Affiche une image 64x64 depuis des données Base64
        /// Format: RGB888 avec alpha pré-multiplié (comme dans le repo r12f/divoom)
        /// </summary>
        public async Task<bool> SendImageBase64Async(string base64Image, CancellationToken cancellationToken = default)
        {
            try
            {
                var payload = new
                {
                    Command = "Draw/SendHttpGif",
                    PicNum = 1,
                    PicWidth = 64,
                    PicOffset = 0,
                    PicID = 0,
                    PicSpeed = 1000,
                    PicData = base64Image
                };

                return await SendCommandAsync(payload, cancellationToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Divoom] Erreur lors de l'affichage de l'image Base64: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region Notifications & Display Control

        /// <summary>
        /// Contrôle la luminosité de l'écran
        /// </summary>
        public async Task<bool> SetBrightnessAsync(int brightness, CancellationToken cancellationToken = default)
        {
            try
            {
                brightness = Math.Max(0, Math.Min(100, brightness)); // 0-100

                var payload = new
                {
                    Command = "Channel/SetBrightness",
                    Brightness = brightness
                };

                return await SendCommandAsync(payload, cancellationToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Divoom] Erreur lors du réglage de la luminosité: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Efface l'écran
        /// </summary>
        public async Task<bool> ClearScreenAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var payload = new
                {
                    Command = "Draw/ClearHttpText"
                };

                bool success = await SendCommandAsync(payload, cancellationToken);
                
                // Aussi réinitialiser le dessin
                var clearDrawPayload = new
                {
                    Command = "Draw/ResetHttpGifId"
                };
                
                await SendCommandAsync(clearDrawPayload, cancellationToken);
                
                return success;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Divoom] Erreur lors de l'effacement de l'écran: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Remplit l'écran avec une couleur
        /// </summary>
        public async Task<bool> FillScreenAsync(string hexColor, CancellationToken cancellationToken = default)
        {
            try
            {
                var payload = new
                {
                    Command = "Draw/ClearHttpText"
                };

                await SendCommandAsync(payload, cancellationToken);

                // Envoyer une commande pour remplir l'écran
                var fillPayload = new
                {
                    Command = "Draw/SendHttpItemList",
                    ItemList = new[]
                    {
                        new
                        {
                            type = 1, // Fill type
                            color = hexColor
                        }
                    }
                };

                return await SendCommandAsync(fillPayload, cancellationToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Divoom] Erreur lors du remplissage: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region System Information

        /// <summary>
        /// Récupère les informations système du Divoom
        /// </summary>
        public async Task<DivoomSystemInfo> GetSystemInfoAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                // Essayer différentes commandes selon la version du firmware
                var commands = new[]
                {
                    "Device/GetDeviceInfo",
                    "System/GetDeviceInfo",
                    "Channel/GetIndex"
                };

                foreach (var cmd in commands)
                {
                    try
                    {
                        var payload = new
                        {
                            Command = cmd
                        };

                        var response = await _httpClient.PostAsync("/post", 
                            new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json"), 
                            cancellationToken);

                        if (!response.IsSuccessStatusCode)
                            continue;

                        var json = await response.Content.ReadAsStringAsync(cancellationToken);
                        Console.WriteLine($"[Divoom] Commande '{cmd}' réponse: {json}");
                        
                        // Essayer de parser
                        var info = JsonSerializer.Deserialize<DivoomSystemInfo>(json, _jsonOptions);
                        if (info != null && info.ErrorCode != "Request data illegal json")
                            return info;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[Divoom] Commande '{cmd}' erreur: {ex.Message}");
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Divoom] Erreur récupération info système: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Récupère tous les paramètres du device
        /// </summary>
        public async Task<string> GetAllSettingsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var payload = new
                {
                    Command = "Channel/GetAllConf"
                };

                var response = await _httpClient.PostAsync("/post", 
                    new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json"), 
                    cancellationToken);

                var json = await response.Content.ReadAsStringAsync(cancellationToken);
                Console.WriteLine($"[Divoom] Paramètres: {json}");
                return json;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Divoom] Erreur récupération paramètres: {ex.Message}");
                return null;
            }
        }

        #endregion

        #region Helper Methods

        private async Task<bool> SendCommandAsync(object payload, CancellationToken cancellationToken)
        {
            try
            {
                var json = JsonSerializer.Serialize(payload, _jsonOptions);
                Console.WriteLine($"[Divoom] Envoi commande: {json.Substring(0, Math.Min(200, json.Length))}...");

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("/post", content, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
                    Console.WriteLine($"[Divoom] Erreur HTTP {response.StatusCode}: {responseBody}");
                    return false;
                }

                var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
                Console.WriteLine($"[Divoom] Réponse: {responseJson}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Divoom] Exception lors de l'envoi: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Parse une couleur hexadécimale
        /// </summary>
        private (byte R, byte G, byte B) ParseHexColor(string hexColor)
        {
            if (string.IsNullOrEmpty(hexColor))
                return (255, 255, 255);

            hexColor = hexColor.TrimStart('#');
            
            if (hexColor.Length == 6)
            {
                return (
                    Convert.ToByte(hexColor.Substring(0, 2), 16),
                    Convert.ToByte(hexColor.Substring(2, 2), 16),
                    Convert.ToByte(hexColor.Substring(4, 2), 16)
                );
            }

            return (255, 255, 255);
        }

        /// <summary>
        /// Convertit une Image en données RGB888 avec alpha pré-multiplié
        /// Correspond exactement au format du repo r12f/divoom
        /// </summary>
        private static byte[] ConvertImageToRgb888WithPremultipliedAlpha(Image<Rgba32> image)
        {
            if (image.Width != 64 || image.Height != 64)
                throw new ArgumentException("L'image doit faire 64x64 pixels");

            // 64x64 pixels × 3 bytes RGB = 12288 bytes
            byte[] buffer = new byte[64 * 64 * 3];
            int bufferIndex = 0;

            // Parcourir tous les pixels (row par row, left to right)
            for (int y = 0; y < 64; y++)
            {
                for (int x = 0; x < 64; x++)
                {
                    var pixel = image[x, y];
                    
                    // Alpha pré-multiplié comme dans le code Rust:
                    // (color * alpha / 255)
                    buffer[bufferIndex++] = (byte)(pixel.R * pixel.A / 255);  // R
                    buffer[bufferIndex++] = (byte)(pixel.G * pixel.A / 255);  // G
                    buffer[bufferIndex++] = (byte)(pixel.B * pixel.A / 255);  // B
                }
            }

            return buffer;
        }

        /// <summary>
        /// Affiche une image 64x64 sur le Divoom
        /// </summary>
        public async Task<bool> SendImageAsync(Image<Rgba32> image, CancellationToken cancellationToken = default)
        {
            try
            {
                // Convertir l'image en RGB888 avec alpha pré-multiplié
                byte[] imageData = ConvertImageToRgb888WithPremultipliedAlpha(image);
                string base64Data = Convert.ToBase64String(imageData);
                
                Console.WriteLine($"[Divoom] Image convertie: {imageData.Length} bytes -> {base64Data.Length} chars base64");
                
                return await SendImageBase64Async(base64Data, cancellationToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Divoom] Erreur conversion image: {ex.Message}");
                return false;
            }
        }

        #endregion

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }

    #region Enums & Settings

    /// <summary>
    /// Canaux d'affichage Divoom
    /// </summary>
    public enum DivoomChannel
    {
        Faces = 0,
        Cloud = 1,
        Visualizer = 2,
        Custom = 3
    }

    /// <summary>
    /// Direction du texte
    /// </summary>
    public enum DivoomTextDirection
    {
        Left = 0,
        Right = 1
    }

    /// <summary>
    /// Alignement du texte
    /// </summary>
    public enum DivoomTextAlignment
    {
        Left = 1,
        Center = 2,
        Right = 3
    }

    /// <summary>
    /// Paramètres pour l'affichage de texte
    /// </summary>
    public class DivoomTextSettings
    {
        public int TextId { get; set; } = 1;
        public int X { get; set; } = 0;
        public int Y { get; set; } = 0;
        public DivoomTextDirection Direction { get; set; } = DivoomTextDirection.Left;
        public int Font { get; set; } = 0;
        public int TextWidth { get; set; } = 64;
        public int ScrollSpeed { get; set; } = 50;
        public string Color { get; set; } = "#FFFFFF";
        public DivoomTextAlignment Alignment { get; set; } = DivoomTextAlignment.Left;
    }

    /// <summary>
    /// Informations système du Divoom
    /// </summary>
    public class DivoomSystemInfo
    {
        [JsonPropertyName("DeviceName")]
        public string DeviceName { get; set; }

        [JsonPropertyName("DeviceId")]
        public int? DeviceId { get; set; }

        [JsonPropertyName("DevicePrivateIP")]
        public string DevicePrivateIP { get; set; }

        [JsonPropertyName("DeviceMac")]
        public string DeviceMac { get; set; }

        [JsonPropertyName("FirmwareVersion")]
        public string FirmwareVersion { get; set; }

        [JsonPropertyName("HardwareVersion")]
        public string HardwareVersion { get; set; }

        [JsonPropertyName("error_code")]
        public object ErrorCode { get; set; } // Accepte int ou string
        
        [JsonPropertyName("SelectIndex")]
        public int? SelectIndex { get; set; }
        
        [JsonPropertyName("Brightness")]
        public int? Brightness { get; set; }
        
        public bool IsSuccess => 
            (ErrorCode is int intCode && intCode == 0) || 
            (ErrorCode is string strCode && strCode == "0");
    }

    #endregion
}
