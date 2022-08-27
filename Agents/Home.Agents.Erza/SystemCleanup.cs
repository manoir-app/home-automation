using Home.Common;
using Home.Common.Model;
using Home.Graph.Common;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Home.Agents.Erza
{
    public static class SystemCleanup
    {
        private static DateTime _lastRun = DateTime.Now;

        public static void Start()
        {
            var t = new Thread(() => SystemCleanup.Do());
            t.Name = "System cleaner";
            t.Start();
        }

        private static void Do()
        {
            while (!_stop)
            {
                Thread.Sleep(250);
                if (Math.Abs((DateTime.Now - _lastRun).TotalMinutes) > 30)
                {
                    _lastRun = DateTime.Now;
                    string currentTask = "starting";
                    try
                    {
                        currentTask = "Clean logs";
                        DoCleanLogs();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                        AgentHelper.UpdateStatusWithInfo("erza", "Cleanup failed at task : " + currentTask + " : " + ex.Message);
                        return;
                    }
                    try
                    {
                        currentTask = "CleanNotifications";
                        DoCleanUpNotifications();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                        AgentHelper.UpdateStatusWithInfo("erza", "Cleanup failed at task : " + currentTask + " : " + ex.Message);
                        return;
                    }
                    AgentHelper.UpdateStatusWithInfo("erza", "Cleanup done");
                }
            }
        }

        private static void DoCleanLogs()
        {
            // le nettoyage des logs se fait en direct sans appel
            // serveur
            DateTimeOffset before = DateTimeOffset.Now.AddMonths(-1);
            var coll = MongoDbHelper.GetClient<LogData>();
            coll.DeleteMany(x => x.Date < before);
        }

        private static void DoCleanUpNotifications()
        {
            Dictionary<string, Exception> exceptions = new Dictionary<string, Exception>();

            using (var cli = new MainApiAgentWebClient("erza"))
            {
                var usrs = cli.DownloadData<List<User>>("/v1.0/users/main");
                foreach (var c in usrs)
                {
                    try
                    {
                        bool b = cli.DownloadData<bool>($"/v1.0/users/{c.Id}/notifications/clearreaditems");
                        if (!b)
                            throw new ApplicationException("L'appel au point de nettoyage des notifications a retourné false");
                    }
                    catch (Exception ex)
                    {
                        if (!exceptions.ContainsKey(c.Id))
                            exceptions.Add(c.Id, ex);
                    }
                }
            }

            if(exceptions.Count>0)
            {
                StringBuilder blr = new StringBuilder();
                foreach(var uid in exceptions.Keys)
                {
                    var exc = exceptions[uid];
                    if (blr.Length > 0)
                        blr.Append(" ; ");
                    else
                        blr.Append("Impossible de supprimer les notifications ");
                    blr.Append("pour ");
                    blr.Append(uid);
                    blr.Append("(");
                    blr.Append(exc.Message);
                    blr.Append(")");
                    throw new ApplicationException(blr.ToString());
                }
            }

        }

        public static void Stop()
        {
            _stop = true;
        }


        public static bool _stop = false;
    }
}
