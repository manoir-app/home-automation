using Home.Common.Messages;
using Home.Common.Model;
using Home.Graph.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Home.Graph.Server.Controllers
{
    partial class AutomationMeshController
    {
        [Route("local/triggers"), HttpGet()]
        public List<Trigger> GetTriggers()
        {
            var coll = MongoDbHelper.GetClient<Trigger>();
            return coll.Find(x => true).ToList();
        }

        [Route("local/triggers"), HttpPost()]
        public Trigger UpsertTrigger([FromBody] Trigger t)
        {
            var coll = MongoDbHelper.GetClient<Trigger>();

            if (t.Id == null)
                t.Id = Guid.NewGuid().ToString("N").ToLowerInvariant();

            var tmp = coll.Find(x => x.Id == t.Id).FirstOrDefault();

            if (tmp == null)
            {
                coll.InsertOne(t);
            }
            else
            {
                coll.ReplaceOne(x => x.Id == t.Id, t);
            }

            MessagingHelper.PushToLocalAgent(new AgentGenericMessage("system.triggers.change")
            {
                MessageContent = "Tiggers changed"
            });

            return GetTrigger(t.Id);
        }

        [Route("local/triggers/{triggerId}"), HttpGet()]
        public Trigger GetTrigger(string triggerId)
        {
            var coll = MongoDbHelper.GetClient<Trigger>();
            return coll.Find(x => x.Id == triggerId).FirstOrDefault();
        }

        [Route("local/triggers/{triggerId}/raise"), HttpGet, HttpPost, AllowAnonymous]
        public async Task<bool> RaiseEvent(string triggerId, string data = null)
        {
            var coll = MongoDbHelper.GetClient<Trigger>();
            string source = User.Identity.Name;

            var tmp = coll.Find(x => x.Id == triggerId).FirstOrDefault();

            if (tmp != null)
            {
                Console.WriteLine($"Trigger raised : {triggerId}");

                DateTimeOffset runDate = DateTimeOffset.Now;
                coll.UpdateOne(x => x.Id == triggerId,
                    Builders<Trigger>.Update
                    .Set("LatestOccurence", runDate));
                MqttHelper.PublishTriggerActivated(triggerId, runDate, tmp.ToString());

                var content = "";
                if (this.Request.ContentLength.HasValue && this.Request.ContentLength.Value > 0)
                {
                    using (var st = new StreamReader(this.Request.Body))
                        content = await st.ReadToEndAsync();
                }

                if (tmp.RaisedMessages != null)
                {
                    foreach (var t in tmp.RaisedMessages)
                    {
                        // on raise un event correspondant au webhook
                        if (!string.IsNullOrEmpty(t.MessageTopic))
                        {
                            MessagingHelper.PushToLocalAgent(t.MessageTopic,
                                tmp.ReplaceContent(t.MessageContent, source, content));
                        }
                    }
                }

                if (tmp.ChangedProperties != null)
                {
                    foreach (var t in tmp.ChangedProperties)
                    {
                        if (!string.IsNullOrEmpty(t.PropertyName))
                        {
                            switch (t.Kind)
                            {
                                case TriggerPropertyChangeKind.RoomProperty:
                                    SetRoomProperty(t, data);
                                    break;
                            }
                        }
                    }
                }

                return true;
            }

            return false;
        }

        private void SetRoomProperty(TriggerPropertyChange t, string data)
        {
            var meshColl = MongoDbHelper.GetClient<AutomationMesh>();
            var local = meshColl.Find(x => x.Id.Equals("local")).FirstOrDefault();
            if (local == null)
                return;

            var coll = MongoDbHelper.GetClient<Location>();
            var locs = coll.Find(x => x.Id.Equals(local.LocationId)).FirstOrDefault();
            var rooms = new List<LocationRoom>();
            foreach (var tmp in (from z in locs.Zones select z.Rooms).ToList())
                rooms.AddRange(tmp);

            var room = (from z in rooms where z.Id.Equals(t.RoomId, StringComparison.InvariantCultureIgnoreCase) select z).FirstOrDefault();

            if (room == null)
            {
                Console.WriteLine($"Room {t.RoomId} not found");
                return;
            }
            decimal decVal = decimal.MinValue;
            switch (t.PropertyName.ToLowerInvariant())
            {
                case "temperature":
                case "temp":
                    if (decimal.TryParse(data, out decVal))
                    {
                        Console.WriteLine($"Room {t.RoomId} : setting temperature to {decVal}");
                        room.Properties.Temperature = decVal;
                        TimeDBHelper.Trace("temperature",
                            decVal, new Dictionary<string, string>() {
                                {"locationId", locs.Id},
                                {"roomId", room.Id},
                                {"roomLevel", room.FloorLevel.ToString()}
                            });
                    }

                    break;
                case "humidity":
                    if (decimal.TryParse(data, out decVal))
                    {
                        Console.WriteLine($"Room {t.RoomId} : setting humidity to {decVal}");
                        room.Properties.Humidity = decVal;
                        TimeDBHelper.Trace("humidity",
                            decVal, new Dictionary<string, string>() {
                                {"locationId", locs.Id},
                                {"roomId", room.Id},
                                {"roomLevel", room.FloorLevel.ToString()}
                            });
                    }
                    break;
                default:
                    Console.WriteLine($"Room {t.RoomId} : setting {t.PropertyName} to {data}");
                    if (room.Properties.MoreProperties == null)
                        room.Properties.MoreProperties = new Dictionary<string, string>();
                    room.Properties.MoreProperties[t.PropertyName] = data;
                    break;
            }


            MqttHelper.PublishRoom(room);

            try
            {

                var parentZone = (from z in locs.Zones where z.Rooms.Contains(room) select z).FirstOrDefault();
                if (parentZone != null)
                {
                    var arrayFilters = Builders<Location>.Filter.Eq("Id", locs.Id)
                            & Builders<Location>.Filter.Eq("Zones.Id", parentZone.Id);

                    var arrayUpdate = Builders<Location>.Update
                        .Set("Zones.$.Rooms", parentZone.Rooms);

                    coll.UpdateOne(arrayFilters, arrayUpdate);
                }
                else
                    Console.WriteLine($"Parent zone of {t.RoomId} not found for update");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        [Route("local/triggers/{triggerId}"), HttpDelete()]
        public bool DeleteTrigger(string triggerId)
        {
            var coll = MongoDbHelper.GetClient<Trigger>();
            var obj = coll.Find(x => x.Id == triggerId).FirstOrDefault();
            if (obj == null)
                return false;

            var res = coll.DeleteOne(x => x.Id == triggerId);
            MessagingHelper.PushToLocalAgent(new AgentGenericMessage("system.triggers.change")
            {
                MessageContent = "Tiggers changed"
            });

            if (res.IsAcknowledged)
                return true;

            return false;
        }

    }
}
