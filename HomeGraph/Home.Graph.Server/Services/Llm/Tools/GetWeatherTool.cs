using Home.Common.Llm;
using Home.Graph.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Home.Graph.Server.Services.Llm.Tools
{
    /// <summary>
    /// Outil pour récupérer les informations météo
    /// </summary>
    public class GetWeatherTool : ILlmTool
    {
        private readonly WeatherService _weatherService;

        public GetWeatherTool()
        {
            _weatherService = new WeatherService();
        }

        public string Name => "get_weather";

        public string Description =>
            "Récupère les informations météorologiques actuelles pour la localisation de la maison.";

        public Dictionary<string, object> ParametersSchema => new Dictionary<string, object>
        {
            ["type"] = "object",
            ["properties"] = new Dictionary<string, object>(),
            ["required"] = new List<string>()
        };

        public async Task<object> ExecuteAsync(Dictionary<string, object> parameters, CancellationToken cancellationToken = default)
        {
            return await Task.Run(() =>
            {
                try
                {
                    var currentWeather = _weatherService.GetCurrentWeather();

                    if (currentWeather == null)
                    {
                        return new
                        {
                            success = false,
                            message = "Aucune information météo disponible actuellement."
                        };
                    }

                    var forecast = _weatherService.GetWeatherForecast(3);

                    var result = new
                    {
                        success = true,
                        weather = new
                        {
                            temperature = currentWeather.Temperature,
                            temperatureUnit = "°C",
                            condition = currentWeather.Label,
                            description = currentWeather.Description,
                            humidity = currentWeather.Humidity,
                            windSpeed = currentWeather.WindSpeed,
                            pressure = currentWeather.Pressure,
                            forecast = forecast.Select(w => new
                            {
                                date = w.DateTime,
                                temperature = w.Temperature,
                                condition = w.Label,
                                description = w.Description
                            }).ToList()
                        }
                    };

                    return result;
                }
                catch (System.Exception ex)
                {
                    return new
                    {
                        success = false,
                        error = $"Erreur lors de la récupération de la météo: {ex.Message}"
                    };
                }
            });
        }
    }
}
