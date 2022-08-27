using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Home.Common;
using Home.Common.Model;
using Home.Graph.Common;
using Microsoft.AspNetCore.SignalR;

namespace Home.Graph.Server.Hubs
{
    public partial class AppAndDeviceHub : Hub<IAppAndDeviceHubClient>
    {
        public static void SendMeshChange(IHubContext<AppAndDeviceHub> context, string changeType, AutomationMesh mesh)
        {
            context.Clients.All.SendAsync("notifyMeshChange", changeType, mesh);
        }

    }
}
