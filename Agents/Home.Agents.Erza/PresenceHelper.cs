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
using Home.Graph.Common;

namespace Home.Agents.Erza
{
    internal static partial class PresenceHelper
    {
        private static DateTime _lastRun = DateTime.Now;
        private static DateTime _lastRefreshLoc = DateTime.Now;
        private static Location _localLoc = null;

        public static void Start()
        {
            var t = new Thread(() => PresenceHelper.Do());
            t.Name = "Presence maintenance";
            t.Start();
        }

        private static void Do()
        {
            while (!_stop)
            {

                if (Math.Abs((DateTime.Now - _lastRefreshLoc).TotalMinutes) > 1 || _localLoc == null)
                {
                    _lastRefreshLoc = DateTime.Now;
                    _localLoc = AgentHelper.GetLocalMeshLocation("erza");
                }

                Thread.Sleep(250);
                if (Math.Abs((DateTime.Now - _lastRun).TotalMinutes) > 2)
                {

                    _lastRun = DateTime.Now;
                    try
                    {
                        DoCleanUpPresence();
                    }
                    catch (Exception ex)
                    {
                        return;
                    }
                    try
                    {
                        MaintenanceUserPresent(false);
                    }
                    catch (Exception ex)
                    {
                        return;
                    }
                }
            }
        }

        private static void DoCleanUpPresence()
        {
            Dictionary<string, Exception> exceptions = new Dictionary<string, Exception>();

            var usrs = AgentHelper.GetLocalPresentUsers("erza");
            foreach (var c in usrs)
            {
                DecoterLesPresences(c);
            }
        }

        public static void Stop()
        {
            _stop = true;
        }


        public static bool _stop = false;



        internal static MessageResponse HandleMessage(MessageOrigin origin, string topic, string messageBody)
        {
            if (topic.Equals("users.presence.activity", StringComparison.InvariantCultureIgnoreCase))
            {
                return HandleActivity(JsonConvert.DeserializeObject<PresenceNotificationMessage>(messageBody));
            }
            else if (topic.Equals("users.presence.changed", StringComparison.InvariantCultureIgnoreCase))
            {
                return HandlePresenceChanged(JsonConvert.DeserializeObject<PresenceChangedMessage>(messageBody));
            }

            return MessageResponse.OK;
        }

        private static void DecoterLesPresences(User usr)
        {
            foreach (var pr in usr.Presence.Location)
            {
                if (pr.LatestUpdate < DateTimeOffset.Now.AddHours(-12))
                {
                    pr.Probability = 0;
                    pr.LatestUpdate = DateTime.UtcNow;
                    UploadPresenceData(usr, new PresenceUpdateData()
                    {
                        Location = pr
                    });
                }
                else if (pr.LatestUpdate < DateTimeOffset.Now.AddMinutes(-15))
                {
                    int newProba = Math.Max(5, pr.Probability - 5);
                    if (newProba == pr.Probability)
                        continue;
                    pr.Probability = newProba;
                    pr.LatestUpdate = DateTime.UtcNow;
                    UploadPresenceData(usr, new PresenceUpdateData()
                    {
                        Location = pr
                    });
                }
            }
        }

    }
}