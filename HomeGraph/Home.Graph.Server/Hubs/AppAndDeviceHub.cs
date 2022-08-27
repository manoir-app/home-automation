using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Home.Common;
using Home.Graph.Common;
using Microsoft.AspNetCore.SignalR;

namespace Home.Graph.Server.Hubs
{
    public partial class AppAndDeviceHub : Hub<IAppAndDeviceHubClient>
    {
        static AppAndDeviceHub()
        {
            MessagingHelper.MessagePushed += MessagingHelper_MessagePushed;
        }

        private static void MessagingHelper_MessagePushed(object sender, MessagingHelper.MessagePushEventArgs e)
        {

        }
    }
}
