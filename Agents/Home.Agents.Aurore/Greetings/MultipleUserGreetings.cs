using Home.Common.Messages;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using System.Threading;
using Home.Common;

namespace Home.Agents.Aurore.Greetings
{
    internal static class MultipleUserGreetings
    {
        private static Dictionary<string, string> _latestGreetings = new Dictionary<string, string>();


        public static GreetingsMessageResponse GetGreetings(GreetingsMessage request)
        {
            var ret = new GreetingsMessageResponse();
            ret.Response = "OK";

            var users = (from z in request.Users
                         where z.IsMain
                         select z).ToList(); ;


            StringBuilder blr = new StringBuilder();
            blr.Append("Bonjour **");
            for (int i = 0; i < users.Count; i++)
            {
                if (i > 0) blr.Append(" & ");
                blr.Append(users[i].CommonName);
            }
            blr.Append("**,");

            ret.Items.Add(new GreetingsMessageResponseItem()
            {
                Content = blr.ToString(),
                ContentKind = GreetingsMessageResponseItem.GreetingsMessageResponseItemKind.HeaderContent
            });

            GreetingsHandler.AddMeshStatus(ret);
            switch (request.Destination)
            {
                case GreetingsMessage.GreetingsDestination.UserApp:
                case GreetingsMessage.GreetingsDestination.Speakers:
                    for (int i = 0; i < users.Count; i++)
                    {
                        GreetingsHandler.AddUserStatus(ret, users[i], true);
                    }
                    break;
            }
            string userName = string.Join(',', (from z in users select z.Id).ToArray());
            string oldGreets = null;
            if (!_latestGreetings.TryGetValue(userName, out oldGreets))
                oldGreets = "";

            if (oldGreets == null)
                oldGreets = "";

            var json = JsonConvert.SerializeObject(ret.Items);
            return ret;
        }




    }
}
