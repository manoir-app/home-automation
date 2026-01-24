using Home.Common.Model;
using Home.Graph.Common;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Home.Graph.Services
{
    /// <summary>
    /// Service de gestion des scènes domotiques
    /// </summary>
    public class SceneService
    {
        private readonly IMongoCollection<Scene> _scenesCollection;

        public SceneService()
        {
            var mongoDb = MongoDbHelper.GetClient<Scene>();
            _scenesCollection = mongoDb.Database.GetCollection<Scene>("scenes");
        }

        /// <summary>
        /// Récupère toutes les scènes
        /// </summary>
        public List<Scene> GetAllScenes(string areaId = null)
        {
            try
            {
                var filter = Builders<Scene>.Filter.Empty;

                if (!string.IsNullOrEmpty(areaId))
                {
                    filter = Builders<Scene>.Filter.Eq(s => s.AreaId, areaId);
                }

                return _scenesCollection
                    .Find(filter)
                    .ToList();
            }
            catch (Exception ex)
            {
                LogHelper.Log("scene-service", "get-scenes-error", $"Erreur: {ex.Message}");
                return new List<Scene>();
            }
        }

        /// <summary>
        /// Récupère une scène par son ID
        /// </summary>
        public Scene GetSceneById(string sceneId)
        {
            if (string.IsNullOrEmpty(sceneId))
                return null;

            try
            {
                return _scenesCollection
                    .Find(s => s.Id == sceneId)
                    .FirstOrDefault();
            }
            catch (Exception ex)
            {
                LogHelper.Log("scene-service", "get-scene-error", $"Erreur: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Exécute une scène
        /// </summary>
        public bool ExecuteScene(string sceneId)
        {
            if (string.IsNullOrEmpty(sceneId))
                return false;

            try
            {
                var scene = GetSceneById(sceneId);
                if (scene == null)
                    return false;

                // Utiliser SceneHelper existant
                SceneHelper.ExecuteScene(sceneId);
                
                LogHelper.Log("scene-service", "scene-executed", $"Scène {sceneId} exécutée");
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Log("scene-service", "execute-scene-error", $"Erreur: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Liste les scènes actives
        /// </summary>
        public List<Scene> GetActiveScenes(string areaId = null)
        {
            var allScenes = GetAllScenes(areaId);
            return allScenes.Where(s => s.IsActive).ToList();
        }
    }
}
