using Home.Common;
using Home.Common.Messages;
using Home.Common.Model;
using Home.Graph.Common;
using Home.Graph.Server.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Driver;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace Home.Graph.Server.Controllers
{
    [Route("v1.0/agents")]
    [ApiController]
    public class AgentsController : ControllerBase
    {
        private readonly IHubContext<AdminToolsHub> _hubContext;

        public AgentsController(IHubContext<AdminToolsHub> hubContext)
        {
            _hubContext = hubContext;
        }


        [Route("all/send/{topic}"), HttpPost]
        public bool SendMessage(string topic)
        {
            using (var reader = new StreamReader(Request.Body))
            {

                var body = reader.ReadToEndAsync().Result;
                NatsMessageThread.Push(topic, body);
            }

            return true;
        }

        [Route("{agent}"), HttpGet]
        public Agent GetAgent(string agent)
        {
            if (agent == null)
                return null;

            agent = agent.ToLowerInvariant();

            var collection = MongoDbHelper.GetClient<Agent>();
            var lst = collection.Find(x => x.AgentMesh == "local" && x.AgentName == agent).FirstOrDefault();
            return lst;
        }

        [Route("{agentName}/restart"), HttpGet]
        public bool RestartAgent(string agentName)
        {
            if (agentName == null)
                return false;

            string service = "agents-" + agentName.ToLowerInvariant();
            SystemDeploymentMessage msg = new SystemDeploymentMessage()
            {
                Action = SystemDeploymentAction.Restart,
                DeploymentName = service
            };

            MessagingHelper.PushToLocalAgent(msg);

            return true;
        }

        [Route("{agent}/ping"), HttpGet]
        public bool Ping(string agent)
        {
            if (agent == null)
                return false;
            agent = agent.ToLowerInvariant();

            DateTimeOffset lastPing = DateTimeOffset.Now;

            var collection = MongoDbHelper.GetClient<Agent>();
            collection.UpdateOne(x => x.AgentMesh == "local" && x.AgentName == agent,
                Builders<Agent>.Update
                .Set("LastPing", lastPing));

            return true;
        }


        [Route("{agent}/status"), HttpPost]
        public Agent SetAgentStatus(string agent, [FromBody] string status)
        {
            if (agent == null)
                return null;
            agent = agent.ToLowerInvariant();

            DateTimeOffset lastPing = DateTimeOffset.Now;

            var dt =  DateTimeOffset.Now;
            var st = new AgentStatus() { MessageDate = dt, Message = status, Kind=AgentStatusKind.Info };
            var collection = MongoDbHelper.GetClient<Agent>();
            collection.UpdateOne(x => x.AgentMesh == "local" && x.AgentName == agent,
                Builders<Agent>.Update
                .Set("CurrentStatus", st)
                .Set("LastPing", lastPing));

            if (_hubContext != null)
                AdminToolsHub.SendAgentStatusUpdate(_hubContext, agent, new AdminToolsHub.AgentStatusChange()
                {
                    Kind = AgentStatusKind.Info,
                    Message = status,
                    MessageDate = dt
                });

            MessagingHelper.PushToLocalAgent(new AgentStatusUpdateMessage(agent)
            {
                NewStatus = st
            });

            MqttHelper.PublishAgentStatus(agent, status, lastPing);

            var lst = collection.Find(x => x.AgentMesh == "local" && x.AgentName == agent).FirstOrDefault();
            return lst;
        }

        [Route("{agent}/configuration"), HttpPost]
        public Agent SetAgentConfiguration(string agent, [FromBody] string configuration)
        {
            if (agent == null)
                return null;

            agent = agent.ToLowerInvariant();

            var collection = MongoDbHelper.GetClient<Agent>();
            collection.UpdateOne(x => x.AgentMesh == "local" && x.AgentName == agent,
                Builders<Agent>.Update.Set("ConfigurationData", configuration));

            var lst = collection.Find(x => x.AgentMesh == "local" && x.AgentName == agent).FirstOrDefault();
            return lst;
        }

        [Route("all"), HttpGet()]
        public IEnumerable<Agent> GetLocalAgents()
        {
            var collection = MongoDbHelper.GetClient<Agent>();
            var lst = collection.Find(x => x.AgentMesh == "local").ToList();
            return lst;
        }

        [Route("all/clearobsolete"), HttpGet()]
        public bool ClearObsoletesAgents()
        {
            var collection = MongoDbHelper.GetClient<Agent>();
            var lst = collection.Find(x => true).ToList();

            foreach(var l in lst)
            {
                if(l.LastPing< DateTimeOffset.Now.AddDays(-2))
                {
                    collection.DeleteOne(x => x.Id == l.Id);
                }
            }

            return true;
        }

    }
}
