using Home.Common.Messages;
using Home.Common.Model;
using Home.Graph.Common;
using Home.Graph.Server.Hubs;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Home.Graph.Server.Controllers
{
    partial class UsersController
    {
        [Route("presence/mesh/local/all"), HttpGet]
        public List<User> GetLocalPresentUsers()
        {
            return UserHelper.GetLocalPresentUsers();
        }

        [Route("presence/notifyactivity"), HttpPost]
        public bool NotifyPresenceActivity([FromBody] PresenceNotificationData data)
        {
            if (data.Date == null)
                data.Date = DateTimeOffset.Now;

            MessagingHelper.PushToLocalAgent(new PresenceNotificationMessage(data));
            return true;
        }

        [Route("presence/{userName}/forcelocation/{locationId}/{status}"), HttpGet]
        public bool ForceLocation(string userName, string locationId, string status)
        {
            var data = new PresenceNotificationData()
            {
                ActivityKind = "forcelocation",
                AssociatedUser = userName,
                Date = DateTimeOffset.Now,
                IsUserInput = true,
                Status = status,
                LocationId = locationId
            };

            var loc = new LocationsController().Get(locationId);
            LogHelper.Log("user", "presence", $"User {userName} : {status} at {(loc == null ? locationId : loc.Name)}");

            MessagingHelper.PushToLocalAgent(new PresenceNotificationMessage(data));
            return true;
        }

        public bool ForceLocationFromEvent(string userName, string locationId, string status)
        {
            var data = new PresenceNotificationData()
            {
                ActivityKind = "fromevent",
                AssociatedUser = userName,
                Date = DateTimeOffset.Now,
                IsUserInput = true,
                Status = status,
                LocationId = locationId
            };

            var loc = new LocationsController().Get(locationId);
            LogHelper.Log("user", "presence", $"User {userName} : {status} at {(loc == null ? locationId : loc.Name)} (from event)");


            MessagingHelper.PushToLocalAgent(new PresenceNotificationMessage(data));
            return true;
        }


        [Route("all/{userName}/presence"), HttpPost]
        public PresenceData UpdateUserPresence(string userName, [FromBody] PresenceUpdateData update)
        {
            if (userName == null)
                return null;

            var collection = MongoDbHelper.GetClient<User>();
            var usr = collection.Find(x => x.Id == userName).FirstOrDefault();
            var oldLoc = (from z in usr.Presence.Location
                          orderby z.Probability descending
                          select z).FirstOrDefault();

            if (usr != null)
            {
                if (update.ActivityToLog != null)
                    usr.Presence.LatestActivities.Add(update.ActivityToLog);
                if (update.Location != null)
                {
                    var pres = (from z in usr.Presence.Location
                                where z.LocationId == update.Location.LocationId
                                select z).FirstOrDefault();
                    if (pres == null)
                        usr.Presence.Location.Add(update.Location);
                    else
                    {
                        pres.LatestUpdate = update.Location.LatestUpdate;
                        pres.Probability = update.Location.Probability;
                    }
                }

                usr.Presence.Location.Sort((a, b) => a.Probability - b.Probability);
                usr.Presence.Location = (from z in usr.Presence.Location
                                         where z.Probability > 0
                                         select z).ToList();

                usr.Presence.LatestActivities = (from z in usr.Presence.LatestActivities
                                                 where z.Date > DateTimeOffset.Now.AddDays(-2)
                                                 select z).ToList();

                var presUpdate = Builders<User>.Update
                    .Set("Presence", usr.Presence);
                collection.UpdateOne(c => c.Id == usr.Id, presUpdate);

                var loc = usr.Presence.Location.FirstOrDefault();
                usr.ForPresence();
                if (loc != null)
                {
                    if (oldLoc == null || !oldLoc.LocationId.Equals(loc.LocationId))
                    {
                        MessagingHelper.PushToLocalAgent(new PresenceChangedMessage(usr.Id, usr.Presence));
                        AppAndDeviceHub.SendUserChange(_appContext, "presence", usr);
                    }

                    if (loc.Probability > 50)
                    {
                        var locColl = MongoDbHelper.GetClient<Location>();
                        var laLoc = locColl.Find(x => x.Id == loc.LocationId).FirstOrDefault();
                        if (!usr.IsGuest)
                        {
                            if (oldLoc == null || !oldLoc.LocationId.Equals(loc.LocationId))
                                LogHelper.Log("user", "presence", $"User {userName} : present at {laLoc.Name} (from presence data)");
                            MqttHelper.PublishUserPresence(usr, laLoc);
                        }
                    }
                }
                else
                {
                    if (oldLoc != null)
                        MessagingHelper.PushToLocalAgent(new PresenceChangedMessage(usr.Id, usr.Presence));

                    AppAndDeviceHub.SendUserChange(_appContext, "presence", usr);
                    if (!usr.IsGuest)
                    {
                        if (oldLoc != null)
                            LogHelper.Log("user", "presence", $"User {userName} : presence cleared");
                        MqttHelper.PublishUserPresence(usr, null);
                    }
                }

                return usr.Presence;

            }
            return null;
        }


    }
}
