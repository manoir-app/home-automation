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
        [Route("local/integrations"), HttpGet()]
        public List<Integration> GetIntegrations()
        {
            var coll = MongoDbHelper.GetClient<Integration>();
            return coll.Find(x => true).ToList();
        }

        [Route("local/integrations/byagent/{agentId}"), HttpGet()]
        public List<Integration> GetIntegrations(string agentId, bool onlyActives = false)
        {
            if (string.IsNullOrEmpty(agentId))
                return null;
            agentId = agentId.ToLowerInvariant();
            var coll = MongoDbHelper.GetClient<Integration>();
            var ret = coll.Find(x => x.AgentId!=null && x.AgentId == agentId).ToList();

            if(onlyActives)
            {
                ret = (from z in ret
                       where z.Instances != null && z.Instances.Count > 0
                       select z).ToList();
            }

            return ret;
        }


        [Route("local/integrations"), HttpPost()]
        public Integration UpsertIntegration([FromBody] Integration t)
        {
            var coll = MongoDbHelper.GetClient<Integration>();

            var ext = coll.Find(x => x.Id == t.Id).FirstOrDefault();

            if (ext == null)
                coll.InsertOne(t);
            else
            {
                t.Instances = ext.Instances;
                coll.ReplaceOne(w => w.Id == t.Id, t);
            }

            MessagingHelper.PushToLocalAgent(new IntegrationListChangedMessage()
            {

            });

            return GetIntegration(t.Id);
        }

        [Route("local/integrations/raw"), HttpPost()]
        public Integration RawUpsertIntegration([FromBody] Integration t)
        {
            var coll = MongoDbHelper.GetClient<Integration>();

            var ext = coll.Find(x => x.Id == t.Id).FirstOrDefault();

            if (ext == null)
                coll.InsertOne(t);
            else
            {
                coll.ReplaceOne(w => w.Id == t.Id, t);
            }

            MessagingHelper.PushToLocalAgent(new IntegrationListChangedMessage()
            {

            });

            MessagingHelper.PushToLocalAgent(new IntegrationInstancesListChangedMessage()
            {

            });

            return GetIntegration(t.Id);
        }


        [Route("local/integrations/{integrationId}"), HttpGet()]
        public Integration GetIntegration(string integrationId)
        {
            if (integrationId == null)
                return null;
            integrationId = integrationId.ToLowerInvariant();
            var coll = MongoDbHelper.GetClient<Integration>();
            return coll.Find(x => x.Id == integrationId).FirstOrDefault();
        }


    }
}
