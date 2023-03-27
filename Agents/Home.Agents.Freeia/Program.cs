using Home.Common;
using System;
using System.IO;
using System.Threading;

namespace Home.Agents.Freeia
{
    class Program
    {
        public static bool _stop = false;

        static void Main(string[] args)
        {
            AgentHelper.WriteStartupMessage("Freeia", typeof(Program).Assembly);

            AgentHelper.SetupReporting("freeia");
            AgentHelper.SetupLocaleFromServer("freeia");
            AgentHelper.ReportStart("freeia", "monitoring");


            MqttHandler.Start();
            FreeiaMessageHandler.Start();
            FreeboxHelper.Start();
            while (!_stop)
            {
                Thread.Sleep(500);
                AgentHelper.Ping("freeia");
            }

            FreeiaMessageHandler.Stop();
            FreeboxHelper.Stop();
            MqttHandler.Stop();
        }
    }
}
