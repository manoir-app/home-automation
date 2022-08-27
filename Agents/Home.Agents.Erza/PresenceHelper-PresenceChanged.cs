using Home.Common;
using Home.Common.Messages;
using Home.Common.Model;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Threading;

namespace Home.Agents.Erza
{
    partial class PresenceHelper
    {

        private static List<string> _alreadyPresents = new List<string>();
        private static MessageResponse HandlePresenceChanged(PresenceChangedMessage msg)
        {
            MaintenanceUserPresent();

            return MessageResponse.OK;
        }

        private static void MaintenanceUserPresent(bool sendNotificationForChanges = true)
        {
            var usrs = GetLocalPresentUsers();
            if (usrs != null)
            {
                var guests = (from z in usrs
                              where !z.IsMain
                              select z).ToList();

                if (guests != null && guests.Count > 0)
                    AgentHelper.SetMeshPrivacyLevel("erza", AutomationMeshPrivacyMode.High);
                else
                    AgentHelper.SetMeshPrivacyLevel("erza", null);

                var mains = (from z in usrs
                             where z.IsMain
                             select z).ToList();


                var newIn = (from z in usrs
                             where !_alreadyPresents.Contains(z.Id)
                             select z.Id).ToList();


                var newOuts = (from z in _alreadyPresents
                               where !usrs.Any(x => x.Id.Equals(z))
                               select z).ToList();

                if (newIn.Count > 0)
                    Console.WriteLine("Users in : " + String.Join(',', newIn));
                if (newOuts.Count > 0)
                    Console.WriteLine("Users out : " + String.Join(',', newOuts));


                if (sendNotificationForChanges)
                    SendChangeNotification(newIn, newOuts);

                _alreadyPresents = (from z in usrs
                                    select z.Id).ToList();
            }
        }

        private static void SendChangeNotification(List<string> newIn, List<string> newOuts)
        {

        }

        private static List<User> GetLocalPresentUsers()
        {
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    using (var cli = new MainApiAgentWebClient("erza"))
                    {
                        return cli.DownloadData<List<User>>("v1.0/users/presence/mesh/local/all");
                    }
                }
                catch (Exception ex)
                {

                }
            }

            return new List<User>();
        }


    }
}
