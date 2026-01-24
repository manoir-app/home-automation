using Home.Common;
using Home.Common.Model;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Home.Agents.Aurore.Llm.Tools
{
    /// <summary>
    /// Outil permettant de récupérer les utilisateurs actuellement présents dans la maison
    /// </summary>
    public class GetPresentUsersTool : ILlmTool
    {
        public string Name => "get_present_users";

        public string Description => "Récupère la liste des utilisateurs actuellement présents dans la maison locale.";

        public Dictionary<string, object> ParametersSchema => new Dictionary<string, object>
        {
            ["type"] = "object",
            ["properties"] = new Dictionary<string, object>(),
            ["required"] = new List<string>()
        };

        public Task<object> ExecuteAsync(Dictionary<string, object> parameters, CancellationToken cancellationToken = default)
        {
            var users = AgentHelper.GetLocalPresentUsers("aurore");
            
            if (users == null || users.Count == 0)
                return Task.FromResult<object>(new { users = new List<object>(), message = "Aucun utilisateur présent actuellement" });

            var result = users.Select(u => new
            {
                id = u.Id,
                name = u.Name,
                firstName = u.FirstName,
                fullName = $"{u.FirstName} {u.Name}"
            }).ToList();

            return Task.FromResult<object>(new { users = result, count = result.Count });
        }
    }
}
