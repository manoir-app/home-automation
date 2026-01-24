using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Home.Common;
using Home.Agents.Aurore.UserNotifications;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;

namespace Home.Agents.Aurore.Integrations
{
    /// <summary>
    /// Helper pour les notifications et applications de chat sur Divoom
    /// </summary>
    partial class DivoomHelper
    {
        /// <summary>
        /// Envoie une notification Divoom pour un nouveau message de chat
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
                        Console.WriteLine($"[Divoom Chat] Mode privé activé, pas de notification");
                        return;
                    }

                    Console.WriteLine($"[Divoom Chat] Notification de {senderName}: {message}");

                    // Générer l'image 64x64 avec bulle de chat et texte
                    string imageBase64 = CreateChatNotificationImage(senderName, message, channelName);

                    var divoomDevices = GetConfiguredDivoomDevices();
                    foreach (var ip in divoomDevices)
                    {
                        try
                        {
                            using (var client = new Divoom(ip))
                            {
                                // Sélectionner le canal Custom
                                await client.SelectChannelAsync(DivoomChannel.Custom);
                                await Task.Delay(100);
                                
                                bool success = await client.SendImageBase64Async(imageBase64);
                                if (success)
                                {
                                    Console.WriteLine($"[Divoom Chat] ✓ Notification envoyée sur {ip}");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[Divoom Chat] ✗ Erreur notification pour {ip}: {ex.Message}");
                        }

                        await Task.Delay(100);
                    }

                    // Afficher pendant 10 secondes puis revenir au canal précédent
                    await Task.Delay(10000);
                    
                    foreach (var ip in divoomDevices)
                    {
                        try
                        {
                            using (var client = new Divoom(ip))
                            {
                                await client.SelectChannelAsync(DivoomChannel.Faces);
                            }
                        }
                        catch { }
                    }

                    Console.WriteLine($"[Divoom Chat] Notification terminée pour message de {senderName}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Divoom Chat] Erreur lors de l'envoi: {ex.Message}");
                }
            });
        }

        /// <summary>
        /// Crée une image 64x64 pour une notification de chat
        /// </summary>
        private static string CreateChatNotificationImage(string senderName, string message, string channelName)
        {
            try
            {
                using (var image = new Image<Rgba32>(64, 64))
                {
                    // Couleurs
                    var bgColor = new Rgba32(0, 0, 0); // Fond noir
                    var chatColor = new Rgba32(0, 170, 255); // Bulle bleue
                    var textColor = new Rgba32(255, 255, 255); // Blanc

                    // Remplir le fond
                    image.Mutate(ctx => ctx.BackgroundColor(SixLabors.ImageSharp.Color.Black));

                    // Dessiner une grande bulle de chat (16x16 au lieu de 8x8)
                    DrawLargeChatBubble(image, 4, 4, chatColor);

                    // Dessiner le texte (simplifié pour le moment)
                    // On affichera le nom de l'expéditeur et une partie du message
                    
                    // TODO: Intégrer SixLabors.Fonts pour un vrai rendu de texte
                    // Pour l'instant, on utilise juste la bulle comme indicateur visuel

                    // Convertir en Base64 RGB565
                    return ConvertImageToBase64RGB565(image);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Divoom Chat] Erreur génération image: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Dessine une grande bulle de chat (16x16)
        /// </summary>
        private static void DrawLargeChatBubble(Image<Rgba32> image, int offsetX, int offsetY, Rgba32 chatColor)
        {
            int scale = 2; // Facteur d'agrandissement pour bulle 8x8 -> 16x16
            var textColor = new Rgba32(255, 255, 255);

            // Bulle de chat agrandie
            // Ligne haute
            for (int x = 1; x <= 6; x++)
                FillRect(image, offsetX + x * scale, offsetY + 1 * scale, scale, scale, chatColor);

            // Côtés
            for (int y = 2; y <= 4; y++)
            {
                FillRect(image, offsetX + 0 * scale, offsetY + y * scale, scale, scale, chatColor);
                FillRect(image, offsetX + 7 * scale, offsetY + y * scale, scale, scale, chatColor);
            }

            // Ligne basse
            for (int x = 1; x <= 6; x++)
                FillRect(image, offsetX + x * scale, offsetY + 5 * scale, scale, scale, chatColor);

            // Remplissage intérieur
            for (int y = 2; y <= 4; y++)
                for (int x = 1; x <= 6; x++)
                    FillRect(image, offsetX + x * scale, offsetY + y * scale, scale, scale, chatColor);

            // Points de texte (3 points dans la bulle)
            FillRect(image, offsetX + 2 * scale, offsetY + 3 * scale, scale, scale, textColor);
            FillRect(image, offsetX + 4 * scale, offsetY + 3 * scale, scale, scale, textColor);
            FillRect(image, offsetX + 6 * scale, offsetY + 3 * scale, scale, scale, textColor);

            // Queue de la bulle (en bas à gauche)
            FillRect(image, offsetX + 1 * scale, offsetY + 6 * scale, scale, scale, chatColor);
            FillRect(image, offsetX + 0 * scale, offsetY + 7 * scale, scale, scale, chatColor);

            // Badge de notification (point rouge en haut à droite)
            var badgeColor = new Rgba32(255, 0, 0);
            FillRect(image, offsetX + 12 * scale, offsetY + 0 * scale, scale * 2, scale * 2, badgeColor);
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
                        // Revenir au canal Faces si plus de messages
                        var devices = GetConfiguredDivoomDevices();
                        foreach (var ip in devices)
                        {
                            try
                            {
                                using (var client = new Divoom(ip))
                                {
                                    await client.SelectChannelAsync(DivoomChannel.Faces);
                                    Console.WriteLine($"[Divoom Chat] ✓ Retour au canal Faces sur {ip}");
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"[Divoom Chat] ✗ Erreur pour {ip}: {ex.Message}");
                            }
                        }
                        return;
                    }

                    string statusText = unreadCount == 1 
                        ? "1 nouveau message" 
                        : $"{unreadCount} nouveaux messages";

                    Console.WriteLine($"[Divoom Chat] Mise à jour: {statusText}");

                    // Générer l'image avec compteur
                    string imageBase64 = CreateUnreadCountImage(unreadCount);

                    var divoomDevices = GetConfiguredDivoomDevices();
                    
                    foreach (var ip in divoomDevices)
                    {
                        try
                        {
                            using (var client = new Divoom(ip))
                            {
                                await client.SelectChannelAsync(DivoomChannel.Custom);
                                await Task.Delay(100);
                                
                                bool success = await client.SendImageBase64Async(imageBase64);
                                
                                if (success)
                                {
                                    Console.WriteLine($"[Divoom Chat] ✓ Compteur mis à jour sur {ip}");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[Divoom Chat] ✗ Erreur pour {ip}: {ex.Message}");
                        }
                    }

                    Console.WriteLine($"[Divoom Chat] ✓ Terminé");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Divoom Chat] ✗ Erreur globale: {ex.Message}");
                }
            });
        }

        /// <summary>
        /// Crée une image 64x64 avec le compteur de messages non lus
        /// </summary>
        private static string CreateUnreadCountImage(int count)
        {
            try
            {
                using (var image = new Image<Rgba32>(64, 64))
                {
                    var bgColor = new Rgba32(0, 0, 0);
                    var chatColor = new Rgba32(0, 170, 255);
                    var badgeColor = new Rgba32(255, 0, 0);

                    // Remplir le fond
                    image.Mutate(ctx => ctx.BackgroundColor(SixLabors.ImageSharp.Color.Black));

                    // Grande bulle de chat centrée
                    DrawLargeChatBubble(image, 16, 16, chatColor);

                    // Badge avec nombre (simplifié)
                    // TODO: Afficher le nombre avec SixLabors.Fonts
                    FillRect(image, 44, 8, 12, 12, badgeColor);

                    return ConvertImageToBase64RGB565(image);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Divoom Chat] Erreur génération compteur: {ex.Message}");
                return null;
            }
        }
    }
}
