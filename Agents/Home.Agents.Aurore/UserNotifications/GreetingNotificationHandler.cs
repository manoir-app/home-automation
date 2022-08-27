using Home.Agents.Aurore.Greetings;
using Home.Common;
using Home.Common.Messages;
using Home.Common.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;

namespace Home.Agents.Aurore.UserNotifications
{
    public static class GreetingNotificationHandler
    {
        public static MessageResponse HandleMessage(MessageOrigin origin, string topic, string messageBody)
        {
            if (!topic.Equals("communication.notification.mobile.send.greetings"))
                return MessageResponse.GenericFail;

            var gts = JsonConvert.DeserializeObject<SendMobileNotificationMessage>(messageBody);

            var rq = new GreetingsMessage("")
            {

            };

            var usr = AgentHelper.GetUser("aurore", gts.User);
            if (usr == null)
                return MessageResponse.GenericFail;

            rq.Users.Add(usr);
            rq.Destination = GreetingsMessage.GreetingsDestination.UserApp;

            var resp = SingleUserGreetings.GetGreetings(rq, true);

            if (resp != null)
            {
                StringBuilder blrContent = new StringBuilder();
                StringBuilder blrTitre = new StringBuilder();

                foreach (var item in resp.Items)
                {
                    if (item.ContentKind == GreetingsMessageResponseItem.GreetingsMessageResponseItemKind.HeaderContent)
                    {
                        if (blrTitre.Length == 0)
                            blrTitre.Append(item.Content);
                    }
                    else if (item.ContentKind == GreetingsMessageResponseItem.GreetingsMessageResponseItemKind.MainContent)
                    {
                        if (blrContent.Length > 0)
                            blrContent.AppendLine();
                        blrContent.AppendLine(item.Content);
                    }
                    else if (item.ContentKind == GreetingsMessageResponseItem.GreetingsMessageResponseItemKind.DateContent)
                    {
                        if (blrContent.Length > 0)
                            blrContent.AppendLine();
                        blrContent.AppendLine(item.Content);
                    }

                }

                UserNotification not = new UserNotification()
                {
                    Date = DateTimeOffset.Now,
                    Description = blrContent.ToString(),
                    Title = blrTitre.ToString(),
                    Id = Guid.NewGuid().ToString(),
                    UserId = gts.User,
                    Importance = UserNotificationImportance.Normal
                };

                for (int i = 0; i < 3; i++)
                {
                    try
                    {
                        using (var cli = new MainApiAgentWebClient("aurore"))
                        {
                            var t = cli.UploadData<bool, UserNotification>($"v1.0/users/{gts.User}/notify?sendToMobile=true",
                                "POST", not);
                            return MessageResponse.OK;
                        }
                    }
                    catch (WebException ex)
                    {
                        Thread.Sleep(1000);
                    }
                }


            }

            return MessageResponse.GenericFail;
        }



    }
}
