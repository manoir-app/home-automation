using Home.Common;
using Home.Common.Messages;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Home.Agents.Aurore
{
    public static class AuroreMessageHandler
    {
        public static void Start()
        {

            Thread t = new Thread(() => NatsMessageThread.Run(new string[] { "aurore.>",
                                    "pim.chat.>",
                                    "system.mesh.privacymode.changed",
                                    "greetings.>",
                                    "users.accounts.>",
                                    "communication.>", "security.alarm"},
            AuroreMessageHandler.HandleMessage));
            t.Name = "NatsThread";
            t.Start();
        }


        public static void Stop()
        {
            NatsMessageThread.Stop();
        }



        public static MessageResponse HandleMessage(MessageOrigin origin, string topic, string messageBody)
        {
            if (messageBody == null)
                return MessageResponse.GenericFail;

            messageBody = messageBody.Trim();
            Console.WriteLine("------------------------");
            Console.WriteLine("Message :");
            Console.WriteLine(messageBody);
            Console.WriteLine("------------------------");
            MessageResponse ret;

            switch (topic.ToLowerInvariant())
            {
                case "aurore.stop":
                    Program._stop = true;
                    return MessageResponse.OK;
                case GreetingsMessage.SimpleGetGreetings:
                    ret = Greetings.GreetingsHandler.GetGreetings(JsonConvert.DeserializeObject<GreetingsMessage>(messageBody));
                    if (ret == null)
                        return MessageResponse.GenericFail;
                    return ret;
                case "system.mesh.privacymode.changed":
                    return UserNotifications.PrivacyModeChangedNotificationHandler.HandleMessage(origin, topic, messageBody);
                case ChatActivityMessage.TopicName:
                    return UserNotifications.ChatMessageNotificationHandler.HandleMessage(origin, topic, messageBody);
                case "communication.homestatus.items.get":
                    return AuroreNewsItemService.HandleMessage(origin, topic, messageBody);
                case "communication.notification.mobile.send.greetings":
                    return UserNotifications.GreetingNotificationHandler.HandleMessage(origin, topic, messageBody);
                case UserChangeMessage.DeletedTopic:
                case UserChangeMessage.GuestDeletedTopic:
                case UserChangeMessage.UpdatedTopic:
                case UserChangeMessage.GuestUpdatedTopic:
                case UserChangeMessage.CreatedTopic:
                case UserChangeMessage.GuestCreatedTopic:
                    ret =  UserNotifications.UserChangeNotificationHandler.HandleMessage(origin, topic, messageBody);
                    Console.WriteLine("Réponse : ");
                    Console.WriteLine(JsonConvert.SerializeObject(ret));
                    return ret;
                default:
                    return MessageResponse.GenericFail;
            }
        }


    }
}
