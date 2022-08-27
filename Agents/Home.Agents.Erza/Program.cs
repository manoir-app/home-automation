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
            Console.WriteLine("Starting Erza");
            string cn = Environment.GetEnvironmentVariable("APPCONFIG_CNSTRING");


            if (cn != null)
            {
                Console.WriteLine("with config = " + cn);
                ConfigurationSettingsHelper.Init(cn);
            }
            else
                Console.WriteLine("without config");
            Console.WriteLine("-----------------------");
            var envs = Environment.GetEnvironmentVariables();
            Console.WriteLine("Env vars : ");
            foreach (var k in envs.Keys)
            {
                Console.Write(k.ToString().PadRight(35, ' '));
                Console.Write(" : ");
                Console.WriteLine(envs[k].ToString());
            }
            Console.WriteLine("-----------------------");

            DatabaseMaintenanceThread.CheckDb();

#if DEBUG
            NetworkChecker.RunPublicServerCheck();
#endif

            AgentHelper.SetupLocaleFromServer("erza");
            AgentHelper.ReportStart("erza", "monitoring", "security");

            InitIntegrations();

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
