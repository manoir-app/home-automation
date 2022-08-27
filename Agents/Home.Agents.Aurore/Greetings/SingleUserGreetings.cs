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
    internal static class SingleUserGreetings
    {
        private static Dictionary<string, string> _latestGreetings = new Dictionary<string, string>();


        public static GreetingsMessageResponse GetGreetings(GreetingsMessage request, bool withNotify)
        {
            var ret = new GreetingsMessageResponse();
            ret.Response = "OK";

            var user = request.Users.First();


            StringBuilder blr = new StringBuilder();
            blr.Append("Bonjour **");
            blr.Append(user.CommonName);
            blr.Append("**,");

            ret.Items.Add(new GreetingsMessageResponseItem()
            {
                Content = blr.ToString(),
                ContentKind = GreetingsMessageResponseItem.GreetingsMessageResponseItemKind.HeaderContent
            });

           

            GreetingsHandler.AddMeshStatus(ret);
            GreetingsHandler.AddUserStatus(ret, user, false);

            string userName = user.Id;
            string oldGreets = null;
            if (!_latestGreetings.TryGetValue(userName, out oldGreets))
                oldGreets = "";

            if (oldGreets == null)
                oldGreets = "";

            var json = JsonConvert.SerializeObject(ret.Items);
            if (withNotify)
            {
                if (!json.Equals(oldGreets))
                {
                    Console.WriteLine("Greetings has changed : uploading data");
                    _latestGreetings[userName] = json;
                    GreetingsHandler.NotifyChangeInGreetings(userName, ret);
                }
                else
                {
                    Console.WriteLine("Greetings were identical to previous one");
                }
            }


            return ret;
        }

        

      
    }
}
