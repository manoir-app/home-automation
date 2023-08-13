using Home.Common;
using Home.Graph.Common;
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

            AgentHelper.SetupReporting("erina");
            AgentHelper.SetupLocaleFromServer("erina");
            AgentHelper.ReportStart("erina", "pim");

            MqttHelper.Start("agents-erian");

            ErinaMessageHandler.Start();

            while (!_stop)
            {
                Thread.Sleep(500);
                AgentHelper.Ping("erina");
            }

            ErinaMessageHandler.Stop();
            MqttHelper.Stop();

        }
    }
}
