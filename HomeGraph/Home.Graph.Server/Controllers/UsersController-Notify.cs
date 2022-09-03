using Home.Common;
using Home.Common.Model;
using Home.Graph.Common;
using Home.Graph.Server.Hubs;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.NotificationHubs;
using MongoDB.Driver;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Home.Graph.Server.Controllers
{
    partial class UsersController
    {
        [Route("{user}/notifications/clearreaditems"), HttpGet]
        public bool ClearReadItems(string user)
        {
            if (user == null)
                return false;
            DateTimeOffset dt = DateTime.Now.AddMinutes(-1);
            user = user.ToLowerInvariant();
            var collection = MongoDbHelper.GetClient<UserNotification>();
            var ret = collection.DeleteMany(x => x.UserId == user
                            && x.IsRead && x.Date < dt);
            return ret.IsAcknowledged;
        }

        [Route("{user}/notifications/markallasread"), HttpGet]
        public bool MarkAllAsRead(string user)
        {
            if (user == null)
                return false;

            user = user.ToLowerInvariant();

            var collection = MongoDbHelper.GetClient<UserNotification>();

            var up = Builders<UserNotification>.Update
                .Set("IsRead", true);

            var dt = DateTimeOffset.Now.AddSeconds(-5);

            collection.UpdateMany(x => x.UserId == user
                            && x.Date < dt, up);
            return true;
        }

        [Route("{user}/notifications"), HttpGet]
        public IEnumerable<UserNotification> GetNotifications(string user)
        {
            if (user == null)
                return null;

            user = user.ToLowerInvariant();

            var collection = MongoDbHelper.GetClient<UserNotification>();

            var lst = collection.Find(x => x.UserId == user).ToList();

            lst.Sort((a, b) => b.Date.CompareTo(a.Date));
            return lst;
        }

        [Route("{user}/notifications/{notifId}/markasread"), HttpGet]
        public bool MarkAsRead(string user, string notifID)
        {
            if (user == null)
                return false;

            user = user.ToLowerInvariant();

            var collection = MongoDbHelper.GetClient<UserNotification>();

            var up = Builders<UserNotification>.Update
                .Set("IsRead", true);

            collection.UpdateOne(x => x.UserId == user
                            && x.Id == notifID, up);
            return true;
        }

        [Route("{user}/notify"), Route("{user}/notifications"), HttpPost]
        public bool NotifyUser(string user, [FromBody] UserNotification notif, bool? sendToMobile = null)
        {
            if (user == null)
                return false;

            user = user.ToLowerInvariant();
            notif.UserId = user;

            UserNotification exists = null;
            var collection = MongoDbHelper.GetClient<UserNotification>();

            if (!string.IsNullOrEmpty(notif.SourceAgent))
                notif.SourceAgent = notif.SourceAgent.ToLowerInvariant();

            if (!string.IsNullOrEmpty(notif.SourceAgent)
                && !string.IsNullOrEmpty(notif.SourceAgentNotificationId))
            {
                exists = collection.Find(x => x.UserId == user
                            && x.SourceAgent == notif.SourceAgent
                            && x.SourceAgentNotificationId == notif.SourceAgentNotificationId).FirstOrDefault();
            }
            else if (!string.IsNullOrEmpty(notif.Id))
            {
                notif.Id = notif.Id.ToLowerInvariant();
                exists = collection.Find(x => x.Id == notif.Id).FirstOrDefault();
            }

            if(exists==null)
            {
                notif.Id = Guid.NewGuid().ToString().ToLowerInvariant();
                collection.InsertOne(notif);
            }
            else
            {
                notif.Id = exists.Id;
                notif.IsRead = exists.IsRead;
                collection.ReplaceOne(x => x.Id == notif.Id, notif);
            }

            if(!sendToMobile.HasValue)
            {
                switch(notif.Importance)
                {
                    case UserNotificationImportance.High:
                    case UserNotificationImportance.Critical:
                        sendToMobile = true;
                        break;
                    default:
                        sendToMobile = false;
                        break;
                }
            }


            if(sendToMobile.GetValueOrDefault())
            {
                var mesh = new AutomationMeshController(_hubContext, _appContext).Get("local");
                if (mesh!=null)
                    MessagingHelper.PushToMobileApp(user, mesh, notif);
            }

            if (_usersContext != null)
                UsersHub.SendNewNotification(_usersContext, user, notif);

            return true;
        }


       
    }

}