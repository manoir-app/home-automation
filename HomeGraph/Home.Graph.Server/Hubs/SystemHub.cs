using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Home.Common;
using Home.Graph.Common;
using Microsoft.AspNetCore.SignalR;

namespace Home.Graph.Server.Hubs
{
    public class SystemHub : Hub
    {
        public class DeviceAppInfo
        {
            public string Url { get; set; }
        }
        public static void ChangeDeviceApp(IHubContext<SystemHub> context, string deviceId, DeviceAppInfo app)
        {
            context.Clients.All.SendAsync("changeAppOnDevice", deviceId, app);
        }

        public static void ForceRefreshDeviceApp(IHubContext<SystemHub> context, string deviceId)
        {
            context.Clients.All.SendAsync("forceRefresh", deviceId);
        }
    }
}
