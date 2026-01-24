using Home.Common;
using Home.Common.Model;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Home.Agents.Aurore.Llm.Tools
{
    /// <summary>
    /// Outil permettant de récupérer la météo actuelle
    /// </summary>
    public class GetWeatherTool : ILlmTool
    {
        public string Name => "get_weather";

        public string Description => "Récupère les informations météorologiques actuelles pour la localisation de la maison.";

        public Dictionary<string, object> ParametersSchema => new Dictionary<string, object>
        {
            ["type"] = "object",
            ["properties"] = new Dictionary<string, object>(),
            ["required"] = new List<string>()
        };

        public Task<object> ExecuteAsync(Dictionary<string, object> parameters, CancellationToken cancellationToken = default)
        {
            try
            {
                var location = AgentHelper.GetLocalMeshLocation("aurore");
                if (location == null)
                    return Task.FromResult<object>(new { success = false, message = "Localisation non disponible" });

                // Construire la réponse avec les données disponibles
                var result = new
                {
                    success = true,
                    location = new
                    {
                        name = location.Name,
                        coordinates = location.Coordinates != null ? new
                        {
                            latitude = location.Coordinates.Latitude,
                            longitude = location.Coordinates.Longitude
                        } : null
                    },
                    message = "Informations météo à implémenter via API dédiée"
                };

                return Task.FromResult<object>(result);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new { success = false, error = ex.Message });
            }
        }
    }
}
