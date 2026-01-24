using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Home.Common;

namespace Home.Agents.Aurore.Integrations
{
    /// <summary>
    /// Helper pour faciliter l'utilisation d'Awtrix dans l'agent Aurore
    /// </summary>
    public static partial class AwtrixHelper
    {
        private static readonly Dictionary<string, AwTrix> _clients = new Dictionary<string, AwTrix>();
        private static readonly object _lock = new object();

        /// <summary>
        /// Obtient ou crée un client Awtrix pour une IP donnée
        /// </summary>
        public static AwTrix GetClient(string ipAddress, int port = 80)
        {
            string key = $"{ipAddress}:{port}";
            
            lock (_lock)
            {
                if (!_clients.ContainsKey(key))
                {
                    _clients[key] = new AwTrix(ipAddress, port);
                }
                return _clients[key];
            }
        }

        #region BroadcastMessage - Surcharges

        /// <summary>
        /// Envoie un message simple à tous les Awtrix configurés
        /// </summary>
        public static async Task<bool> BroadcastMessageAsync(string message, string icon = null, int? duration = null)
        {
            var awtrixIps = GetConfiguredAwtrixDevices();
            if (awtrixIps == null || !awtrixIps.Any())
            {
                Console.WriteLine("Aucun appareil Awtrix configuré");
                return false;
            }

            bool success = true;
            foreach (var ip in awtrixIps)
            {
                try
                {
                    var client = GetClient(ip);
                    
                    if (!string.IsNullOrEmpty(icon))
                    {
                        // Essayer d'abord de charger depuis les ressources
                        string iconBase64 = IconResourceHelper.GetIconBase64(icon);
                        if (!string.IsNullOrEmpty(iconBase64))
                        {
                            // Utiliser l'icône incorporée
                            var notification = new AwtrixNotification
                            {
                                Text = message,
                                Icon = iconBase64,
                                Duration = duration ?? 5
                            };
                            success &= await client.SendNotificationAsync(notification);
                        }
                        else
                        {
                            // Fallback: utiliser l'icône comme ID AWTRIX
                            success &= await client.SendTextWithIconAsync(message, icon, duration);
                        }
                    }
                    else
                    {
                        success &= await client.SendTextAsync(message, duration);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erreur lors de l'envoi à Awtrix {ip}: {ex.Message}");
                    success = false;
                }
            }

            return success;
        }

        /// <summary>
        /// Envoie un message simple avec une icône typée
        /// </summary>
        public static Task<bool> BroadcastMessageAsync(string message, AwtrixIcon8x8 icon, int? duration = null)
        {
            string iconBase64 = icon.GetBase64();
            return BroadcastMessageAsync(message, iconBase64, duration);
        }

        #endregion

        #region SendProgressNotification - Surcharges

        /// <summary>
        /// Envoie une notification avec progression (utile pour téléchargements, etc.)
        /// </summary>
        public static async Task<bool> SendProgressNotificationAsync(string message, int progress, string icon = null)
        {
            var awtrixIps = GetConfiguredAwtrixDevices();
            if (awtrixIps == null || !awtrixIps.Any())
                return false;

            // Charger l'icône si spécifiée
            string iconBase64 = null;
            if (!string.IsNullOrEmpty(icon))
            {
                iconBase64 = IconResourceHelper.GetIconBase64(icon);
            }

            var notification = new AwtrixNotification
            {
                Text = message,
                Icon = iconBase64 ?? icon, // Utiliser Base64 ou fallback ID
                Progress = progress,
                ProgressColor = "#00FF00",
                ProgressBackgroundColor = "#333333",
                Duration = 5
            };

            bool success = true;
            foreach (var ip in awtrixIps)
            {
                try
                {
                    var client = GetClient(ip);
                    success &= await client.SendNotificationAsync(notification);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erreur lors de l'envoi à Awtrix {ip}: {ex.Message}");
                    success = false;
                }
            }

            return success;
        }

        /// <summary>
        /// Envoie une notification avec progression et icône typée
        /// </summary>
        public static Task<bool> SendProgressNotificationAsync(string message, int progress, AwtrixIcon8x8 icon)
        {
            string iconBase64 = icon.GetBase64();
            return SendProgressNotificationAsync(message, progress, iconBase64);
        }

        #endregion

        #region CreateOrUpdateCustomApp - Surcharges

        /// <summary>
        /// Crée une application personnalisée pour afficher des informations persistantes
        /// (météo, température, etc.)
        /// </summary>
        public static async Task<bool> CreateOrUpdateCustomAppAsync(string appName, string text, string icon = null, int? duration = null)
        {
            var awtrixIps = GetConfiguredAwtrixDevices();
            if (awtrixIps == null || !awtrixIps.Any())
                return false;

            // Charger l'icône si spécifiée
            string iconBase64 = null;
            if (!string.IsNullOrEmpty(icon))
            {
                iconBase64 = IconResourceHelper.GetIconBase64(icon);
            }

            var app = new AwtrixCustomApp
            {
                Text = text,
                Icon = iconBase64 ?? icon, // Utiliser Base64 ou fallback ID
                Duration = duration ?? 10,
                Repeat = -1 // Répétition infinie
            };

            bool success = true;
            foreach (var ip in awtrixIps)
            {
                try
                {
                    var client = GetClient(ip);
                    success &= await client.SetCustomAppAsync(appName, app);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erreur lors de la création de l'app sur Awtrix {ip}: {ex.Message}");
                    success = false;
                }
            }

            return success;
        }

        /// <summary>
        /// Crée une application personnalisée avec icône typée
        /// </summary>
        public static Task<bool> CreateOrUpdateCustomAppAsync(string appName, string text, AwtrixIcon8x8 icon, int? duration = null)
        {
            string iconBase64 = icon.GetBase64();
            return CreateOrUpdateCustomAppAsync(appName, text, iconBase64, duration);
        }

        #endregion

        /// <summary>
        /// Récupère la liste des IPs Awtrix depuis la configuration
        /// </summary>
        internal static List<string> GetConfiguredAwtrixDevices()
        {
            // TODO: Récupérer depuis la configuration de l'intégration
            // Pour l'instant, en dur pour le développement
            
            try
            {
                // Liste des devices Awtrix en dur pour le développement
                var devices = new List<string>
                {
                    "192.168.2.11"
                    // Ajoutez d'autres IPs ici si besoin
                };
                
                return devices;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la récupération de la config Awtrix: {ex.Message}");
            }

            return new List<string>();
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
            
            // Nettoyer aussi le cache des icônes
            IconResourceHelper.ClearCache();
        }

        #region Exemples de notifications prédéfinies

        /// <summary>
        /// Envoie une notification de bienvenue
        /// </summary>
        public static Task<bool> SendWelcomeNotificationAsync(string userName)
        {
            return BroadcastMessageAsync($"Bonjour {userName}!", AwtrixIcon8x8.DoorOpen, 5);
        }

        /// <summary>
        /// Envoie une alerte
        /// </summary>
        public static async Task<bool> SendAlertAsync(string message)
        {
            var awtrixIps = GetConfiguredAwtrixDevices();
            if (awtrixIps == null || !awtrixIps.Any())
                return false;

            // Charger l'icône d'alarme
            string iconBase64 = AwtrixIcon8x8.Alarm.GetBase64();

            var notification = new AwtrixNotification
            {
                Text = message,
                Icon = iconBase64,
                Color = "#FF0000",
                Duration = 10,
                Repeat = 3,
                Rainbow = false,
                Sound = "alarm"
            };

            bool success = true;
            foreach (var ip in awtrixIps)
            {
                try
                {
                    var client = GetClient(ip);
                    success &= await client.SendNotificationAsync(notification);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erreur lors de l'envoi de l'alerte à Awtrix {ip}: {ex.Message}");
                    success = false;
                }
            }

            return success;
        }

        /// <summary>
        /// Envoie une notification météo
        /// </summary>
        public static Task<bool> SendWeatherNotificationAsync(string weatherText, AwtrixIcon8x8 weatherIcon)
        {
            return CreateOrUpdateCustomAppAsync("weather", weatherText, weatherIcon, 15);
        }

        /// <summary>
        /// Envoie une notification météo (surcharge string pour compatibilité)
        /// </summary>
        public static Task<bool> SendWeatherNotificationAsync(string weatherText, string weatherIcon)
        {
            return CreateOrUpdateCustomAppAsync("weather", weatherText, weatherIcon, 15);
        }

        #endregion
    }
}
