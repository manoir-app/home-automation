using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Home.Common;
using Home.Agents.Aurore.UserNotifications;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Formats.Jpeg;

namespace Home.Agents.Aurore.Integrations
{
    /// <summary>
    /// Helper pour les notifications et applications de chat sur Awtrix
    /// </summary>
    partial class AwtrixHelper
    {
        /// <summary>
        /// Envoie une notification Awtrix pour un nouveau message de chat
        /// </summary>
        public static void SendChatNotification(string senderName, string message, string channelName = null)
        {
            Task.Run(async () =>
            {
                try
                {
                    // Vérifier si la maison est en mode privé
                    var mesh = PrivacyModeChangedNotificationHandler.GetMesh();
                    if (mesh != null && mesh.CurrentPrivacyMode.HasValue)
                    {
                        Console.WriteLine($"[Awtrix Chat] Mode privé activé, pas de notification");
                        return;
                    }

                    // Tronquer le message si trop long
                    string messageText = message;
                    if (messageText.Length > 50)
                    {
                        messageText = messageText.Substring(0, 47) + "...";
                    }

                    string displayText = string.IsNullOrEmpty(channelName)
                        ? $"{senderName}: {messageText}"
                        : $"{senderName} ({channelName}): {messageText}";

                    // Générer l'icône de chat
                    string chatIconBase64 = CreateChatIconBase64();

                    // Notification temporaire (sans couleur de texte)
                    var notification = new AwtrixNotification
                    {
                        Text = displayText,
                        Duration = 10,
                        Repeat = 1,
                        ScrollSpeed = 80,
                        Icon = chatIconBase64,
                        PushIcon = 0
                    };

                    var awtrixDevices = GetConfiguredAwtrixDevices();
                    foreach (var ip in awtrixDevices)
                    {
                        try
                        {
                            using (var client = new AwTrix(ip))
                            {
                                bool success = await client.SendNotificationAsync(notification);
                                if (success)
                                {
                                    Console.WriteLine($"[Awtrix Chat] ✓ Notification envoyée sur {ip}");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[Awtrix Chat] ✗ Erreur notification pour {ip}: {ex.Message}");
                        }

                        await Task.Delay(100);
                    }

                    Console.WriteLine($"[Awtrix Chat] Notification envoyée pour message de {senderName}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Awtrix Chat] Erreur lors de l'envoi de la notification: {ex.Message}");
                }
            });
        }

        /// <summary>
        /// Met à jour l'application personnalisée pour afficher le nombre de messages non lus
        /// </summary>
        public static void UpdateUnreadMessagesApp(int unreadCount)
        {
            Task.Run(async () =>
            {
                try
                {
                    if (unreadCount <= 0)
                    {
                        // Supprimer l'app si plus de messages
                        await DeleteChatApp();
                        return;
                    }

                    string statusText = unreadCount == 1 
                        ? "1 message" 
                        : $"{unreadCount} messages";

                    Console.WriteLine($"[Awtrix Chat] Mise à jour: {statusText}");

                    // Générer l'icône de chat avec badge
                    string chatIconBase64 = CreateChatIconBase64(unreadCount > 0);

                    // Application personnalisée (sans couleur de texte)
                    var customApp = new AwtrixCustomApp
                    {
                        Text = statusText,
                        Duration = 8,
                        Repeat = -1, // Répétition infinie
                        Lifetime = 300, // 5 minutes de lifetime
                        Icon = chatIconBase64,
                        PushIcon = 0,
                        TextOffset = 2
                    };

                    var awtrixDevices = GetConfiguredAwtrixDevices();
                    
                    foreach (var ip in awtrixDevices)
                    {
                        try
                        {
                            using (var client = new AwTrix(ip))
                            {
                                bool success = await client.SetCustomAppAsync("chat_unread", customApp);
                                
                                if (success)
                                {
                                    Console.WriteLine($"[Awtrix Chat] ✓ App mise à jour sur {ip}");
                                }
                                else
                                {
                                    Console.WriteLine($"[Awtrix Chat] ✗ Échec mise à jour sur {ip}");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[Awtrix Chat] ✗ Erreur pour {ip}: {ex.Message}");
                        }

                        if (awtrixDevices.Count > 1)
                            await Task.Delay(200);
                    }

                    Console.WriteLine($"[Awtrix Chat] ✓ Terminé");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Awtrix Chat] ✗ Erreur globale: {ex.Message}");
                }
            });
        }

        /// <summary>
        /// Supprime l'application de chat
        /// </summary>
        private static async Task DeleteChatApp()
        {
            var awtrixDevices = GetConfiguredAwtrixDevices();
            
            foreach (var ip in awtrixDevices)
            {
                try
                {
                    using (var client = new AwTrix(ip))
                    {
                        await client.DeleteCustomAppAsync("chat_unread");
                        Console.WriteLine($"[Awtrix Chat] ✓ App supprimée sur {ip}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Awtrix Chat] ✗ Erreur suppression pour {ip}: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Crée une icône 8x8 de bulle de chat et la retourne en Base64 JPEG
        /// </summary>
        private static string CreateChatIconBase64(bool hasUnread = false)
        {
            try
            {
                using (var image = new Image<Rgba32>(8, 8))
                {
                    // Couleurs
                    var chatColor = new Rgba32(0, 170, 255); // Bleu
                    var textColor = new Rgba32(255, 255, 255); // Blanc
                    var badgeColor = new Rgba32(255, 0, 0); // Rouge pour le badge
                    var bgColor = new Rgba32(0, 0, 0, 0); // Transparent

                    // Remplir le fond transparent
                    for (int y = 0; y < 8; y++)
                        for (int x = 0; x < 8; x++)
                            image[x, y] = bgColor;

                    // Bulle de chat (rectangle arrondi)
                    // Ligne haute
                    for (int x = 1; x <= 6; x++)
                        image[x, 1] = chatColor;

                    // Côtés
                    for (int y = 2; y <= 4; y++)
                    {
                        image[0, y] = chatColor;
                        image[7, y] = chatColor;
                    }

                    // Ligne basse
                    for (int x = 1; x <= 6; x++)
                        image[x, 5] = chatColor;

                    // Remplissage intérieur
                    for (int y = 2; y <= 4; y++)
                        for (int x = 1; x <= 6; x++)
                            image[x, y] = chatColor;

                    // Points de texte (3 points dans la bulle)
                    image[2, 3] = textColor;
                    image[4, 3] = textColor;
                    image[6, 3] = textColor;

                    // Queue de la bulle (en bas à gauche)
                    image[1, 6] = chatColor;
                    image[0, 7] = chatColor;

                    // Badge de notification si des messages non lus
                    if (hasUnread)
                    {
                        image[6, 0] = badgeColor;
                        image[7, 0] = badgeColor;
                        image[6, 1] = badgeColor;
                        image[7, 1] = badgeColor;
                    }

                    // Convertir en JPEG Base64
                    using (var ms = new MemoryStream())
                    {
                        image.SaveAsJpeg(ms, new JpegEncoder { Quality = 90 });
                        byte[] imageBytes = ms.ToArray();
                        string base64 = Convert.ToBase64String(imageBytes);
                        
                        Console.WriteLine($"[Awtrix Chat] Icône générée: {base64.Length} caractères Base64");
                        return base64;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Awtrix Chat] Erreur génération icône: {ex.Message}");
                return null;
            }
        }
    }
}
