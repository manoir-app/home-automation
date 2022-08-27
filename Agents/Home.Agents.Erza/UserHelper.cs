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
    internal static partial class UserHelper
    {
        private static DateTime _lastRun = DateTime.Now;
        private static Location _localLoc = null;

        public static void Start()
        {
            var t = new Thread(() => UserHelper.Do());
            t.Name = "User maintenance";
            t.Start();
        }

        private static void Do()
        {
            while (!_stop)
            {
                _localLoc = AgentHelper.GetLocalMeshLocation("erza");

                Thread.Sleep(250);
                if (Math.Abs((DateTime.Now - _lastRun).TotalMinutes) > 5)
                {
                    _lastRun = DateTime.Now;
                    try
                    {
                        DoCleanUpGuests();
                    }
                    catch (Exception ex)
                    {
                        return;
                    }
                }
            }
        }

        private static void DoCleanUpGuests()
        {
            Dictionary<string, Exception> exceptions = new Dictionary<string, Exception>();

            var usrs = GetGuests();
            foreach (var c in usrs)
            {
                if (!c.IsGuest)
                    continue;

                if(c.DeleteAfter.HasValue && c.DeleteAfter < DateTimeOffset.Now)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        try
                        {
                            using (var cli = new MainApiAgentWebClient("erza"))
                            {
                                bool b = cli.UploadData<bool,string>("v1.0/users/all/" + c.Id, "DELETE", "");
                                break;
                            }
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                }
            }
        }


        public static List<User> GetGuests()
        {
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    using (var cli = new MainApiAgentWebClient("erza"))
                    {
                        return cli.DownloadData<List<User>>("v1.0/users/guests");
                    }
                }
                catch (Exception ex)
                {

                }
            }

            return new List<User>();
        }

        public static void Stop()
        {
            _stop = true;
        }


        public static bool _stop = false;

    }
}