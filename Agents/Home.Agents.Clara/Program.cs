using Home.Agents.Clara;
using Home.Agents.Clara.HomeServices.Integrations.HouseKeeping;
using Home.Common;
using Home.Graph.Common;
using System;
using System.Threading;

namespace Home.Agents.Clara
{
    class Program
    {
        public static bool _stop = false;

        static void Main(string[] args)
        {
            Console.WriteLine("Starting Clara");

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

            AgentHelper.SetupLocaleFromServer("clara");
            AgentHelper.ReportStart("clara", "pim");

            MainScheduleThread.Start();
            PimSchedulerThread.Start();
            ClaraMessageHandler.Start();
            NewsChecker.Start();
            DownloadSourceChecker.Start();


            var exitCode = Microsoft.Playwright.Program.Main(new[] { "install" });
            if (exitCode != 0)
            {
                Console.WriteLine("Failed to install browsers for playwright");
            }

            while (!_stop)
            {
                Thread.Sleep(500);
                AgentHelper.Ping("clara");
            }

            DownloadSourceChecker.Stop();
            NewsChecker.Stop();
            ClaraMessageHandler.Stop();
            PimSchedulerThread.Start();
            MainScheduleThread.Start();
        }
    }
}
