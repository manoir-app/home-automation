using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Formats.Jpeg;

namespace Home.Agents.Aurore.Integrations
{
    /// <summary>
    /// Helper pour accéder aux icônes incorporées dans les ressources
    /// </summary>
    public static class IconResourceHelper
    {
        private static readonly Assembly _assembly = typeof(IconResourceHelper).Assembly;
        private static readonly string _resourcePrefix = "Home.Agents.Aurore.Files.pixelart.";
        private static Dictionary<string, string> _iconCache = new Dictionary<string, string>();

        /// <summary>
        /// Obtient une icône en Base64 depuis les ressources incorporées
        /// </summary>
        /// <param name="iconName">Nom de l'icône (avec ou sans extension, ex: "door-open" ou "door-open.png")</param>
        /// <param name="subfolder">Sous-dossier optionnel (ex: "8x8", "16x16")</param>
        /// <returns>Icône encodée en Base64 JPEG ou null si non trouvée</returns>
        public static string GetIconBase64(string iconName, string subfolder = "8x8")
        {
            if (string.IsNullOrEmpty(iconName))
                return null;

            // Nettoyer le nom
            iconName = iconName.Trim();
            
            // Ajouter .png si pas d'extension
            if (!iconName.EndsWith(".png", StringComparison.OrdinalIgnoreCase) &&
                !iconName.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) &&
                !iconName.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase))
            {
                iconName += ".png";
            }

            // Créer la clé de cache
            string cacheKey = $"{subfolder}/{iconName}";
            
            // Vérifier le cache
            if (_iconCache.TryGetValue(cacheKey, out string cachedBase64))
            {
                return cachedBase64;
            }

            // Construire le nom de ressource
            string resourceName = _resourcePrefix;
            if (!string.IsNullOrEmpty(subfolder))
            {
                resourceName += subfolder.Replace("/", ".").Replace("\\", ".") + ".";
            }
            resourceName += iconName.Replace("/", ".").Replace("\\", ".");

            Console.WriteLine($"[IconResource] Recherche: {resourceName}");

            try
            {
                // Charger la ressource
                using (Stream stream = _assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream == null)
                    {
                        // Essayer de lister les ressources disponibles pour debug
                        var availableResources = _assembly.GetManifestResourceNames()
                            .Where(r => r.Contains("pixelart"))
                            .ToList();
                        
                        if (availableResources.Any())
                        {
                            Console.WriteLine($"[IconResource] Ressources pixelart disponibles:");
                            foreach (var res in availableResources.Take(5))
                            {
                                Console.WriteLine($"  - {res}");
                            }
                        }
                        
                        Console.WriteLine($"[IconResource] ✗ Icône non trouvée: {resourceName}");
                        return null;
                    }

                    // Charger l'image
                    using (var image = Image.Load<Rgba32>(stream))
                    {
                        // Convertir en JPEG Base64
                        string base64 = ConvertToBase64Jpeg(image);
                        
                        // Mettre en cache
                        _iconCache[cacheKey] = base64;
                        
                        Console.WriteLine($"[IconResource] ✓ Icône chargée: {iconName} ({base64.Length} chars)");
                        return base64;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[IconResource] ✗ Erreur chargement {iconName}: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Liste toutes les icônes disponibles
        /// </summary>
        /// <param name="subfolder">Filtre par sous-dossier (optionnel)</param>
        /// <returns>Liste des noms d'icônes disponibles</returns>
        public static List<string> ListAvailableIcons(string subfolder = null)
        {
            var icons = new List<string>();
            var resourceNames = _assembly.GetManifestResourceNames()
                .Where(r => r.StartsWith(_resourcePrefix));

            if (!string.IsNullOrEmpty(subfolder))
            {
                string folderPrefix = _resourcePrefix + subfolder.Replace("/", ".").Replace("\\", ".") + ".";
                resourceNames = resourceNames.Where(r => r.StartsWith(folderPrefix));
            }

            foreach (var resourceName in resourceNames)
            {
                // Extraire le nom du fichier
                string fileName = resourceName.Substring(_resourcePrefix.Length);
                if (!string.IsNullOrEmpty(subfolder))
                {
                    fileName = fileName.Substring(subfolder.Replace("/", ".").Replace("\\", ".").Length + 1);
                }
                
                // Ignorer les sous-dossiers
                if (!fileName.Contains("."))
                    continue;

                icons.Add(fileName);
            }

            return icons.OrderBy(i => i).ToList();
        }

        /// <summary>
        /// Convertit une image en Base64 JPEG
        /// </summary>
        private static string ConvertToBase64Jpeg(Image<Rgba32> image)
        {
            using (var ms = new MemoryStream())
            {
                image.SaveAsJpeg(ms, new JpegEncoder { Quality = 90 });
                byte[] imageBytes = ms.ToArray();
                return Convert.ToBase64String(imageBytes);
            }
        }

        /// <summary>
        /// Efface le cache des icônes
        /// </summary>
        public static void ClearCache()
        {
            _iconCache.Clear();
            Console.WriteLine("[IconResource] Cache effacé");
        }

        /// <summary>
        /// Obtient les statistiques du cache
        /// </summary>
        public static string GetCacheStats()
        {
            return $"Cache: {_iconCache.Count} icônes en mémoire";
        }
    }
}
