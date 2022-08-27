using Home.Common;
using Home.Common.Messages;
using Home.Common.Model;
using Home.Graph.Common;
using Home.Graph.Server.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;

namespace Home.Graph.Server.Controllers
{
    [Route("v1.0/system/mesh"), Authorize(Roles = "Agent,Admin,Device")]
    [ApiController]
    public partial class AutomationMeshController : ControllerBase
    {
        private readonly IHubContext<AdminToolsHub> _adminContext;
        private readonly IHubContext<AppAndDeviceHub> _appContext;

        public AutomationMeshController(IHubContext<AdminToolsHub> adminContext, IHubContext<AppAndDeviceHub> appContext)
        {
            _adminContext = adminContext;
            _appContext = appContext;
        }

        [Route("local/source-code-integration"), HttpPost(), Authorize(Roles = "Admin,Agent")]
        public AutomationMesh AssociateSourceCode([FromBody] AutomationMeshSouceCodeIntegration integ)
        {
            var collection = MongoDbHelper.GetClient<AutomationMesh>();

            collection.UpdateOne(x => x.Id == "local",
                Builders<AutomationMesh>.Update
                .Set("SourceCodeIntegration", integ));

            HomeServerHelper._cache = null;

            return Get("local");
        }

        [Route("local/associate-account/{accountGuid:guid}"), HttpGet(), Authorize(Roles = "Admin,Agent")]
        public AutomationMesh AssociateAccount(Guid accountGuid, string name = null, string prefix = null)
        {
            var collection = MongoDbHelper.GetClient<AutomationMesh>();

            var account = new AutomationMeshManoirAppAccount()
            {
                AccountGuid = accountGuid,
                Name = name,
                DomainPrefix = prefix
            };

            string url = $"https://home.{prefix}.manoir.app/";

            collection.UpdateOne(x => x.Id == "local", 
                Builders<AutomationMesh>.Update
                .Set("ManoirAppAccount", account)
                .Set("MainServer.MainRole.Uri", url));

            HomeServerHelper._cache = null;

            return Get("local");
        }

        [Route("local/interactions/greetings/general"), HttpGet, AllowAnonymous()]
        public GreetingsMessageResponse GetGeneralGreetings(string convertTo = null)
        {
            var msg = new GreetingsMessage(GreetingsMessage.SimpleGetGreetings)
            {
                Destination = GreetingsMessage.GreetingsDestination.Screen,
            };

            var response = MessagingHelper.RequestFromLocalAgent<GreetingsMessageResponse>(msg);
            response.ConvertTo(convertTo);
            return response;
        }

        

        [Route("{name}"), HttpGet(), Authorize(Roles = "Agent,Admin,Device")]
        public AutomationMesh Get(string name)
        {


            if (string.IsNullOrEmpty(name))
                return null;

            name = name.ToLowerInvariant();

            var collection = MongoDbHelper.GetClient<AutomationMesh>();
            var lst = collection.Find(x => x.Id == name).FirstOrDefault();
            if(lst!=null && lst.PublicId==null && name.Equals("local", StringComparison.InvariantCultureIgnoreCase))
            {
                lst.PublicId = Guid.NewGuid().ToString("D").ToLowerInvariant();
                var up = Builders<AutomationMesh>.Update.Set("PublicId", lst.PublicId);
                collection.UpdateOne(x => x.Id == lst.Id, up);
            }
            else if (lst == null && name.Equals("local", StringComparison.InvariantCultureIgnoreCase))
            {
                var rq = this.Request;
                lst = new AutomationMesh()
                {
                    Id = name.ToLowerInvariant(),
                    PublicId = Guid.NewGuid().ToString("D").ToLowerInvariant(),
                    MainServer = new AutomationServer()
                    {
                        Id = Environment.MachineName,
                        Name = Environment.MachineName,
                        MainRole = new AutomationServerRole()
                        {
                            Role = AutomationRole.GraphApi,
                            CommunicationMode = CommunicationMode.RestApi,
                            Uri = $"{rq.Scheme}://{rq.Host}"
                        }
                    }
                };
                collection.InsertOne(lst);
                lst = collection.Find(x => x.Id.Equals(name)).FirstOrDefault();
            }

            if(name.Equals("local"))
            {
                HomeServerHelper._cache = lst;
            }

            return lst;
        }

        [Route("local/agents"), HttpGet()]
        public IEnumerable<Agent> GetLocalAgents()
        {
            return new AgentsController(_adminContext).GetLocalAgents();
        }

        [Route("local/agents/register"), HttpPost(), AllowAnonymous()]
        public Agent GetLocalAgents([FromBody] AgentRegistration reg)
        {

            if (reg == null)
                return null;

            // "autorisation en mode light"
            var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
            var credentialBytes = Convert.FromBase64String(authHeader.Parameter);
            var credentials = Encoding.UTF8.GetString(credentialBytes).Split(new[] { ':' }, 2);
            var username = credentials[0];
            var password = credentials[1];

            var passwordToGet = Environment.GetEnvironmentVariable("HOMEAUTOMATION_APIKEY");
#if DEBUG
                passwordToGet = "12345678";
#endif
            if (!password.Equals(passwordToGet))
                return null;

            if (!username.Equals(reg.AgentName))
                return null;


            if (reg.AgentMachineName == null)
                reg.AgentMachineName = "local";
            else if (reg.AgentMachineName.Equals("docker"))
                reg.AgentMachineName = "local";
            else if (reg.AgentMachineName.Equals("kubernetes"))
                reg.AgentMachineName = "local";

            reg.AgentMachineName = reg.AgentMachineName.ToLowerInvariant();
            reg.AgentName = reg.AgentName.ToLowerInvariant();

            var collection = MongoDbHelper.GetClient<Agent>();
            var lst = collection.Find(x => x.AgentMesh == "local" && x.AgentMachineName == reg.AgentMachineName && x.AgentName == reg.AgentName).FirstOrDefault();

            if (lst == null)
            {
                var ag = new Agent()
                {
                    AgentMachineName = reg.AgentMachineName,
                    AgentMesh = "local",
                    AgentName = reg.AgentName,
                    Roles = reg.Roles,
                    CurrentStatus = new AgentStatus()
                    {
                        Kind = AgentStatusKind.Info,
                        Message = "Agent started",
                        MessageDate = DateTimeOffset.Now,
                        Format = AgentStatusFormat.String
                    },
                    LastPing = DateTimeOffset.Now,
                    Id = "local-" + reg.AgentMachineName + "-" + reg.AgentName
                };
                collection.InsertOne(ag);
            }

            AdminToolsHub.SendAgentStatusUpdate(_adminContext, reg.AgentName, new AdminToolsHub.AgentStatusChange()
            {
                Kind = AgentStatusKind.Info,
                Message = "Agent restarted",
                MessageDate = DateTimeOffset.Now
            });
            LogHelper.Log("agent", reg.AgentName, "Agent started");

            return lst;
        }
    }
}
