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
            Console.WriteLine("Starting Erina");
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
