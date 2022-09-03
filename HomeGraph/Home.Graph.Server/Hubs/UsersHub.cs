using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Home.Common;
using Home.Common.Messages;
using Home.Common.Model;
using Home.Graph.Common;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Azure.NotificationHubs;

namespace Home.Graph.Server.Hubs
{
    public partial class UsersHub : Hub<IUsersHub>
    {
        
        static UsersHub()
        {
            MessagingHelper.MessagePushed += MessagingHelper_MessagePushed;
        }

        private static void MessagingHelper_MessagePushed(object sender, MessagingHelper.MessagePushEventArgs e)
        {

        }

        public static void SendNewNotification(IHubContext<UsersHub> context, string userId, UserNotification notification)
        {
            try
            {
                var lst = FindConnections(userId);
                if(lst!=null && lst.Length>0)
                    context.Clients.Clients(lst).SendAsync("NewNotification", userId, notification);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private static string[] FindConnections(string userId)
        {
            var lst = _connections[userId];
            if (lst == null)
                return new string[0];
            return lst.ToArray();
        }

        static private Dictionary<string, List<string>> _connections = new Dictionary<string, List<string>>();

        public void ClearUserConnectionId(string userId, string connectionId)
        {
            var lst = _connections[userId];
            if (lst == null)
                return;
            lst.Remove(connectionId);
        }

        public void RegisterUserConnectionId(string userId, string connectionId)
        {
            var lst = _connections[userId];
            if (lst == null)
            {
                lst = new List<string>();
                _connections[userId] = lst;
            }
            lst.Add(connectionId);
        }


    }
}
