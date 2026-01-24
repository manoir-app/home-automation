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
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.Fonts;

namespace Home.Agents.Aurore.Integrations
{
    /// <summary>
    /// Helper pour l'application Privacy Status sur Divoom
    /// </summary>
    partial class DivoomHelper
    {
        /// <summary>
        /// Met à jour l'affichage Divoom avec l'état de confidentialité
        /// </summary>
        public static void UpdateMeshPrivacyStatus(bool isPrivacyMode)
        {
            Task.Run(async () =>
            {
                try
                {
                    string statusText = isPrivacyMode ? "MODE PRIVE" : "MODE NORMAL";

                    Console.WriteLine($"[Divoom Privacy] Mise à jour: {statusText}");

                    // Générer l'image 64x64 avec icône et texte
                    string imageBase64 = CreatePrivacyStatusImage(isPrivacyMode);

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
                                    Console.WriteLine($"[Divoom Privacy] ✓ Image envoyée sur {ip}");
                                }
                                else
                                {
                                    Console.WriteLine($"[Divoom Privacy] ✗ Échec envoi sur {ip}");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[Divoom Privacy] ✗ Erreur pour {ip}: {ex.Message}");
                        }

                        if (divoomDevices.Count > 1)
                            await Task.Delay(200);
                    }

                    Console.WriteLine($"[Divoom Privacy] ✓ Terminé");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Divoom Privacy] ✗ Erreur globale: {ex.Message}");
                }
            });
        }

        /// <summary>
        /// Crée une image 64x64 avec l'état de confidentialité
        /// </summary>
        private static string CreatePrivacyStatusImage(bool isLocked)
        {
            try
            {
                using (var image = new Image<Rgba32>(64, 64))
                {
                    // Couleurs
                    var bgColor = new Rgba32(0, 0, 0); // Fond noir
                    var lockColor = isLocked ? new Rgba32(255, 0, 0) : new Rgba32(0, 255, 0); // Rouge ou Vert
                    var bodyColor = isLocked ? new Rgba32(255, 170, 0) : new Rgba32(170, 255, 0); // Orange ou Jaune-vert
                    var textColor = new Rgba32(255, 255, 255); // Blanc

                    // Remplir le fond
                    image.Mutate(ctx => ctx.BackgroundColor(SixLabors.ImageSharp.Color.Black));

                    // Dessiner le cadenas (agrandi 8x pour 64x64)
                    DrawLargeLockIcon(image, isLocked, lockColor, bodyColor);

                    // Ajouter le texte en bas
                    string statusText = isLocked ? "PRIVE" : "NORMAL";
                    DrawText(image, statusText, 32, 52, textColor);

                    // Convertir en Base64 (format RGB565 pour Divoom)
                    return ConvertImageToBase64RGB565(image);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Divoom Privacy] Erreur génération image: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Dessine un cadenas agrandi au centre de l'image
        /// </summary>
        private static void DrawLargeLockIcon(Image<Rgba32> image, bool isLocked, Rgba32 lockColor, Rgba32 bodyColor)
        {
            int scale = 4; // Facteur d'agrandissement
            int offsetX = 16; // Centrage horizontal
            int offsetY = 4;  // Position verticale

            if (isLocked)
            {
                // Cadenas fermé (8x8 agrandi à 32x32)
                // Anse (lignes 0-2)
                for (int x = 2; x <= 5; x++)
                    FillRect(image, offsetX + x * scale, offsetY + 0 * scale, scale, scale, lockColor);
                
                FillRect(image, offsetX + 1 * scale, offsetY + 1 * scale, scale, scale, lockColor);
                FillRect(image, offsetX + 6 * scale, offsetY + 1 * scale, scale, scale, lockColor);
                FillRect(image, offsetX + 1 * scale, offsetY + 2 * scale, scale, scale, lockColor);
                FillRect(image, offsetX + 6 * scale, offsetY + 2 * scale, scale, scale, lockColor);

                // Corps (lignes 3-7)
                for (int y = 3; y < 8; y++)
                    for (int x = 0; x < 8; x++)
                        FillRect(image, offsetX + x * scale, offsetY + y * scale, scale, scale, bodyColor);

                // Trou de serrure
                FillRect(image, offsetX + 3 * scale, offsetY + 5 * scale, scale, scale, new Rgba32(0, 0, 0));
                FillRect(image, offsetX + 4 * scale, offsetY + 5 * scale, scale, scale, new Rgba32(0, 0, 0));
            }
            else
            {
                // Cadenas ouvert
                for (int x = 4; x <= 7; x++)
                    FillRect(image, offsetX + x * scale, offsetY + 0 * scale, scale, scale, lockColor);
                
                FillRect(image, offsetX + 7 * scale, offsetY + 1 * scale, scale, scale, lockColor);
                FillRect(image, offsetX + 7 * scale, offsetY + 2 * scale, scale, scale, lockColor);

                // Corps
                for (int y = 3; y < 8; y++)
                    for (int x = 0; x < 8; x++)
                        FillRect(image, offsetX + x * scale, offsetY + y * scale, scale, scale, bodyColor);

                // Trou de serrure
                FillRect(image, offsetX + 3 * scale, offsetY + 5 * scale, scale, scale, new Rgba32(0, 0, 0));
                FillRect(image, offsetX + 4 * scale, offsetY + 5 * scale, scale, scale, new Rgba32(0, 0, 0));
            }
        }

        /// <summary>
        /// Dessine du texte sur l'image (simplifié, pixel art style)
        /// </summary>
        private static void DrawText(Image<Rgba32> image, string text, int centerX, int y, Rgba32 color)
        {
            // Pour l'instant, on utilise une méthode simple
            // TODO: Intégrer SixLabors.Fonts pour un vrai rendu de texte
            Console.WriteLine($"[Divoom Privacy] Texte à afficher: {text}");
        }
    }
}
