using Home.Agents.Aurore.Greetings;
using Home.Agents.Aurore.Integrations;
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

            // Récupérer les utilisateurs présents
            var presentUsers = PrivacyModeChangedNotificationHandler.GetLocalPresentUsers();
            
            // Vérifier les conditions pour l'envoi de notifications Awtrix
            bool shouldSendAwtrixNotification = ShouldSendAwtrixNotification(gts.FromUserId, channel, presentUsers);

            StringBuilder blrTitre = new StringBuilder();

            var usrSender = (from z in usrs
                             where z.Id.Equals(gts.FromUserId)
                             select z).FirstOrDefault();

            string senderName = usrSender != null
                ? (usrSender.CommonName ?? usrSender.Name ?? usrSender.Id)
                : gts.FromUserId;

            if (usrSender == null)
                blrTitre.Append($"{gts.FromUserId} vient de dire dans {channel.Name}");
            else
                blrTitre.Append($"{senderName} vient de dire dans {channel.Name}");

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

            // Envoyer la notification Awtrix si les conditions sont remplies
            if (shouldSendAwtrixNotification)
            {
                AwtrixHelper.SendChatNotification(senderName, gts.Content, channel.Name);
                DivoomHelper.SendChatNotification(senderName, gts.Content, channel.Name);
            }

            return MessageResponse.OK;
        }

        /// <summary>
        /// Détermine si une notification Awtrix doit être envoyée
        /// </summary>
        /// <param name="senderId">ID de l'expéditeur du message</param>
        /// <param name="channel">Canal de chat</param>
        /// <param name="presentUsers">Liste des utilisateurs présents</param>
        /// <returns>True si la notification doit être envoyée</returns>
        private static bool ShouldSendAwtrixNotification(string senderId, ChatChannel channel, List<User> presentUsers)
        {
            if (presentUsers == null || !presentUsers.Any())
            {
                Console.WriteLine("[Awtrix Chat] Aucun utilisateur présent");
                return false;
            }

            // 1) Vérifier si l'expéditeur est présent => pas de notification
            bool senderIsPresent = presentUsers.Any(u => u.Id.Equals(senderId, StringComparison.InvariantCultureIgnoreCase));
            if (senderIsPresent)
            {
                Console.WriteLine($"[Awtrix Chat] L'expéditeur {senderId} est présent, pas de notification");
                return false;
            }

            // 2) Vérifier si au moins un membre du canal est présent => notification
            bool anyChannelMemberPresent = channel.UserIds.Any(channelUserId =>
                presentUsers.Any(presentUser => presentUser.Id.Equals(channelUserId, StringComparison.InvariantCultureIgnoreCase))
            );

            if (!anyChannelMemberPresent)
            {
                Console.WriteLine($"[Awtrix Chat] Aucun membre du canal '{channel.Name}' n'est présent, pas de notification");
                return false;
            }

            Console.WriteLine($"[Awtrix Chat] Conditions remplies pour notification du canal '{channel.Name}'");
            return true;
        }
    }
}
