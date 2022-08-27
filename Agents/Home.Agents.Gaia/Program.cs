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
            DeploymentHelper.DeployWebApp("security");

            Console.WriteLine("Starting Gaïa");
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

            try
            {
                var config = KubernetesClientConfiguration.InClusterConfig();
                Console.WriteLine($"host for k8s configuration : {config.Host}");
                Console.WriteLine("-----------------------");

            }
            catch
            {

            }


                AgentHelper.SetupLocaleFromServer("gaia");
            AgentHelper.ReportStart("gaia", "monitoring", "system");


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
