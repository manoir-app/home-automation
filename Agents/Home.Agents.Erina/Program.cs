using Home.Common;
using System;
using System.Threading;

namespace Home.Agents.Erina
{
    class Program
    {
        static bool _stop = false;

        static void Main(string[] args)
        {
            AgentHelper.WriteStartupMessage("Erina", typeof(Program).Assembly);

            AgentHelper.SetupLocaleFromServer("erina");
            AgentHelper.ReportStart("erina", "pim");


            ErinaMessageHandler.Start();

            while (!_stop)
            {
                Thread.Sleep(500);
                AgentHelper.Ping("erina");
            }

            ErinaMessageHandler.Stop();
        }
    }
}
