using Home.Common.Model;
using Home.Graph.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using Home.Common.Messages;
using Home.Common;

namespace Home.Graph.Server.Controllers
{
    partial class AutomationMeshController
    {
        [Route("local/extensions"), HttpGet()]
        public List<MeshExtension> GetExtensions()
        {
            var coll = MongoDbHelper.GetClient<MeshExtension>();
            return coll.Find(x => true).ToList();
        }

        [Route("local/extensions"), HttpPost()]
        public MeshExtension UpsertExtension([FromBody] MeshExtension t)
        {
            var coll = MongoDbHelper.GetClient<MeshExtension>();

            var ext = coll.Find(x => x.Id == t.Id).FirstOrDefault();

            if (ext == null)
                coll.InsertOne(t);
            else
            {
                if(DoDelete(t.Id, coll))
                    coll.InsertOne(t);
                else
                {
                    throw new ApplicationException("Impossible de modifier cette extension");
                }
            }

            MessagingHelper.PushToLocalAgent(new AgentGenericMessage("system.extensions.change")
            {
                MessageContent = "Extension changed"
            });

            return GetExtension(t.Id);
        }

        [Route("local/extensions/{extensionId}"), HttpGet()]
        public MeshExtension GetExtension(string extensionId)
        {
            if (extensionId == null)
                return null;
            extensionId = extensionId.ToLowerInvariant();
            var coll = MongoDbHelper.GetClient<MeshExtension>();
            return coll.Find(x => x.Id == extensionId).FirstOrDefault();
        }

        [Route("local/extensions/{extensionId}/restart"), HttpGet()]
        public bool RestartExtension(string extensionId)
        {
            if (extensionId == null)
                return false;
            extensionId = extensionId.ToLowerInvariant();
            var coll = MongoDbHelper.GetClient<MeshExtension>();
            var data = coll.Find(x => x.Id == extensionId).FirstOrDefault();

            if (data == null)
                return false;

            var msg = MessagingHelper.RequestFromLocalAgent<MessageResponse>(
                new MeshExtensionOperationMessage(MeshExtensionOperationMessage.TopicRestart)
                { ExtensionId = extensionId });

            if(!msg.IsFail())
                return true;

            return false;
        }

        [Route("local/extensions/{extensionId}/install"), HttpGet()]
        public bool InstallExtension(string extensionId)
        {
            if (extensionId == null)
                return false;
            extensionId = extensionId.ToLowerInvariant();
            var coll = MongoDbHelper.GetClient<MeshExtension>();
            var data = coll.Find(x => x.Id == extensionId).FirstOrDefault();

            if (data == null)
                return false;

            var msg = MessagingHelper.RequestFromLocalAgent<MessageResponse>(
                new MeshExtensionOperationMessage(MeshExtensionOperationMessage.TopicCreate)
                { ExtensionId = extensionId });

            if (!msg.IsFail())
                return true;

            return false;
        }

        [Route("local/extensions/{extensionId}/uninstall"), HttpGet()]
        public bool UninstallExtension(string extensionId)
        {
            if (extensionId == null)
                return false;
            extensionId = extensionId.ToLowerInvariant();
            var coll = MongoDbHelper.GetClient<MeshExtension>();
            var data = coll.Find(x => x.Id == extensionId).FirstOrDefault();

            if (data == null)
                return false;

            var msg = MessagingHelper.RequestFromLocalAgent<MessageResponse>(
                new MeshExtensionOperationMessage(MeshExtensionOperationMessage.TopicTerminate)
                { ExtensionId = extensionId });

            if (!msg.IsFail())
                return true;

            return false;
        }


        [Route("local/extensions/{extensionId}/setinstalled"), HttpGet()]
        public bool UpdateInstallationStatus(string extensionId, bool newStatus = true)
        {
            if (extensionId == null)
                return false;
            extensionId = extensionId.ToLowerInvariant();

            var coll = MongoDbHelper.GetClient<MeshExtension>();

            var up = Builders<MeshExtension>.Update.Set("IsInstalled", newStatus);
            var res = coll.UpdateOne(x => x.Id == extensionId, up);

            return res.IsAcknowledged && res.ModifiedCount > 0;
        }


        [Route("local/extensions/{extensionId}"), HttpDelete()]
        public bool DeleteExtension(string extensionId)
        {
            if (extensionId == null)
                return false;
            extensionId = extensionId.ToLowerInvariant();

            var coll = MongoDbHelper.GetClient<MeshExtension>();
            var obj = coll.Find(x => x.Id == extensionId).FirstOrDefault();
            if (obj == null)
                return false;

            return DoDelete(extensionId, coll);
        }

        private static bool DoDelete(string extensionId, IMongoCollection<MeshExtension> coll)
        {
            // TODO : faire un appel pour vérifier que l'on peut la supprimer

            var msg = MessagingHelper.RequestFromLocalAgent<MessageResponse>(
                new MeshExtensionOperationMessage(MeshExtensionOperationMessage.TopicTerminate)
                { ExtensionId = extensionId });
            
            if(msg.IsFail())
                return false;

            var res = coll.DeleteOne(x => x.Id == extensionId);
            if (res.IsAcknowledged)
                return true;

            MessagingHelper.PushToLocalAgent(new AgentGenericMessage("system.extensions.change")
            {
                MessageContent = "Extensions changed"
            });

            return true;
        }
    }
}
