using Home.Common;
using Home.Common.Model;
using k8s;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace Home.Agents.Gaia
{
    public static class KubernetesChecker
    {

        public static void Start()
        {
               // CheckPortForwadings();
            var t = new Thread(() => KubernetesChecker.Run());
            t.Name = "K8 Checker";
            t.Start();
        }

        public static void Stop()
        {
            _stop = true;
        }


        public static bool _stop = false;

        private static void Run()
        {
            while (!_stop)
            {
                CheckAgents();

                for (int i = 0; i < 60 && !_stop; i++)
                {
                    Thread.Sleep(500);
                }
            }

        }

        private static void CheckPortForwadings()
        {
            try
            {
                var config = KubernetesClientConfiguration.InClusterConfig();
                using (var client = new Kubernetes(config))
                {
                    var dep = client.ListNamespacedPod("default");
                    var homemain = (from z in dep.Items
                                    where z.Metadata.Name.StartsWith("home-main-")
                                    select z).FirstOrDefault();
                    if(homemain!=null)
                    {
                        // on récupère le port-forward pour ce module
                        using(var st = client.ConnectGetNamespacedPodPortforward(homemain.Metadata.Name, "default"))
                        using(var rdr = new StreamReader(st))
                        {
                            Console.WriteLine(rdr.ReadToEnd());
                        }

                        
                    }
                    else
                    {
                        Console.WriteLine("home-main not found");
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private static void CheckAgents()
        {
            try
            {
                if (DeploymentHelper._lastDeploy.AddMinutes(3) > DateTime.Now)
                    return;

                var agents = GetAgents();

                var config = KubernetesClientConfiguration.InClusterConfig();
                using (var client = new Kubernetes(config))
                {
                    var dps = client.ListNamespacedDeployment("default", limit: 100);
                    foreach (var dp in dps.Items)
                    {
                        string depName = dp.Metadata.Name?.ToLowerInvariant();
                        if (depName == null)
                            continue;

                        if (depName.StartsWith("agents-"))
                        {
                            CheckAgentState(client, dp, depName, agents);
                        }
                    }
                }

            }
            catch
            {

            }
        }

        private static void CheckAgentState(Kubernetes client, k8s.Models.V1Deployment dp, string depName, List<Agent> agents)
        {
            string agNAme = depName.Substring("agents-".Length);
            var ag = (from z in agents
                      where z.AgentName.Equals(agNAme, StringComparison.InvariantCultureIgnoreCase)
                      select z).FirstOrDefault();
            if (ag == null)
                return;
            var sc = client.ReadNamespacedDeploymentScale(depName, "default");
            if (sc != null)
            {
                if (sc.Spec.Replicas.HasValue && sc.Spec.Replicas == 0)
                {
                    Console.WriteLine("Vérifié le déploiement de " + agNAme + " retour 'arreté' avec la valeur : " + JsonConvert.SerializeObject(sc));
                    // désactivé
                    AgentHelper.UpdateStatusWithInfo(ag.AgentName, "Agent stopped");
                }
                else
                {

                }
            }
        }

        private static List<Agent> GetAgents()
        {
            using (var apicli = new MainApiAgentWebClient("gaia"))
            {
                string url = $"/v1.0/system/mesh/local/agents";
                return apicli.DownloadData<List<Agent>>(url);
            }
        }
    }
}
