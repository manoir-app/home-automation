using Home.Common.Llm;
using Home.Graph.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Home.Graph.Server.Services.Llm.Tools
{
    /// <summary>
    /// Outil pour lister les scènes domotiques disponibles
    /// </summary>
    public class ListScenesTool : ILlmTool
    {
        private readonly SceneService _sceneService;

        public ListScenesTool()
        {
            _sceneService = new SceneService();
        }

        public string Name => "list_scenes";

        public string Description =>
            "Liste toutes les scènes domotiques disponibles dans la maison (ex: Cinéma, Bonne nuit, etc.)";

        public Dictionary<string, object> ParametersSchema => new Dictionary<string, object>
        {
            ["type"] = "object",
            ["properties"] = new Dictionary<string, object>
            {
                ["areaId"] = new Dictionary<string, object>
                {
                    ["type"] = "string",
                    ["description"] = "ID de la zone pour filtrer les scènes (optionnel)"
                }
            },
            ["required"] = new List<string>()
        };

        public async Task<object> ExecuteAsync(Dictionary<string, object> parameters, CancellationToken cancellationToken = default)
        {
            return await Task.Run(() =>
            {
                try
                {
                    string areaId = parameters != null && parameters.ContainsKey("areaId") 
                        ? parameters["areaId"]?.ToString() 
                        : null;

                    var scenes = _sceneService.GetAllScenes(areaId);

                    if (scenes == null || scenes.Count == 0)
                    {
                        return new
                        {
                            success = true,
                            message = "Aucune scène n'est configurée.",
                            scenes = new List<object>()
                        };
                    }

                    var sceneList = scenes.Select(s => new
                    {
                        id = s.Id,
                        name = s.Name,
                        description = s.Description,
                        areaId = s.AreaId,
                        isActive = s.IsActive,
                        canBeInvoked = true // Les scènes peuvent être invoquées via execute_scene
                    }).ToList();

                    return new
                    {
                        success = true,
                        scenes = sceneList,
                        count = sceneList.Count,
                        message = $"{sceneList.Count} scène(s) disponible(s)."
                    };
                }
                catch (System.Exception ex)
                {
                    return new
                    {
                        success = false,
                        error = $"Erreur lors de la récupération des scènes: {ex.Message}"
                    };
                }
            });
        }
    }
}
