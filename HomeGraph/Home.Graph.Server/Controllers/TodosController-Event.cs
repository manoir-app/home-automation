using Home.Common.Messages;
using Home.Common.Model;
using Home.Graph.Common;
using Home.Graph.Server.Hubs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Home.Graph.Server.Controllers
{

    partial class TodosController
    {
        [Route("events/fromScheduleSource/{sourceId}"), HttpGet]
        public List<TodoItem> GetEventsFromScheduledSource(string sourceId)
        {
            sourceId = sourceId.ToLowerInvariant();

            var collection = MongoDbHelper.GetClient<TodoItem>();

            var lst = collection.Find(x => (x.SourceItemId != null && x.SourceItemId == sourceId) || x.Id == sourceId).ToList();
            return lst;
        }

        [Route("events/{eventId}"), HttpDelete]
        public bool DeleteEvent(string eventId)
        {
            eventId = eventId.ToLowerInvariant();
            var collection = MongoDbHelper.GetClient<TodoItem>();
            return collection.DeleteOne(x => x.Type != TodoItemType.TodoItem && x.Id == eventId).DeletedCount == 1;
        }

        [Route("events/{eventId}/start"), HttpGet]
        public bool StartEvent(string eventId, string limitUsers = null)
        {
            if (eventId == null)
                throw new ArgumentNullException(nameof(eventId));


            eventId = eventId.ToLowerInvariant();

            var meshColl = MongoDbHelper.GetClient<AutomationMesh>();
            var mesh = meshColl.Find(x => x.Id == "local").FirstOrDefault();

            var collection = MongoDbHelper.GetClient<TodoItem>();
            var item = collection.Find(x => x.Id == eventId).FirstOrDefault();

            if (item == null)
            {
                NotFound();
                return false;
            }

            if (item.Status == TodoItemStatus.Done)
                return true;

            string[] onlyUsers = null;
            if (!string.IsNullOrEmpty(limitUsers))
                onlyUsers = limitUsers.Split(new char[] { ',', ';', '|' }, StringSplitOptions.RemoveEmptyEntries);

            var count = collection.UpdateOne(x => x.Id == eventId, Builders<TodoItem>.Update.Set("Status", TodoItemStatus.InProgress)).ModifiedCount;
            if (!string.IsNullOrEmpty(item.ScenarioOnStart))
                ActivateScene(item.ScenarioOnStart);

            var usrCtl = new UsersController(_hubContext, _appContext);

            if (item.AssociatedUsers != null)
            {
                foreach (var user in item.AssociatedUsers)
                {
                    if (!user.ShouldUpdatePresence)
                        continue;
                    string userId = null;

                    if (onlyUsers != null)
                    {
                        if (!string.IsNullOrEmpty(user.UserId))
                        {
                            if (!onlyUsers.Contains(user.UserId))
                                continue;
                        }
                        else if (!string.IsNullOrEmpty(user.GuestEmail))
                        {
                            if (!onlyUsers.Contains(user.GuestEmail))
                                continue;
                        }
                        else if (!string.IsNullOrEmpty(user.GuestName))
                        {
                            if (!onlyUsers.Contains(user.GuestName))
                                continue;
                        }
                    }

                    if (string.IsNullOrEmpty(user.UserId))
                    {
                        if (!string.IsNullOrEmpty(user.GuestEmail)
                            || !string.IsNullOrEmpty(user.GuestName))
                        {
                            var tmp = usrCtl.UpsertGuestUser(new Home.Common.Model.User()
                            {
                                MainEmail = user.GuestEmail,
                                Name = user.GuestName,
                                CommonName = user.GuestName
                            });
                            if (tmp != null)
                                userId = tmp.Id;
                        }
                    }
                    else
                        userId = user.UserId;

                    if (!string.IsNullOrEmpty(userId))
                        usrCtl.ForceLocationFromEvent(userId, mesh.LocationId, "start");
                }
            }

            item.Status = TodoItemStatus.InProgress;

            AppAndDeviceHub.SendEventStartOrEnd(_appContext, item, false);

            return true;
        }


        private void ActivateScene(string sceneId)
        {
            string url = $"/v1.0/agents/all/send/homeautomation.scenario.execute";
            MessagingHelper.PushToLocalAgent(
                new ExecuteScenarioHomeAutomationMessage()
                {
                    SceneId = sceneId
                });
        }

        [Route("events/{eventId}/end"), HttpGet]
        public bool EndEvent(string eventId)
        {
            if (eventId == null)
                throw new ArgumentNullException(nameof(eventId));


            eventId = eventId.ToLowerInvariant();

            var meshColl = MongoDbHelper.GetClient<AutomationMesh>();
            var mesh = meshColl.Find(x => x.Id == "local").FirstOrDefault();

            var collection = MongoDbHelper.GetClient<TodoItem>();
            var item = collection.Find(x => x.Id == eventId).FirstOrDefault();

            if (item == null)
            {
                NotFound();
                return false;
            }

            if (item.Status == TodoItemStatus.Done)
                return true;

            var count = collection.UpdateOne(x => x.Id == eventId, Builders<TodoItem>.Update.Set("Status", TodoItemStatus.Done)).ModifiedCount;
            if (count == 1)
            {
                if (!string.IsNullOrEmpty(item.ScenarioOnEnd))
                    ActivateScene(item.ScenarioOnEnd);

                var usrCtl = new UsersController(_hubContext, _appContext);

                if (item.AssociatedUsers != null)
                {
                    foreach (var user in item.AssociatedUsers)
                    {
                        if (!user.ShouldUpdatePresence)
                            continue;
                        string userId = null;
                        if (string.IsNullOrEmpty(user.UserId))
                        {
                            if (!string.IsNullOrEmpty(user.GuestEmail)
                                || !string.IsNullOrEmpty(user.GuestName))
                            {
                                var tmp = usrCtl.UpsertGuestUser(new Home.Common.Model.User()
                                {
                                    MainEmail = user.GuestEmail,
                                    Name = user.GuestName
                                });
                                if (tmp != null)
                                    userId = tmp.Id;
                            }
                        }
                        else
                            userId = user.UserId;

                        if (!string.IsNullOrEmpty(userId))
                            usrCtl.ForceLocationFromEvent(userId, mesh.LocationId, "end");
                    }
                }

                item.Status = TodoItemStatus.Done;
                AppAndDeviceHub.SendEventStartOrEnd(_appContext, item, true);


                return true;
            }

            return false;
        }



        [Route("events"), HttpGet]
        public List<TodoItem> GetEvents(string userId = null, string listId = null, string syncServiceId = null, int nextDays = 7, bool includeDone = false, bool onlyScheduled = false)
        {
            if (userId == null)
                userId = UsersController.GetCurrentUserId(User);

            var collection = MongoDbHelper.GetClient<TodoItem>();

            var bldr = Builders<TodoItem>.Filter.Ne("Type", TodoItemType.TodoItem)
                & Builders<TodoItem>.Filter.Or(
                    Builders<TodoItem>.Filter.Eq<DateTimeOffset?>("DueDate", null),
                    Builders<TodoItem>.Filter.Lt<DateTimeOffset?>("DueDate", DateTimeOffset.Now.AddDays(nextDays)))
                ;

            if (!includeDone)
                bldr &= Builders<TodoItem>.Filter.Ne("Status", TodoItemStatus.Done);

            if (!string.IsNullOrEmpty(listId))
                bldr &= Builders<TodoItem>.Filter.Eq("ListId", listId);

            if (!string.IsNullOrEmpty(userId))
                bldr &= Builders<TodoItem>.Filter.Eq("UserId", userId);


            if (!string.IsNullOrEmpty(syncServiceId))
                bldr &= Builders<TodoItem>.Filter.Eq("SyncDatas.ExternalServiceId", syncServiceId);

            if (onlyScheduled)
                bldr &= Builders<TodoItem>.Filter.SizeGt("Schedule", 0);

            var lst = collection.Find(bldr).ToList();
            return lst;
        }

        [Route("events/mesh"), HttpGet]
        public List<TodoItem> GetEvents(string listId = null, string syncServiceId = null, int nextDays = 7, bool includeDone = false, bool onlyScheduled = false)
        {
            var collection = MongoDbHelper.GetClient<TodoItem>();

            var bldr = Builders<TodoItem>.Filter.Ne("Type", TodoItemType.TodoItem)
                & Builders<TodoItem>.Filter.Eq<string>("UserId", "#MESH#")
                & Builders<TodoItem>.Filter.Or(
                    Builders<TodoItem>.Filter.Eq<DateTimeOffset?>("DueDate", null),
                    Builders<TodoItem>.Filter.Lt<DateTimeOffset?>("DueDate", DateTimeOffset.Now.AddDays(nextDays)))
                ;

            if (!includeDone)
                bldr &= Builders<TodoItem>.Filter.Ne("Status", TodoItemStatus.Done);

            if (!string.IsNullOrEmpty(listId))
                bldr &= Builders<TodoItem>.Filter.Eq("ListId", listId);


            if (!string.IsNullOrEmpty(syncServiceId))
                bldr &= Builders<TodoItem>.Filter.Eq("SyncDatas.ExternalServiceId", syncServiceId);

            if (onlyScheduled)
                bldr &= Builders<TodoItem>.Filter.SizeGt("Schedule", 0);

            var lst = collection.Find(bldr).ToList();
            return lst;
        }

        [Route("events/clearInPast"), HttpGet]
        public bool ClearPastEvents()
        {
            var collection = MongoDbHelper.GetClient<TodoItem>();

            var bldr = Builders<TodoItem>.Filter.Ne("Type", TodoItemType.TodoItem)
                & Builders<TodoItem>.Filter.And(
                    Builders<TodoItem>.Filter.Ne<DateTimeOffset?>("DueDate", null),
                    Builders<TodoItem>.Filter.Lt<DateTimeOffset?>("DueDate", DateTimeOffset.Now.AddDays(-30)));
            collection.DeleteMany(bldr);

            return true;
        }

        [Route("events/temp"), HttpGet]
        public bool ClearTemp()
        {
            var collection = MongoDbHelper.GetClient<TodoItem>();

            var bldr = Builders<TodoItem>.Filter.Ne("Type", TodoItemType.TodoItem)
                & Builders<TodoItem>.Filter.And(
                    Builders<TodoItem>.Filter.Eq<string>("Label", "Ménage"),
                    Builders<TodoItem>.Filter.SizeLt("Schedule", 1));
            collection.DeleteMany(bldr);

            return true;
        }

    }
}
