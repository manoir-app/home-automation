using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Home.Common;
using Home.Common.Messages;
using Home.Common.Model;
using Home.Graph.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Home.Graph.Server.Controllers
{
    partial class AutomationMeshController 
    {
        [Route("local/internet-connections"), HttpGet()]
        public IEnumerable<InternetConnection> GetInternetConnection()
        {
            var collection = MongoDbHelper.GetClient<AutomationMesh>();
            var lst = collection.Find(x => x.Id == "local").FirstOrDefault();
            if (lst == null)
                return null;

            return lst.InternetConnections;
        }

        [Route("local/internet-connections"), HttpPost()]
        public InternetConnection UpdateInternetConnection([FromBody] InternetConnectionStatusRefresh status)
        {
            var collection = MongoDbHelper.GetClient<AutomationMesh>();
            var lst = collection.Find(x => x.Id == "local").FirstOrDefault();
            if (lst == null)
                return null;

            var ic = (from z in lst.InternetConnections
                      where z.Id.Equals(status.ConnectionId)
                      select z).FirstOrDefault();

            if(ic==null)
            {
                ic = new InternetConnection()
                {
                    ConnectionType = status.ConnectionType,
                    Id = status.ConnectionId,
                    IsMain = lst.InternetConnections.Count==0,
                    LastUpdate = DateTimeOffset.Now
                };
                lst.InternetConnections.Add(ic);

                var upd1 = Builders<AutomationMesh>.Update.Set("InternetConnections", lst.InternetConnections);
                collection.UpdateOne(w => w.Id == "local", upd1);
            }


            ic.DownloadBandwith = status.DownloadBandwith;
            ic.UsedDownloadBandwith = status.UsedDownloadBandwith;
            ic.UploadBandwith = status.UploadBandwith;
            ic.UsedUploadBandwith = status.UsedUploadBandwith;
            ic.LastMessage = status.Message;
            ic.LastUpdate = DateTimeOffset.Now;
            ic.Status = status.Status;


            var arrayFilters = Builders<AutomationMesh>.Filter.Eq("Id", "local")
                    & Builders<AutomationMesh>.Filter.Eq("InternetConnections.Id", status.ConnectionId);

            var arrayUpdate = Builders<AutomationMesh>.Update
                .Set("InternetConnections.$.DownloadBandwith", status.DownloadBandwith)
                .Set("InternetConnections.$.UsedDownloadBandwith", status.UsedDownloadBandwith)
                .Set("InternetConnections.$.UploadBandwith", status.UploadBandwith)
                .Set("InternetConnections.$.UsedUploadBandwith", status.UsedUploadBandwith)
                .Set("InternetConnections.$.LastMessage", status.Message)
                .Set("InternetConnections.$.LastUpdate", DateTimeOffset.Now)
                ;

            collection.UpdateOne(arrayFilters, arrayUpdate);

            arrayUpdate = Builders<AutomationMesh>.Update
                .Set("InternetConnections.$.Status", status.Status);

            var r = collection.UpdateOne(arrayFilters, arrayUpdate);
            if(r.ModifiedCount>0)
            {
                MessagingHelper.PushToLocalAgent(new NetworkStatusChangeMessage() { NetworkId= status.ConnectionId, NewStatus = status.Status });
            }

            if (status.Ssids != null && status.Ssids.Length > 0)
            {
                arrayUpdate = Builders<AutomationMesh>.Update
                    .Set("MainSsid", status.Ssids.First());
                collection.UpdateOne(x => x.Id == "local", arrayUpdate);
            }

            lst = collection.Find(x => x.Id == "local").FirstOrDefault();
            string intStats = AutomationMeshStatus.StatusOK;
            int countOK = (from z in lst.InternetConnections where z.Status == ConnectionStatus.Up select z).Count();
            int countNotOk = (from z in lst.InternetConnections where z.Status != ConnectionStatus.Up select z).Count();

            if(countNotOk == lst.InternetConnections.Count)
                intStats = AutomationMeshStatus.StatusKO;
            else if (countOK == lst.InternetConnections.Count)
                intStats = AutomationMeshStatus.StatusOK;
            else
                intStats = AutomationMeshStatus.StatusPartiallyOK;

            var upd2 = Builders<AutomationMesh>.Update.Set("Status.InternetConnectionStatusCode", intStats);
            r = collection.UpdateOne(w => w.Id == "local", upd2);

            if(r.ModifiedCount >0)
            {
                // notifier d'un changement d'état général
                MessagingHelper.PushToLocalAgent(new MeshStatusChangeMessage() { MeshId = "local", NewStatus = intStats, StatusKind = "InternetConnectionStatus" });
            }

            return ic;
        }
    }
}
