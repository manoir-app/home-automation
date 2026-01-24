using Home.Common;
using Home.Graph.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Home.Agents.Aurore.Llm.Tools
{
    /// <summary>
    /// Outil permettant de récupérer les tâches TODO d'un utilisateur
    /// </summary>
    public class GetUserTodosTool : ILlmTool
    {
        public string Name => "get_user_todos";

        public string Description => "Récupère les tâches TODO d'un utilisateur spécifique, avec possibilité de filtrer par statut (pending, completed, etc.).";

        public Dictionary<string, object> ParametersSchema => new Dictionary<string, object>
        {
            ["type"] = "object",
            ["properties"] = new Dictionary<string, object>
            {
                ["userId"] = new Dictionary<string, object>
                {
                    ["type"] = "string",
                    ["description"] = "ID de l'utilisateur"
                },
                ["status"] = new Dictionary<string, object>
                {
                    ["type"] = "string",
                    ["description"] = "Statut des tâches à récupérer (pending, completed, all)",
                    ["enum"] = new List<string> { "pending", "completed", "all" }
                },
                ["limit"] = new Dictionary<string, object>
                {
                    ["type"] = "integer",
                    ["description"] = "Nombre maximum de tâches à retourner (par défaut: 10)",
                    ["default"] = 10
                }
            },
            ["required"] = new List<string> { "userId" }
        };

        public async Task<object> ExecuteAsync(Dictionary<string, object> parameters, CancellationToken cancellationToken = default)
        {
            if (!parameters.ContainsKey("userId"))
                return new { success = false, error = "Paramètre 'userId' requis" };

            string userId = parameters["userId"].ToString();
            string status = parameters.ContainsKey("status") ? parameters["status"].ToString() : "pending";
            int limit = parameters.ContainsKey("limit") ? Convert.ToInt32(parameters["limit"]) : 10;

            try
            {
                using (var client = new MainApiAgentWebClient("aurore"))
                {
                    // Récupère les TODOs via l'API en JSON brut
                    string jsonResult = await Task.Run(() => client.DownloadString(
                        $"v1.0/todos/todoItems?userId={userId}&days=30"));

                    if (string.IsNullOrEmpty(jsonResult))
                        return new { success = true, todos = new List<object>(), count = 0 };

                    // Parser le JSON
                    var todos = JsonConvert.DeserializeObject<JArray>(jsonResult);
                    if (todos == null || todos.Count == 0)
                        return new { success = true, todos = new List<object>(), count = 0 };

                    // Filtrer et formater les résultats
                    var result = new List<object>();
                    int count = 0;

                    foreach (var todo in todos)
                    {
                        if (count >= limit)
                            break;

                        var todoStatus = todo["status"]?.ToString() ?? "";
                        
                        // Filtrer par statut
                        if (status == "pending" && todoStatus == "Done")
                            continue;
                        if (status == "completed" && todoStatus != "Done")
                            continue;

                        result.Add(new
                        {
                            id = todo["id"]?.ToString(),
                            title = todo["title"]?.ToString(),
                            status = todoStatus,
                            dueDate = todo["dueDate"]?.ToString(),
                            priority = todo["priority"]?.ToString()
                        });

                        count++;
                    }

                    return new { success = true, todos = result, count = result.Count };
                }
            }
            catch (Exception ex)
            {
                return new { success = false, error = ex.Message };
            }
        }
    }
}
