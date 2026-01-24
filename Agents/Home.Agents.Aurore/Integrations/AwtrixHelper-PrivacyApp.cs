using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Home.Common;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Formats.Jpeg;

namespace Home.Agents.Aurore.Integrations
{
    /// <summary>
    /// Helper pour l'application Privacy Status sur Awtrix
    /// </summary>
    partial class AwtrixHelper
    {
        /// <summary>
        /// Met à jour l'affichage Awtrix avec l'état de confidentialité
        /// </summary>
        public static void UpdateMeshPrivacyStatus(bool isPrivacyMode)
        {
            Task.Run(async () =>
            {
                try
                {
                    string statusText = isPrivacyMode ? "Mode prive" : "Mode normal";

                    Console.WriteLine($"[Awtrix Privacy] Mise à jour: {statusText}");

                    // Génère l'icône en Base64
                    string iconBase64 = CreatePrivacyLockIconBase64(isPrivacyMode);

                    // Application personnalisée avec icône
                    var customApp = new AwtrixCustomApp
                    {
                        Text = statusText,
                        TextOffset = 2,
                        Duration = 4,
                        Lifetime = 90,
                        Icon = iconBase64, // Icône en Base64
                        PushIcon = 0 // L'icône ne bouge pas
                    };

                    var awtrixDevices = GetConfiguredAwtrixDevices();
                    
                    foreach (var ip in awtrixDevices)
                    {
                        try
                        {
                            using (var client = new AwTrix(ip))
                            {
                                bool success = await client.SetCustomAppAsync("privacy_status", customApp);
                                
                                if (success)
                                {
                                    Console.WriteLine($"[Awtrix Privacy] ✓ App mise à jour sur {ip}");
                                }
                                else
                                {
                                    Console.WriteLine($"[Awtrix Privacy] ✗ Échec mise à jour sur {ip}");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[Awtrix Privacy] ✗ Erreur pour {ip}: {ex.Message}");
                        }

                        // Pause entre les devices pour éviter les conflits
                        if (awtrixDevices.Count > 1)
                            await Task.Delay(200);
                    }

                    Console.WriteLine($"[Awtrix Privacy] ✓ Terminé");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Awtrix Privacy] ✗ Erreur globale: {ex.Message}");
                }
            }).Wait();

            Task.Run(async () =>
            {
                try
                {
                    string statusText = isPrivacyMode ? "Passage en mode prive" : "Retour au mode normal";

                    Console.WriteLine($"[Awtrix Privacy] Mise à jour: {statusText}");

                    // Génère l'icône en Base64
                    string iconBase64 = CreatePrivacyLockIconBase64(isPrivacyMode);

                    // Application personnalisée avec icône
                    var notif = new AwtrixNotification
                    {
                        Text = statusText,
                        TextOffset = 2,
                        Repeat = 2,
                        Icon = iconBase64, // Icône en Base64
                        PushIcon = 0 // L'icône ne bouge pas
                    };

                    var awtrixDevices = GetConfiguredAwtrixDevices();

                    foreach (var ip in awtrixDevices)
                    {
                        try
                        {
                            using (var client = new AwTrix(ip))
                            {
                                bool success = await client.SendNotificationAsync(notif);

                                if (success)
                                {
                                    Console.WriteLine($"[Awtrix Privacy] ✓ App mise à jour sur {ip}");
                                }
                                else
                                {
                                    Console.WriteLine($"[Awtrix Privacy] ✗ Échec mise à jour sur {ip}");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[Awtrix Privacy] ✗ Erreur pour {ip}: {ex.Message}");
                        }

                        // Pause entre les devices pour éviter les conflits
                        if (awtrixDevices.Count > 1)
                            await Task.Delay(200);
                    }

                    Console.WriteLine($"[Awtrix Privacy] ✓ Terminé");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Awtrix Privacy] ✗ Erreur globale: {ex.Message}");
                }
            }).Wait();

        }

        /// <summary>
        /// Crée une icône 8x8 de cadenas et la retourne en Base64 JPEG
        /// </summary>
        private static string CreatePrivacyLockIconBase64(bool isLocked)
        {
            try
            {
                using (var image = new Image<Rgba32>(8, 8))
                {
                    // Couleurs
                    var lockColor = isLocked ? new Rgba32(255, 0, 0) : new Rgba32(0, 255, 0); // Rouge ou Vert
                    var bodyColor = isLocked ? new Rgba32(255, 170, 0) : new Rgba32(170, 255, 0); // Orange ou Jaune-vert
                    var holeColor = new Rgba32(0, 0, 0); // Noir
                    var bgColor = new Rgba32(0, 0, 0, 0); // Transparent

                    // Remplir le fond transparent
                    for (int y = 0; y < 8; y++)
                        for (int x = 0; x < 8; x++)
                            image[x, y] = bgColor;

                    if (isLocked)
                    {
                        // Cadenas fermé
                        // Anse (lignes 0-2)
                        for (int x = 2; x <= 5; x++)
                            image[x, 0] = lockColor;
                        
                        image[1, 1] = lockColor;
                        image[6, 1] = lockColor;
                        image[1, 2] = lockColor;
                        image[6, 2] = lockColor;

                        // Corps (lignes 3-7)
                        for (int y = 3; y < 8; y++)
                            for (int x = 0; x < 8; x++)
                                image[x, y] = bodyColor;

                        // Trou de serrure
                        image[3, 5] = holeColor;
                        image[4, 5] = holeColor;
                    }
                    else
                    {
                        // Cadenas ouvert (anse décalée à droite)
                        for (int x = 4; x <= 7; x++)
                            image[x, 0] = lockColor;
                        
                        image[7, 1] = lockColor;
                        image[7, 2] = lockColor;

                        // Corps (lignes 3-7)
                        for (int y = 3; y < 8; y++)
                            for (int x = 0; x < 8; x++)
                                image[x, y] = bodyColor;

                        // Trou de serrure
                        image[3, 5] = holeColor;
                        image[4, 5] = holeColor;
                    }

                    // Convertir en JPEG Base64
                    using (var ms = new MemoryStream())
                    {
                        image.SaveAsJpeg(ms, new JpegEncoder { Quality = 90 });
                        byte[] imageBytes = ms.ToArray();
                        string base64 = Convert.ToBase64String(imageBytes);
                        
                        Console.WriteLine($"[Awtrix Privacy] Icône générée: {base64.Length} caractères Base64");
                        return base64;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Awtrix Privacy] Erreur génération icône: {ex.Message}");
                return null;
            }
        }
    }
}