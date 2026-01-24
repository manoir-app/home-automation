using Home.Common.Llm;
using Home.Graph.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Home.Graph.Server.Services.Llm.Tools
{
    /// <summary>
    /// Outil pour lister les utilisateurs présents à la maison
    /// </summary>
    public class GetPresentUsersTool : ILlmTool
    {
        private readonly UserService _userService;

        public GetPresentUsersTool()
        {
            _userService = new UserService();
        }

        public string Name => "get_present_users";

        public string Description => 
            "Récupère la liste des utilisateurs actuellement présents à la maison (basé sur leur statut de présence).";

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
                    var users = _userService.GetPresentUsers();
                    
                    if (users == null || users.Count == 0)
                    {
                        return new
                        {
                            success = true,
                            message = "Aucun utilisateur n'est présent actuellement.",
                            presentUsers = new List<object>()
                        };
                    }

                    var userList = users.Select(u => new
                    {
                        id = u.Id,
                        name = u.Name,
                        commonName = u.CommonName ?? u.Name,
                        presenceStatus = "present"
                    }).ToList();

                    return new
                    {
                        success = true,
                        presentUsers = userList,
                        count = userList.Count
                    };
                }
                catch (System.Exception ex)
                {
                    return new
                    {
                        success = false,
                        error = $"Erreur lors de la récupération des utilisateurs: {ex.Message}"
                    };
                }
            });
        }
    }
}
