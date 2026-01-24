using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Home.Common;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Home.Agents.Aurore.Integrations
{
    /// <summary>
    /// Helper pour faciliter l'utilisation de Divoom dans l'agent Aurore
    /// </summary>
    public static partial class DivoomHelper
    {
        private static readonly Dictionary<string, Divoom> _clients = new Dictionary<string, Divoom>();
        private static readonly object _lock = new object();

        /// <summary>
        /// Obtient ou crée un client Divoom pour une IP donnée
        /// </summary>
        public static Divoom GetClient(string ipAddress, int port = 80)
        {
            string key = $"{ipAddress}:{port}";
            
            lock (_lock)
            {
                if (!_clients.ContainsKey(key))
                {
                    _clients[key] = new Divoom(ipAddress, port);
                }
                return _clients[key];
            }
        }

        /// <summary>
        /// Envoie un message texte simple à tous les Divoom configurés
        /// </summary>
        public static async Task<bool> SendTextAsync(string text, DivoomTextSettings settings = null)
        {
            var divoomIps = GetConfiguredDivoomDevices();
            if (divoomIps == null || !divoomIps.Any())
            {
                Console.WriteLine("[Divoom] Aucun appareil Divoom configuré");
                return false;
            }

            bool success = true;
            foreach (var ip in divoomIps)
            {
                try
                {
                    var client = GetClient(ip);
                    
                    // Sélectionner le canal Custom pour afficher notre contenu
                    await client.SelectChannelAsync(DivoomChannel.Custom);
                    await Task.Delay(100);
                    
                    success &= await client.SendTextAsync(text, settings);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Divoom] Erreur lors de l'envoi à {ip}: {ex.Message}");
                    success = false;
                }
            }

            return success;
        }

        /// <summary>
        /// Affiche une image sur tous les Divoom configurés
        /// </summary>
        public static async Task<bool> SendImageAsync(string base64Image)
        {
            var divoomIps = GetConfiguredDivoomDevices();
            if (divoomIps == null || !divoomIps.Any())
                return false;

            bool success = true;
            foreach (var ip in divoomIps)
            {
                try
                {
                    var client = GetClient(ip);
                    
                    // Sélectionner le canal Custom
                    await client.SelectChannelAsync(DivoomChannel.Custom);
                    await Task.Delay(100);
                    
                    success &= await client.SendImageBase64Async(base64Image);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Divoom] Erreur lors de l'envoi de l'image à {ip}: {ex.Message}");
                    success = false;
                }
            }

            return success;
        }

        /// <summary>
        /// Récupère la liste des IPs Divoom depuis la configuration
        /// </summary>
        internal static List<string> GetConfiguredDivoomDevices()
        {
            // TODO: Récupérer depuis la configuration de l'intégration
            // Pour l'instant, en dur pour le développement
            
            try
            {
                // Liste des devices Divoom en dur pour le développement
                var devices = new List<string>
                {
                    "192.168.2.64"
                    // Ajoutez d'autres IPs ici si besoin
                };
                
                return devices;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Divoom] Erreur lors de la récupération de la config: {ex.Message}");
            }

            return new List<string>();
        }

        /// <summary>
        /// Convertit une image en Base64 RGB565 pour Divoom
        /// Format: chaque pixel = 2 bytes (RGB565) Big Endian
        /// </summary>
        internal static string ConvertImageToBase64RGB565(Image<Rgba32> image)
        {
            try
            {
                // Le Divoom attend du RGB565 (16-bit par pixel) en Big Endian
                // 64x64 pixels = 4096 pixels * 2 bytes = 8192 bytes
                byte[] rgb565Data = new byte[64 * 64 * 2];
                int index = 0;

                for (int y = 0; y < 64; y++)
                {
                    for (int x = 0; x < 64; x++)
                    {
                        var pixel = image[x, y];
                        
                        // Convertir RGBA32 en RGB565
                        byte r = (byte)((pixel.R >> 3) & 0x1F); // 5 bits
                        byte g = (byte)((pixel.G >> 2) & 0x3F); // 6 bits
                        byte b = (byte)((pixel.B >> 3) & 0x1F); // 5 bits
                        
                        ushort rgb565 = (ushort)((r << 11) | (g << 5) | b);
                        
                        // Big-endian (MSB first)
                        rgb565Data[index++] = (byte)((rgb565 >> 8) & 0xFF);
                        rgb565Data[index++] = (byte)(rgb565 & 0xFF);
                    }
                }

                string base64 = Convert.ToBase64String(rgb565Data);
                Console.WriteLine($"[Divoom] Image encodée: {base64.Length} caractères Base64 ({rgb565Data.Length} bytes)");
                return base64;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Divoom] Erreur conversion RGB565: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Remplit un rectangle sur l'image
        /// </summary>
        internal static void FillRect(Image<Rgba32> image, int x, int y, int width, int height, Rgba32 color)
        {
            for (int py = y; py < y + height && py < 64; py++)
            {
                for (int px = x; px < x + width && px < 64; px++)
                {
                    if (px >= 0 && py >= 0)
                        image[px, py] = color;
                }
            }
        }

        /// <summary>
        /// Nettoie les clients
        /// </summary>
        public static void Cleanup()
        {
            lock (_lock)
            {
                foreach (var client in _clients.Values)
                {
                    try
                    {
                        client.Dispose();
                    }
                    catch { }
                }
                _clients.Clear();
            }
        }
    }
}
