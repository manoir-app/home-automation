using Home.Common;
using k8s;
using System;
using System.Threading;

namespace Home.Agents.Gaia
{
    class Program
    {
        public static bool _stop = false;

        static void Main(string[] args)
        {
            AgentHelper.WriteStartupMessage("Gaïa", typeof(Program).Assembly);


            AgentHelper.SetupReporting("gaia");
            AgentHelper.SetupLocaleFromServer("gaia");
            AgentHelper.ReportStart("gaia", "monitoring", "system");

            // au démarrage, on prépare un reboot de tous les agents
            // dans 5 minutes.
            new Thread(() =>
            {
                Thread.Sleep(5 * 60000);
                KubernetesChecker.RestartAfterReboot();
            }).Start();



            GaiaMessageHandler.Start();
            KubernetesChecker.Start();
            MqttHandler.Start();
            while (!_stop)
            {
                Thread.Sleep(500);
                AgentHelper.Ping("gaia");
            }
            MqttHandler.Stop();
            KubernetesChecker.Stop();
            GaiaMessageHandler.Stop();

        }
    }
}
