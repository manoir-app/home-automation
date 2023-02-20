using Home.Common;
using Home.Common.Model;
using k8s;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
            DateTimeOffset lastCheckCertificate = DateTimeOffset.MinValue;
            while (!_stop)
            {
                CheckAgents();

                if (Math.Abs((DateTimeOffset.Now - lastCheckCertificate).TotalMinutes) > 5)
                {
                    lastCheckCertificate = DateTimeOffset.Now;
                    CheckCertificates();
                }

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
                    if (homemain != null)
                    {
                        // on récupère le port-forward pour ce module
                        using (var st = client.ConnectGetNamespacedPodPortforward(homemain.Metadata.Name, "default"))
                        using (var rdr = new StreamReader(st))
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
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }


        private static void CheckCertificates()
        {
            Console.WriteLine("Checking local certificates");
            try
            {
                var tmpK8 = DeploymentHelper.GetSecret("local-certs");
                CheckAndUpdate(tmpK8, Path.Combine("/home-automation/frps/", "generic.pem.crt"), "tls.crt");
                CheckAndUpdate(tmpK8, Path.Combine("/home-automation/frps/", "generic.pem.key"), "tls.key");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private static void CheckAndUpdate(k8s.Models.V1Secret tmpK8, string pth, string k8sDataName)
        {
            if (File.Exists(pth))
            {
                string tmp = File.ReadAllText(pth);
                var bs = UTF8Encoding.UTF8.GetBytes(tmp);
                if (tmpK8 == null)
                {
                    Console.WriteLine("Local certificates not found, updating " + k8sDataName);
                    DeploymentHelper.RefreshOpaqueSecret("local-certs", k8sDataName, tmp);
                    return;
                }
                else
                {
                    byte[] bsK8 = null;
                    if (!tmpK8.Data.TryGetValue(k8sDataName, out bsK8))
                    {
                        Console.WriteLine("Local certificates not complete, updating " + k8sDataName);
                        DeploymentHelper.RefreshOpaqueSecret("local-certs", k8sDataName, tmp);
                        return;
                    }
                    else
                    {
                        bool diff = bs.Length != bsK8.Length;
                        for (int i = 0; i < bs.Length && i < bsK8.Length; i++)
                        {
                            if (bs[i] != bsK8[i])
                                diff = true;
                        }
                        if (diff)
                        {
                            Console.WriteLine("Local certificates not up-to-date, updating " + k8sDataName);
                            GaiaMessageHandler.ChangeCertificate("");
                            return;
                        }
                    }
                }
                Console.WriteLine("Certificate was updated to date");
            }
            else
            {
                Console.WriteLine("Local certificates file not found : " + pth);
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
