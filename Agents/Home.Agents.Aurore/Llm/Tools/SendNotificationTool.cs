using Home.Common;
using Home.Common.Model;
using Home.Graph.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Home.Agents.Aurore.Llm.Tools
{
    /// <summary>
    /// Outil permettant d'envoyer une notification à un utilisateur
    /// </summary>
    public class SendNotificationTool : ILlmTool
    {
        public string Name => "send_notification";

        public string Description => "Envoie une notification push à un utilisateur spécifique de la maison.";

        public Dictionary<string, object> ParametersSchema => new Dictionary<string, object>
        {
            ["type"] = "object",
            ["properties"] = new Dictionary<string, object>
            {
                ["userId"] = new Dictionary<string, object>
                {
                    ["type"] = "string",
                    ["description"] = "ID de l'utilisateur (ex: 'john', 'marie')"
                },
                ["title"] = new Dictionary<string, object>
                {
                    ["type"] = "string",
                    ["description"] = "Titre de la notification"
                },
                ["message"] = new Dictionary<string, object>
                {
                    ["type"] = "string",
                    ["description"] = "Message de la notification"
                },
                ["importance"] = new Dictionary<string, object>
                {
                    ["type"] = "string",
                    ["description"] = "Importance de la notification : 'low', 'normal', 'high'",
                    ["enum"] = new List<string> { "low", "normal", "high" }
                }
            },
            ["required"] = new List<string> { "userId", "message" }
        };

        public async Task<object> ExecuteAsync(Dictionary<string, object> parameters, CancellationToken cancellationToken = default)
        {
            if (!parameters.ContainsKey("userId") || !parameters.ContainsKey("message"))
                return new { success = false, error = "Paramètres 'userId' et 'message' requis" };

            string userId = parameters["userId"].ToString();
            string message = parameters["message"].ToString();
            string title = parameters.ContainsKey("title") ? parameters["title"].ToString() : "Notification Aurore";
            string importance = parameters.ContainsKey("importance") ? parameters["importance"].ToString() : "normal";

            try
            {
                using (var client = new MainApiAgentWebClient("aurore"))
                {
                    var notification = new
                    {
                        title = title,
                        message = message,
                        importance = importance,
                        sendMobile = true
                    };

                    string result = await Task.Run(() => client.UploadString(
                        $"v1.0/users/{userId}/notify",
                        "POST",
                        JsonConvert.SerializeObject(notification)));

                    return new { success = true, message = $"Notification envoyée à {userId}" };
                }
            }
            catch (Exception ex)
            {
                return new { success = false, error = ex.Message };
            }
        }
    }
}
