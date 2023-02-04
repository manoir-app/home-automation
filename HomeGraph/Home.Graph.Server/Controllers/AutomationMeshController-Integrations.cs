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
using Microsoft.Azure.Amqp.Framing;

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
            var ret = coll.Find(x => x.AgentId != null && x.AgentId == agentId).ToList();

            if (onlyActives)
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

        [Route("local/integrations/{integrationId}/config"), HttpGet()]
        public ActionResult ConfigureIntegration(string integrationId, bool forceNew = false)
        {
            if (integrationId == null)
                return BadRequest();
            integrationId = integrationId.ToLowerInvariant();
            var coll = MongoDbHelper.GetClient<Integration>();
            var integ = coll.Find(x => x.Id == integrationId).FirstOrDefault();

            if (integ == null)
                return NotFound();

            IntegrationConfigurationData ret = new IntegrationConfigurationData()
            {
                Integration = integ
            };

            if (forceNew)
            {
                if (integ.Instances != null && integ.Instances.Count > 0
                    && !integ.CanInstallMultipleTimes)
                {
                    return BadRequest("Already configured");
                }
            }
            else
            {
                if (integ.Instances != null && integ.Instances.Count > 0)
                {
                    return BadRequest("Already configured");
                }

            }

            ret.CurrentInstance = new IntegrationInstance()
            {
                Id = Guid.NewGuid().ToString("N"),
                IsSetup = false,
                Label = integ.Label == null ? "New" : integ.Label,
                Settings = new Dictionary<string, string>()
            };
            var msg = new IntegrationConfigurationMessage(integ.AgentId, integ, ret.CurrentInstance);
            return SendConfigurationStepToAgent(coll, integ, ret, msg);
        }


        [Route("local/integrations/{integrationId}/config/{instanceId}"), HttpPost()]
        public ActionResult ConfigureIntegration(string integrationId, string instanceId, [FromBody] Dictionary<string, string> configValues)
        {
            if (integrationId == null)
                return BadRequest();
            if (instanceId == null)
                return BadRequest();

            integrationId = integrationId.ToLowerInvariant();
            var coll = MongoDbHelper.GetClient<Integration>();
            var integ = coll.Find(x => x.Id == integrationId).FirstOrDefault();

            if (integ == null)
                return NotFound();

            IntegrationConfigurationData ret = new IntegrationConfigurationData()
            {
                Integration = integ
            };

             
            ret.CurrentInstance = (from z in integ.Instances
                                   where z.Id.Equals(instanceId, StringComparison.InvariantCultureIgnoreCase)
                                   select z).FirstOrDefault();
            if (ret.CurrentInstance == null)
                return NotFound();

            var msg = new IntegrationConfigurationMessage(integ.AgentId, integ, ret.CurrentInstance);
            if (configValues != null)
                msg.SetupValues = configValues;
            return SendConfigurationStepToAgent(coll, integ, ret, msg);
        }

        private static ActionResult SendConfigurationStepToAgent(IMongoCollection<Integration> coll, Integration integ, IntegrationConfigurationData ret, IntegrationConfigurationMessage msg)
        {
            var dm = MessagingHelper.RequestFromLocalAgent<IntegrationConfigurationResponse>(msg);
            if (dm != null)
            {
                ret.ConfigurationCardFormat = dm.ConfigurationCardFormat;
                ret.ConfigurationCard = dm.ConfigurationCard;
                ret.IsFinalStep = dm.IsFinalStep;
            }

            foreach (var r in integ.Instances)
            {
                if (r.Id.Equals(dm.Instance.Id))
                {
                    integ.Instances.Remove(r);
                    integ.Instances.Add(dm.Instance);
                    break;
                }
            }

            coll.ReplaceOne(x => x.Id.Equals(integ.Id),
                integ);

            if (dm.Instance.IsSetup)
            {
                MessagingHelper.PushToLocalAgent(new IntegrationInstancesListChangedMessage()
                {
                    
                });
            }

            return new OkObjectResult(ret);
        }

        public class IntegrationConfigurationData
        {
            public Integration Integration { get; set; }
            public IntegrationInstance CurrentInstance { get; set; }

            public string ConfigurationCardFormat { get; set; }
            public string ConfigurationCard { get; set; }
            public bool IsFinalStep { get; set; }

        }

    }
}
