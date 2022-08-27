using Home.Common;
using Home.Common.Messages;
using Home.Common.Model;
using Microsoft.Azure.NotificationHubs;
using MongoDB.Driver;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Home.Graph.Common
{
    public static class MessagingHelper
    {
        public static void PushItemChange<T>(string id, ItemChangeKind changeKind)
        {
            var messageTopic = "data.change." + typeof(T).FullName.ToLowerInvariant();
            var msg = new ItemChangeMessage(messageTopic)
            {
                ItemId = id,
                Kind = changeKind
            };
            NatsMessageThread.Push(msg);
            OnMessagePushed(msg);
        }

        public static void PushToLocalAgent(BaseMessage message)
        {
            NatsMessageThread.Push(message);
            OnMessagePushed(message);
        }

        public static void PushToLocalAgent(string messageTopic, string content)
        {
            NatsMessageThread.Push(messageTopic, content);
            OnMessagePushed(new AgentGenericMessage(messageTopic) { MessageContent = content });
        }

        public static T RequestFromLocalAgent<T>(BaseMessage message) where T : MessageResponse
        {
            var ret = NatsMessageThread.Request<T>(message.Topic, message);
            // on attend le retour pour 
            OnMessagePushed(message, ret);
            return ret;
        }

        public static event EventHandler<MessagePushEventArgs> MessagePushed;

        private static void OnMessagePushed(BaseMessage msg, MessageResponse resp = null)
        {
            Console.WriteLine("Pushed message on topic : " + msg.Topic);
            MessagePushed?.Invoke(typeof(MessagingHelper),
                new MessagePushEventArgs()
                {
                    Message = msg,
                    Response = resp
                });
        }

        public class MessagePushEventArgs : EventArgs
        {
            public BaseMessage Message { get; set; }
            public MessageResponse Response { get; set; }

            public T As<T>() where T : BaseMessage
            {
                return Message as T;
            }

            public T ResponseAs<T>() where T : MessageResponse
            {
                return Response as T;
            }
        }

        /// <summary>
        /// Private class for Firebase notification
        /// </summary>
        class FirebaseNotifContent
        {
            public FirebaseNotifContent()
            {
                sound = "default";
            }

            public string tag { get; set; }
            public string title { get; set; }
            public string icon { get; set; }
            public string body { get; set; }
            public string sound { get; set; }
            public string android_channel_id { get; set; }
        }

        /// <summary>
        /// Private class for Firebase notification
        /// </summary>
        class FirebaseNotif
        {
            public FirebaseNotifContent notification { get; set; }
            public Dictionary<string, string> data { get; set; }
        }

        /// <summary>
        /// Sends a notification to all devices assigned to a user
        /// </summary>
        /// <param name="user">User id</param>
        /// <param name="title">Title of notification</param>
        /// <param name="description">Content of notification</param>
        /// <param name="moreData">Key/data pair for the app</param>
        public static void PushToMobileApp(string user, string title, string description, Dictionary<string, string> moreData = null)
        {
            UserNotification not = new UserNotification()
            {
                CustomValues = moreData,
                Date = DateTimeOffset.Now,
                Description = description,
                Title = title,
                Id = Guid.NewGuid().ToString(),
                UserId = user,
                Importance = UserNotificationImportance.Normal
            };
            PushToMobileApp(user, not);
        }

        /// <summary>
        /// Sends a notification to all devices assigned to a user
        /// </summary>
        /// <param name="user">User id</param>
        /// <param name="notif">Notification to send</param>
        public static void PushToMobileApp(string user, UserNotification notif)
        {
            var coll = MongoDbHelper.GetClient<AutomationMesh>();
            var mesh = coll.Find(w => w.Id == "local").FirstOrDefault();
            if (mesh != null)
                PushToMobileApp(user, mesh, notif);
        }




        /// <summary>
        /// Sends a notification to all devices assiged to a user
        /// </summary>
        /// <param name="user">User id</param>
        /// <param name="mesh">Current automation mesh</param>
        /// <param name="notif">Notification to send</param>
        public static void PushToMobileApp(string user, AutomationMesh mesh, UserNotification notif)
        {
            var mes = Guid.Parse(mesh.PublicId);
            var hubcnstring = ConfigurationSettingsHelper.GetAzureNotificationHub();
            var hub = NotificationHubClient.CreateClientFromConnectionString(hubcnstring, "home-automation");
            string tagExpression = "user_" + user;
            tagExpression += " && mesh_" + mes.ToString("N").ToUpperInvariant();
            notif.Title = notif.Title.Replace("**", "");
            notif.Description = notif.Description.Replace("**", "");

            var fullnotif = new FirebaseNotif()
            {
                notification = new FirebaseNotifContent()
                {
                    title = notif.Title,
                    body = notif.Description,
                    icon = "ic_notify",
                    tag = notif.Id
                },
                data = notif.CustomValues
            };

            if (fullnotif.data == null)
            {
                fullnotif.data = new Dictionary<string, string>();
                fullnotif.data.Add("custom_category", notif.Category ?? "default");
            }
            else if (!fullnotif.data.ContainsKey("custom_category"))
                fullnotif.data.Add("custom_category", notif.Category ?? "default");

            if (notif.Category != null)
            {

                switch (notif.Category.ToLowerInvariant())
                {
                    case "default":
                        break;
                    case "alert":
                    case "alerts":
                        fullnotif.notification.android_channel_id = SendMobileNotificationMessage.ALERTS_CHANNEL_ID;
                        break;
                    case "messages":
                    case "personal":
                        fullnotif.notification.android_channel_id = SendMobileNotificationMessage.PERSONAL_CHANNEL_ID;
                        break;
                    case "chat":
                        fullnotif.notification.android_channel_id = SendMobileNotificationMessage.CHAT_CHANNEL_ID;
                        break;
                    case "downloads":
                        fullnotif.notification.android_channel_id = SendMobileNotificationMessage.DOWNLOADS_CHANNEL_ID;
                        break;
                }
            }

            string json = JsonConvert.SerializeObject(fullnotif);
            Console.WriteLine($"Sending to {tagExpression} : {json}");
            var t = hub.SendFcmNativeNotificationAsync(json, tagExpression).Result;
            Console.WriteLine($"Message sent to {t.Success} with sucess, {t.Failure} with errors on notification id {t.NotificationId}(tracking:{t.TrackingId}");
        }


        /// <summary>
        /// Sends a notification to all devices assigned to a user
        /// </summary>
        /// <param name="user">User id</param>
        /// <param name="notif">Notification to send</param>
        public static void PushToMobileApp(string user, Dictionary<string, string> data)
        {
            var coll = MongoDbHelper.GetClient<AutomationMesh>();
            var mesh = coll.Find(w => w.Id == "local").FirstOrDefault();
            if (mesh != null)
                PushToMobileApp(user, mesh, data);
        }
        /// <summary>
        /// Sends a data notification to all devices assigned to a user
        /// </summary>
        /// <param name="user">User id</param>
        /// <param name="mesh">Current automation mesh</param>
        /// <param name="data">les datas à envoyer</param>
        public static void PushToMobileApp(string user, AutomationMesh mesh, Dictionary<string, string> data)
        {
            var mes = Guid.Parse(mesh.PublicId);
            var hubcnstring = ConfigurationSettingsHelper.GetAzureNotificationHub();
            var hub = NotificationHubClient.CreateClientFromConnectionString(hubcnstring, "home-automation");
            string tagExpression = "user_" + user;
            tagExpression += " && mesh_" + mes.ToString("N").ToUpperInvariant();

            var fullnotif = new FirebaseNotif()
            {
                data = data
            };

            string json = JsonConvert.SerializeObject(fullnotif);

            Console.WriteLine($"Sending to {tagExpression} : {json}");
            var t = hub.SendFcmNativeNotificationAsync(json, tagExpression).Result;
            Console.WriteLine($"Message sent to {t.Success} with sucess, {t.Failure} with errors on notification id {t.NotificationId}(tracking:{t.TrackingId}");
        }

    }
}
