using Home.Common.Llm;
using Home.Common.Model;
using Home.Graph.Services;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Home.Graph.Server.Services.Llm.Tools
{
    /// <summary>
    /// Outil pour envoyer une notification à un utilisateur
    /// </summary>
    public class SendNotificationTool : ILlmTool
    {
        private readonly UserService _userService;

        public SendNotificationTool()
        {
            _userService = new UserService();
        }

        public string Name => "send_notification";

        public string Description =>
            "Envoie une notification push à un utilisateur spécifique. Utile pour rappeler quelque chose ou alerter.";

        public Dictionary<string, object> ParametersSchema => new Dictionary<string, object>
        {
            ["type"] = "object",
            ["properties"] = new Dictionary<string, object>
            {
                ["userId"] = new Dictionary<string, object>
                {
                    ["type"] = "string",
                    ["description"] = "ID de l'utilisateur à notifier"
                },
                ["message"] = new Dictionary<string, object>
                {
                    ["type"] = "string",
                    ["description"] = "Message de la notification"
                },
                ["title"] = new Dictionary<string, object>
                {
                    ["type"] = "string",
                    ["description"] = "Titre de la notification (optionnel)"
                },
                ["importance"] = new Dictionary<string, object>
                {
                    ["type"] = "string",
                    ["enum"] = new List<string> { "Low", "Normal", "High", "Critical" },
                    ["description"] = "Importance de la notification (optionnel, par défaut: Normal)"
                }
            },
            ["required"] = new List<string> { "userId", "message" }
        };

        public async Task<object> ExecuteAsync(Dictionary<string, object> parameters, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!parameters.ContainsKey("userId") || !parameters.ContainsKey("message"))
                {
                    return new
                    {
                        success = false,
                        error = "userId et message sont requis"
                    };
                }

                var userId = parameters["userId"].ToString();
                var message = parameters["message"].ToString();
                var title = parameters.ContainsKey("title") ? parameters["title"].ToString() : "Notification";
                var importanceStr = parameters.ContainsKey("importance") ? parameters["importance"].ToString() : "Normal";

                UserNotificationImportance importance = UserNotificationImportance.Normal;
                Enum.TryParse(importanceStr, true, out importance);

                var notification = new UserNotification
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = userId,
                    Title = title,
                    Description = message,
                    Importance = importance,
                    Date = DateTimeOffset.Now,
                    Category = "llm-assistant"
                };

                var sent = await _userService.SendNotificationAsync(userId, notification);

                if (sent)
                {
                    return new
                    {
                        success = true,
                        message = "Notification envoyée avec succès",
                        userId = userId
                    };
                }
                else
                {
                    return new
                    {
                        success = false,
                        error = "Échec de l'envoi de la notification"
                    };
                }
            }
            catch (Exception ex)
            {
                return new
                {
                    success = false,
                    error = $"Erreur lors de l'envoi de la notification: {ex.Message}"
                };
            }
        }
    }
}
