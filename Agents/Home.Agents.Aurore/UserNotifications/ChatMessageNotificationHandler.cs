using Home.Agents.Aurore.Greetings;
using Home.Common;
using Home.Common.Messages;
using Home.Common.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Linq;
using System.Text;
using System.Threading;

namespace Home.Agents.Aurore.UserNotifications
{
    public static class ChatMessageNotificationHandler
    {
        public static MessageResponse HandleMessage(MessageOrigin origin, string topic, string messageBody)
        {
            if (!topic.Equals("pim.chat.activity"))
                return MessageResponse.GenericFail;

            var gts = JsonConvert.DeserializeObject<ChatActivityMessage>(messageBody);

            ChatChannel channel = null;

            for (int i = 0; i < 3; i++)
            {
                try
                {
                    using (var cli = new MainApiAgentWebClient("aurore"))
                    {
                        channel = cli.DownloadData<ChatChannel>($"v1.0/chat/channels/{gts.ChannelId}");
                        break;
                    }
                }
                catch (WebException ex)
                {
                    Thread.Sleep(1000);
                }
            }

            if (channel == null)
                return MessageResponse.GenericFail;

            var usrs = AgentHelper.GetMainUsers("aurore");

            StringBuilder blrTitre = new StringBuilder();

            var usrSender = (from z in usrs
                             where z.Id.Equals(gts.FromUserId)
                             select z).FirstOrDefault();

            if (usrSender == null)
                blrTitre.Append($"{gts.FromUserId} vient de dire dans {channel.Name}");
            else
                blrTitre.Append($"{(usrSender.CommonName == null ? usrSender.Name:usrSender.CommonName)} vient de dire dans {channel.Name}");

            foreach (var usr in usrs)
            {
                if (!usr.IsMain)
                    continue;

                if (usr.Id.Equals(gts.FromUserId, StringComparison.InvariantCultureIgnoreCase))
                    continue;

                UserNotification not = new UserNotification()
                {
                    Date = DateTimeOffset.Now,
                    Description = gts.Content,
                    Title = blrTitre.ToString(),
                    Id = Guid.NewGuid().ToString(),
                    UserId = usr.Id,
                    Importance = UserNotificationImportance.Normal
                };

                for (int i = 0; i < 3; i++)
                {
                    try
                    {
                        using (var cli = new MainApiAgentWebClient("aurore"))
                        {
                            var t = cli.UploadData<bool, UserNotification>($"v1.0/users/{usr.Id}/notify?sendToMobile=true",
                                "POST", not);
                            break;
                        }
                    }
                    catch (WebException ex)
                    {
                        Thread.Sleep(1000);
                    }
                }
            }

            return MessageResponse.OK;
        }



    }
}
