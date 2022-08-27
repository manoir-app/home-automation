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
            Console.WriteLine("Starting Freeia");
            string cn = Environment.GetEnvironmentVariable("APPCONFIG_CNSTRING");

            AgentHelper.SetupLocaleFromServer("freeia");
            AgentHelper.ReportStart("freeia", "monitoring");

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
