using Home.Common;
using Home.Common.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Home.Agents.Erza
{
    class Program
    {
        public static bool _stop = false;

        static void Main(string[] args)
        {
            AgentHelper.WriteStartupMessage("Erza", typeof(Program).Assembly);

            DatabaseMaintenanceThread.CheckDb();

#if DEBUG
            NetworkChecker.RunPublicServerCheck();
#endif
            AgentHelper.SetupReporting("erza");
            AgentHelper.SetupLocaleFromServer("erza");
            AgentHelper.ReportStart("erza", "monitoring", "security");

            ErzaIntegrationsProvider.InitIntegrations();

            ErzaMessageHandler.Start();
            WeatherChecker.Start();
            NetworkChecker.Start();
            DatabaseMaintenanceThread.Start();
            SystemCleanup.Start();
            PresenceHelper.Start();
            UserHelper.Start();

            while (!_stop)
            {
                Thread.Sleep(500);
                AgentHelper.Ping("erza");
            }

            UserHelper.Stop();
            PresenceHelper.Stop();
            SystemCleanup.Stop();
            ErzaMessageHandler.Stop();
            NetworkChecker.Stop();
            WeatherChecker.Stop();
            DatabaseMaintenanceThread.Stop();

        }


        

       
    }
}
