using Home.Common.Model;
using Home.Graph.Common;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Home.Graph.Server.Hubs
{
    partial class AdminToolsHub
    {
        public class AgentStatusChange
        {
            public AgentStatusKind Kind { get; set; }
            public DateTimeOffset MessageDate { get; set; }
            public string Message { get; set; }
        }

        public static void SendAgentStatusUpdate(IHubContext<AdminToolsHub> context, string agentName, AgentStatusChange change)
        {
            context.Clients.All.SendAsync("agentStatusChanged", agentName, change);
        }
    }
}
