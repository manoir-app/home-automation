using Home.Common.Model;
using Home.Graph.Common;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Home.Graph.Server.Hubs
{
    public partial class AdminToolsHub : Hub
    {
        static AdminToolsHub()
        {
            MessagingHelper.MessagePushed += MessagingHelper_MessagePushed;
        }

        private static void MessagingHelper_MessagePushed(object sender, MessagingHelper.MessagePushEventArgs e)
        {
                   
        }
    }
}
