using Home.Common.Model;
using Home.Graph.Common;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Home.Graph.Services
{
    /// <summary>
    /// Service de gestion des données météo
    /// </summary>
    public class WeatherService
    {
        private readonly IMongoCollection<AutomationMesh> _meshCollection;

        public WeatherService()
        {
            var mongoDb = MongoDbHelper.GetClient<AutomationMesh>();
            _meshCollection = mongoDb.Database.GetCollection<AutomationMesh>("automationmeshes");
        }

        /// <summary>
        /// Récupère les informations météo actuelles
        /// </summary>
        public WeatherInfo GetCurrentWeather()
        {
            try
            {
                var mesh = _meshCollection
                    .Find(m => m.Id == "local")
                    .FirstOrDefault();

                if (mesh?.LocationInfo?.Weather == null || mesh.LocationInfo.Weather.Count == 0)
                    return null;

                return mesh.LocationInfo.Weather.FirstOrDefault();
            }
            catch (Exception ex)
            {
                LogHelper.Log("weather-service", "get-weather-error", $"Erreur: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Récupère les prévisions météo
        /// </summary>
        public List<WeatherInfo> GetWeatherForecast(int days = 3)
        {
            try
            {
                var mesh = _meshCollection
                    .Find(m => m.Id == "local")
                    .FirstOrDefault();

                if (mesh?.LocationInfo?.Weather == null || mesh.LocationInfo.Weather.Count == 0)
                    return new List<WeatherInfo>();

                return mesh.LocationInfo.Weather
                    .Skip(1) // Sauter la météo actuelle
                    .Take(days)
                    .ToList();
            }
            catch (Exception ex)
            {
                LogHelper.Log("weather-service", "get-forecast-error", $"Erreur: {ex.Message}");
                return new List<WeatherInfo>();
            }
        }

        /// <summary>
        /// Récupère les alertes météo
        /// </summary>
        public List<WeatherHazard> GetWeatherHazards()
        {
            try
            {
                var mesh = _meshCollection
                    .Find(m => m.Id == "local")
                    .FirstOrDefault();

                if (mesh?.LocationInfo?.WeatherHazards == null)
                    return new List<WeatherHazard>();

                return mesh.LocationInfo.WeatherHazards;
            }
            catch (Exception ex)
            {
                LogHelper.Log("weather-service", "get-hazards-error", $"Erreur: {ex.Message}");
                return new List<WeatherHazard>();
            }
        }
    }
}
