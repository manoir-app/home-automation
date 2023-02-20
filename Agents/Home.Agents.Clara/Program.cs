using Home.Agents.Clara;
using Home.Agents.Clara.Calendars.SchoolPlanning;
using Home.Agents.Clara.HomeServices.Providers.HouseKeeping;
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
            new FranceSchoolPlanning().GetNextScheduledItems(DateTimeOffset.Now.AddYears(1));

            AgentHelper.WriteStartupMessage("Clara", typeof(Program).Assembly);

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
            PimSchedulerThread.Stop();
            MainScheduleThread.Stop();
        }
    }
}
