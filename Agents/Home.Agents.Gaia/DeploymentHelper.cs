using Home.Common;
using Home.Common.Model;
using k8s;
using k8s.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using System.Text;
//using Microsoft.AspNetCore.JsonPatch;
using k8s.Autorest;
using Home.Graph.Common;
using MongoDB.Driver;

namespace Home.Agents.Gaia
{
    public static class DeploymentHelper
    {
        private static string _tag = null;
        public static string GetImageTag()
        {
            if(_tag==null)
            {
                _tag = Environment.GetEnvironmentVariable("IMAGE_TAG");
                if (_tag == null)
                    _tag = "latest";
            }

            return _tag;
        }

        internal static DateTime _lastDeploy = DateTime.MinValue;

        public static void RefreshOpaqueSecret(string secretName, string filename, string content)
        {
            var config = KubernetesClientConfiguration.InClusterConfig();
            using (var client = new Kubernetes(config))
            {
                var t = GetSecret(client, secretName);
                if (t != null)
                {
                    if (!t.Type.Equals("Opaque", StringComparison.InvariantCultureIgnoreCase))
                        throw new InvalidOperationException("Secret is not an opaque secret");
                    var byteContent = Encoding.UTF8.GetBytes(content);
                    if (t.Data.ContainsKey(filename))
                        t.Data[filename] = byteContent;
                    else
                        t.Data.Add(filename, byteContent);
                    Console.WriteLine($"Updating secret {secretName}");
                    client.ReplaceNamespacedSecret(t, secretName, "default");
                }
            }
        }

        public static V1Secret GetSecret(Kubernetes client, string secretName)
        {
            V1Secret dp = null;
            try
            {
                dp = client.ReadNamespacedSecret(secretName, "default");
            }
            catch (HttpOperationException ex)
            {
                if (ex.Response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    dp = null;
                else
                    throw ex;
            }

            return dp;
        }


        public static void Deploy(string deploymentFile)
        {
            var typeMap = new Dictionary<String, Type>();
            typeMap.Add("v1/Deployment", typeof(V1Deployment));
            typeMap.Add("v1/Pod", typeof(V1Pod));
            typeMap.Add("v1/Service", typeof(V1Service));
            typeMap.Add("v1/Secret", typeof(V1Secret));

            var config = KubernetesClientConfiguration.InClusterConfig();
            using (var client = new Kubernetes(config))
            {
                var items = KubernetesYaml.LoadAllFromString(deploymentFile, typeMap);
                foreach (var r in items)
                {
                    if (r is V1Deployment)
                    {
                        Deploy(r as V1Deployment, client);
                    }
                }


            }
        }

        private static void Deploy(V1Service v1Service, Kubernetes client)
        {
            _lastDeploy = DateTime.Now;
            string serviceName = v1Service.Name();
            V1Service dp = Getservice(client, serviceName);

            if (dp == null)
            {
                Console.WriteLine();
                Console.WriteLine("Service non trouvé, création");
                var ret = client.CreateNamespacedService(v1Service, "default");
                Console.WriteLine("Résultat :");
                Console.WriteLine(KubernetesYaml.Serialize(ret));
            }
            else
            {
                Console.WriteLine();
                Console.WriteLine("Service trouvé, mise à jour");
                var ret = client.ReplaceNamespacedService(v1Service, v1Service.Name(), "default");
                Console.WriteLine("Résultat :");
                Console.WriteLine(KubernetesYaml.Serialize(ret));
            }
        }

        private static void Deploy(V1Ingress v1Ingress, Kubernetes client)
        {
            _lastDeploy = DateTime.Now;
            string ingressName = v1Ingress.Name();
            V1Ingress dp = GetIngress(client, ingressName);

            if (dp == null)
            {
                Console.WriteLine();
                Console.WriteLine("Ingress non trouvé, création");
                var ret = client.CreateNamespacedIngress(v1Ingress, "default");
                Console.WriteLine("Résultat :");
                Console.WriteLine(KubernetesYaml.Serialize(ret));
            }
            else
            {
                Console.WriteLine();
                Console.WriteLine("Ingress trouvé, mise à jour");
                var ret = client.CreateNamespacedIngress(v1Ingress, v1Ingress.Name(), "default");
                Console.WriteLine("Résultat :");
                Console.WriteLine(KubernetesYaml.Serialize(ret));
            }
        }

        private static void Deploy(V1Deployment v1Deployment, Kubernetes client)
        {
            CheckMainVolume(client);

            _lastDeploy = DateTime.Now;
            string deploymentName = v1Deployment.Name();
            V1Deployment dp = GetDeployment(client, deploymentName);

            if (dp == null)
            {
                Console.WriteLine();
                Console.WriteLine("Deploiement non trouvé, création");
                var ret = client.CreateNamespacedDeployment(v1Deployment, "default");
                Console.WriteLine("Résultat :");
                Console.WriteLine(KubernetesYaml.Serialize(ret));
            }
            else
            {
                Console.WriteLine();
                Console.WriteLine("Deploiement trouvé, mise à jour");
                var ret = client.ReplaceNamespacedDeployment(v1Deployment, v1Deployment.Name(), "default");
                Console.WriteLine("Résultat :");
                Console.WriteLine(KubernetesYaml.Serialize(ret));
            }
        }

        private static void CheckMainVolume(Kubernetes client)
        {
            var vol = GetVolume(client, "main-folder");
            var dicCap = new Dictionary<string, ResourceQuantity>();
            dicCap.Add("storage", new ResourceQuantity("20Gi"));

            if (vol == null)
            {
                vol = new V1PersistentVolume()
                {
                    Metadata = new V1ObjectMeta()
                    {
                        Name = "main-folder"
                    },
                    Spec = new V1PersistentVolumeSpec()
                    {
                        StorageClassName = "manual",
                        AccessModes = new string[] { "ReadWriteOnce" },
                        Capacity = dicCap,
                        HostPath = new V1HostPathVolumeSource("/home-automation/")
                    }
                };
                client.CreatePersistentVolume(vol);
            }

            var cla = GetVolumeClaim(client, "main-folder-claim");
            if (cla == null)
            {
                cla = new V1PersistentVolumeClaim()
                {
                    ApiVersion = "v1",
                    Metadata = new V1ObjectMeta()
                    {
                        Name = "main-folder-claim"
                    },
                    Spec = new V1PersistentVolumeClaimSpec()
                    {
                        StorageClassName = "manual",
                        AccessModes = new string[] { "ReadWriteOnce" },
                        Resources = new V1ResourceRequirements()
                        {
                            Requests = dicCap
                        }
                    }
                };
                client.CreateNamespacedPersistentVolumeClaim(cla, "default");
            }
        }

        internal static bool DeployWebApp(string deploymentName)
        {
            _lastDeploy = DateTime.Now;
            AgentHelper.UpdateStatusWithInfo("gaia", $"Deploying webapp {deploymentName}");

            string webAppId = deploymentName;

            if (!deploymentName.StartsWith("webapp-"))
                deploymentName = "webapp-" + deploymentName;
            else
                webAppId = deploymentName.Substring(7);

            var config = KubernetesClientConfiguration.InClusterConfig();
            using (var client = new Kubernetes(config))
            {
                var t = GetDeployment(client, deploymentName);
                if (t != null)
                {
                    Console.WriteLine($"WebApp {webAppId} is already deployed, restarting the deployment");
                    AgentHelper.UpdateStatusWithInfo("gaia", $"WebApp {webAppId} is already deployed, restarting the deployment");
                    RestartDeployment(client, deploymentName);
                    return true;
                }

                MeshExtension extension = new MeshExtension()
                {
                    DockerImageName = "webapps-" + webAppId,
                    Id = webAppId,
                    Title = deploymentName,
                    IsInstalled = false,
                    TechStack = MeshExtensionTechStack.WebSite
                };


                DeployWebApp(client, extension, deploymentName);
                AgentHelper.UpdateStatusWithInfo("gaia", $"App {webAppId} installed");
            }


            return false;
        }

        private static void DeployWebApp(Kubernetes client, MeshExtension extension, string deploymentName)
        {
            var dicLbls = new Dictionary<string, string>();
            string appName = $"webapp-{extension.Id}";
            dicLbls.Add("app", appName);
            dicLbls.Add("tier", $"frontend");
            // d'abord le service
            V1Service svc = new V1Service()
            {
                ApiVersion = "v1",
                Kind = "Service",
                Metadata = new V1ObjectMeta()
                {
                    Name = $"webapp-{extension.Id}-service",
                    Labels = dicLbls
                },
                Spec = new V1ServiceSpec()
                {
                    Type = "NodePort",
                    Ports = new List<V1ServicePort>(new V1ServicePort[]
                    {
                        new V1ServicePort() { Port = 80 }
                    }),
                    Selector = dicLbls
                }
            };

            var localHost = new Uri(HomeServerHelper.GetPublicGraphUrl()).Host;
            if (localHost.StartsWith("public."))
                localHost = "home." + localHost.Substring(7);

            var dicAnnot = new Dictionary<string, string>();
            dicAnnot.Add("nginx.ingress.kubernetes.io/rewrite-target", "/$2");
            V1Ingress ign = new V1Ingress()
            {
                ApiVersion = "networking.k8s.io/v1",
                Kind = "Ingress",
                Metadata = new V1ObjectMeta()
                {
                    Name = $"webapp-{extension.Id}-service",
                    Annotations = dicAnnot
                },
                Spec = new V1IngressSpec()
                {
                    Tls = new List<V1IngressTLS>(new V1IngressTLS[]
                    {
                        new V1IngressTLS()
                        {
                            Hosts = new string []{ localHost },
                            SecretName = "local-certs"
                        }
                    }),
                    Rules = new List<V1IngressRule>(new V1IngressRule[]
                    {
                        new V1IngressRule()
                        {
                            Host = localHost,
                            Http = new V1HTTPIngressRuleValue()
                            {
                                Paths = new List<V1HTTPIngressPath>(new V1HTTPIngressPath[]
                                {
                                    new V1HTTPIngressPath()
                                    {
                                        Path = $"/app/{extension.Id}(/|$)(.*)",
                                        PathType = "Prefix",
                                        Backend = new V1IngressBackend()
                                        {
                                            Service = new V1IngressServiceBackend()
                                            {
                                                Name = $"webapp-{extension.Id}-service",
                                                Port = new V1ServiceBackendPort()
                                                {
                                                    Number = 80
                                                }
                                            }
                                        }
                                    }
                                })
                            }
                        }
                    })
                }
            };

            var env = new List<V1EnvVar>();
            var volumes = new List<V1Volume>();
            var mounts = new List<V1VolumeMount>();
            if (extension.TechStack == MeshExtensionTechStack.AzureFunction)
            {
                volumes.Add(new V1Volume()
                {
                    Name = "functions-keys",
                    Secret = new V1SecretVolumeSource()
                    {
                        SecretName = "function-keys"
                    }
                });
                env.Add(new V1EnvVar()
                {
                    Name = "AzureWebJobsSecretStorageType",
                    Value = "kubernetes"
                });
                mounts.Add(new V1VolumeMount()
                {
                    MountPath = "/run/secrets/functions-keys",
                    Name = "functions-keys",
                    ReadOnlyProperty = true
                });
            }
            else
            {
                volumes.Add(new V1Volume()
                {
                    Name = appName + "-storage",
                    PersistentVolumeClaim = new V1PersistentVolumeClaimVolumeSource()
                    {
                        ClaimName = "main-folder-claim"
                    }
                });
                mounts.Add(new V1VolumeMount()
                {
                    Name = appName + "-storage",
                    MountPath = "/home-automation/"
                });

            }

            env.Add(new V1EnvVar()
            {
                Name = "APPCONFIG_CNSTRING",
                ValueFrom = new V1EnvVarSource()
                {
                    SecretKeyRef = new V1SecretKeySelector()
                    {
                        Name = "appconfig",
                        Key = "connection"
                    }
                }
            });

            var apiKey = Environment.GetEnvironmentVariable("HOMEAUTOMATION_APIKEY");
            if (!string.IsNullOrEmpty(apiKey))
            {
                env.Add(new V1EnvVar()
                {
                    Name = "HOMEAUTOMATION_APIKEY",
                    Value = apiKey
                });
            }

            env.Add(new V1EnvVar()
            {
                Name = "LAN_SERVER_IP",
                Value = HomeServerHelper.GetLocalIP()
            });

            var imgTag = Environment.GetEnvironmentVariable("IMAGE_TAG");
            if(!string.IsNullOrEmpty(imgTag))
            {
                env.Add(new V1EnvVar()
                {
                    Name = "IMAGE_TAG",
                    Value = imgTag
                });
            }


            V1Deployment d = new V1Deployment()
            {
                ApiVersion = "apps/v1",
                Kind = "Deployment",
                Metadata = new V1ObjectMeta()
                {
                    Name = deploymentName
                },
                Spec = new V1DeploymentSpec()
                {
                    Selector = new V1LabelSelector()
                    {
                        MatchLabels = dicLbls
                    },
                    Strategy = new V1DeploymentStrategy()
                    {
                        Type = "Recreate"
                    },
                    Template = new V1PodTemplateSpec()
                    {
                        Metadata = new V1ObjectMeta()
                        {
                            Labels = dicLbls
                        },
                        Spec = new V1PodSpec()
                        {

                        }
                    }

                }
            };

            var url = Environment.GetEnvironmentVariable("DOCKER_CONTAINER_REGISTRY");
            if (url == null)
                url = HomeServerHelper.GetLocalIP() + ":5000";

            var cnts = new List<V1Container>();
            cnts.Add(new V1Container()
            {
                Image = url + "/" + extension.DockerImageName + ":" + GetImageTag(),
                ImagePullPolicy = "Always",
                Name = deploymentName,
                Env = env,
                Ports = new List<V1ContainerPort>(new V1ContainerPort[]
                {
                    new V1ContainerPort() { ContainerPort=80}
                }),
                VolumeMounts = mounts
            });
            d.Spec.Template.Spec.Containers = cnts;
            d.Spec.Template.Spec.Volumes = volumes;

            Console.WriteLine("Déploiement de : " + KubernetesYaml.Serialize(d));
            Console.WriteLine("Déploiement de : " + KubernetesYaml.Serialize(ign));
            Console.WriteLine("Déploiement de : " + KubernetesYaml.Serialize(svc));

            Deploy(svc, client);
            Deploy(ign, client);
            Deploy(d, client);

        }


        internal static bool DeployExtension(string deploymentName)
        {
            _lastDeploy = DateTime.Now;
            AgentHelper.UpdateStatusWithInfo("gaia", $"Deploying extension {deploymentName}");

            string extensionId = deploymentName;

            if (!deploymentName.StartsWith("extensions-"))
                deploymentName = "extensions-" + deploymentName;
            else
                extensionId = deploymentName.Substring(11);

            var config = KubernetesClientConfiguration.InClusterConfig();
            using (var client = new Kubernetes(config))
            {
                var t = GetDeployment(client, deploymentName);
                if (t != null)
                {
                    Console.WriteLine($"Extension {extensionId} is already deployed, restarting the deployment");
                    AgentHelper.UpdateStatusWithInfo("gaia", $"Extension {extensionId} is already deployed, restarting the deployment");
                    RestartDeployment(client, deploymentName);
                    return true;
                }

                MeshExtension extension = null;
                for (int i = 0; i < 3; i++)
                {
                    try
                    {
                        using (var cli = new MainApiAgentWebClient("gaia"))
                        {
                            extension = cli.DownloadData<MeshExtension>($"v1.0/system/mesh/local/extensions/{extensionId}");
                            break;
                        }
                    }
                    catch
                    {

                    }
                }

                if (extension == null)
                {
                    Console.WriteLine($"Extension {extensionId} not found");
                    AgentHelper.UpdateStatusWithInfo("gaia", $"Extension {extensionId} not found");
                    return false;
                }

                DeployExtension(client, extension, deploymentName);
                AgentHelper.UpdateStatusWithInfo("gaia", $"Extension {extensionId} installed");
                for (int i = 0; i < 3; i++)
                {
                    try
                    {
                        using (var cli = new MainApiAgentWebClient("gaia"))
                        {
                            bool done = cli.DownloadData<bool>($"v1.0/system/mesh/local/extensions/{extensionId}/setinstalled");
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                }

            }


            return false;
        }

        private static void DeployExtension(Kubernetes client, MeshExtension extension, string deploymentName)
        {
            var dicLbls = new Dictionary<string, string>();
            string appName = $"extensions-{extension.Id}";
            dicLbls.Add("app", appName);
            dicLbls.Add("tier", $"frontend");
            // d'abord le service
            V1Service svc = new V1Service()
            {
                ApiVersion = "v1",
                Kind = "Service",
                Metadata = new V1ObjectMeta()
                {
                    Name = $"extensions-{extension.Id}-service",
                    Labels = dicLbls
                },
                Spec = new V1ServiceSpec()
                {
                    Type = "NodePort",
                    Ports = new List<V1ServicePort>(new V1ServicePort[]
                    {
                        new V1ServicePort() { Port = 80 }
                    }),
                    Selector = dicLbls
                }
            };

            var localHost = new Uri(HomeServerHelper.GetPublicGraphUrl()).Host;
            if (localHost.StartsWith("public."))
                localHost = "home." + localHost.Substring(7);

            var dicAnnot = new Dictionary<string, string>();
            dicAnnot.Add("nginx.ingress.kubernetes.io/rewrite-target", "/$2");
            V1Ingress ign = new V1Ingress()
            {
                ApiVersion = "networking.k8s.io/v1",
                Kind = "Ingress",
                Metadata = new V1ObjectMeta()
                {
                    Name = $"extensions-{extension.Id}-service",
                    Annotations = dicAnnot
                },
                Spec = new V1IngressSpec()
                {
                    Tls = new List<V1IngressTLS>(new V1IngressTLS[]
                    {
                        new V1IngressTLS()
                        {
                            Hosts = { localHost },
                            SecretName = "local-certs"
                        }
                    }),
                    Rules = new List<V1IngressRule>(new V1IngressRule[]
                    {
                        new V1IngressRule()
                        {
                            Http = new V1HTTPIngressRuleValue()
                            {
                                Paths = new List<V1HTTPIngressPath>(new V1HTTPIngressPath[]
                                {
                                    new V1HTTPIngressPath()
                                    {
                                        Path = $"/ext/{extension.Id}(/|$)(.*)",
                                        PathType = "Prefix",
                                        Backend = new V1IngressBackend()
                                        {
                                            Service = new V1IngressServiceBackend()
                                            {
                                                Name = $"extensions-{extension.Id}-service",
                                                Port = new V1ServiceBackendPort()
                                                {
                                                    Number = 80
                                                }
                                            }
                                        }
                                    }
                                })
                            }
                        }
                    })
                }
            };

            var env = new List<V1EnvVar>();
            var volumes = new List<V1Volume>();
            var mounts = new List<V1VolumeMount>();
            if (extension.TechStack == MeshExtensionTechStack.AzureFunction)
            {
                volumes.Add(new V1Volume()
                {
                    Name = "functions-keys",
                    Secret = new V1SecretVolumeSource()
                    {
                        SecretName = "function-keys"
                    }
                });
                env.Add(new V1EnvVar()
                {
                    Name = "AzureWebJobsSecretStorageType",
                    Value = "kubernetes"
                });
                mounts.Add(new V1VolumeMount()
                {
                    MountPath = "/run/secrets/functions-keys",
                    Name = "functions-keys",
                    ReadOnlyProperty = true
                });
            }
            else
            {
                volumes.Add(new V1Volume()
                {
                    Name = appName + "-storage",
                    PersistentVolumeClaim = new V1PersistentVolumeClaimVolumeSource()
                    {
                        ClaimName = "main-folder-claim"
                    }
                });
                mounts.Add(new V1VolumeMount()
                {
                    Name = appName + "-storage",
                    MountPath = "/home-automation/"
                });

            }

            var imgTag = Environment.GetEnvironmentVariable("IMAGE_TAG");
            if (!string.IsNullOrEmpty(imgTag))
            {
                env.Add(new V1EnvVar()
                {
                    Name = "IMAGE_TAG",
                    Value = imgTag
                });
            }

            env.Add(new V1EnvVar()
            {
                Name = "APPCONFIG_CNSTRING",
                ValueFrom = new V1EnvVarSource()
                {
                    SecretKeyRef = new V1SecretKeySelector()
                    {
                        Name = "appconfig",
                        Key = "connection"
                    }
                }
            });

            var apiKey = Environment.GetEnvironmentVariable("HOMEAUTOMATION_APIKEY");
            if (!string.IsNullOrEmpty(apiKey))
            {
                env.Add(new V1EnvVar()
                {
                    Name = "HOMEAUTOMATION_APIKEY",
                    Value = apiKey
                });
            }

            env.Add(new V1EnvVar()
            {
                Name = "LAN_SERVER_IP",
                Value = HomeServerHelper.GetLocalIP()
            });


            V1Deployment d = new V1Deployment()
            {
                ApiVersion = "apps/v1",
                Kind = "Deployment",
                Metadata = new V1ObjectMeta()
                {
                    Name = deploymentName
                },
                Spec = new V1DeploymentSpec()
                {
                    Selector = new V1LabelSelector()
                    {
                        MatchLabels = dicLbls
                    },
                    Strategy = new V1DeploymentStrategy()
                    {
                        Type = "Recreate"
                    },
                    Template = new V1PodTemplateSpec()
                    {
                        Metadata = new V1ObjectMeta()
                        {
                            Labels = dicLbls
                        },
                        Spec = new V1PodSpec()
                        {

                        }
                    }

                }
            };

            var url = Environment.GetEnvironmentVariable("DOCKER_CONTAINER_REGISTRY");
            if (url == null)
                url = HomeServerHelper.GetLocalIP() + ":5000";

            var cnts = new List<V1Container>();
            cnts.Add(new V1Container()
            {
                Image = url + "/" + extension.DockerImageName + ":" + GetImageTag(),
                Name = deploymentName,
                Env = env,
                Ports = new List<V1ContainerPort>(new V1ContainerPort[]
                {
                    new V1ContainerPort() { ContainerPort=80}
                }),
                VolumeMounts = mounts


            });
            d.Spec.Template.Spec.Containers = cnts;
            d.Spec.Template.Spec.Volumes = volumes;

            Console.WriteLine("Déploiement de : " + KubernetesYaml.Serialize(d));

            Deploy(svc, client);
            Deploy(ign, client);
            Deploy(d, client);

        }

        public static V1Deployment GetDeployment(string deploymentName)
        {
            var config = KubernetesClientConfiguration.InClusterConfig();
            using (var client = new Kubernetes(config))
            {
                return GetDeployment(client, deploymentName);
            }
        }

        private static V1Deployment GetDeployment(Kubernetes client, string deploymentName)
        {
            V1Deployment dp = null;
            try
            {
                dp = client.ReadNamespacedDeployment(deploymentName, "default");
            }
            catch (HttpOperationException ex)
            {
                if (ex.Response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    dp = null;
                else
                    throw ex;
            }

            return dp;
        }

        private static V1PersistentVolume GetVolume(Kubernetes client, string volumeName)
        {
            V1PersistentVolume dp = null;
            try
            {
                dp = client.ReadPersistentVolume(volumeName);
            }
            catch (HttpOperationException ex)
            {
                if (ex.Response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    dp = null;
                else
                    throw ex;
            }

            return dp;
        }

        private static V1PersistentVolumeClaim GetVolumeClaim(Kubernetes client, string volumeClaimName)
        {
            V1PersistentVolumeClaim dp = null;
            try
            {
                dp = client.ReadNamespacedPersistentVolumeClaim(volumeClaimName, "default");
            }
            catch (HttpOperationException ex)
            {
                if (ex.Response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    dp = null;
                else
                    throw ex;
            }

            return dp;
        }


        private static V1Ingress GetIngress(Kubernetes client, string ingressName)
        {
            V1Ingress dp = null;
            try
            {
                dp = client.ReadNamespacedIngress(ingressName, "default");
            }
            catch (HttpOperationException ex)
            {
                if (ex.Response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    dp = null;
                else
                    throw ex;
            }

            return dp;
        }

        private static V1Service Getservice(Kubernetes client, string serviceName)
        {
            V1Service dp = null;
            try
            {
                dp = client.ReadNamespacedService(serviceName, "default");
            }
            catch (HttpOperationException ex)
            {
                if (ex.Response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    dp = null;
                else
                    throw ex;
            }

            return dp;
        }

        internal static void DeployAgent(string agentName)
        {
            _lastDeploy = DateTime.Now;
            if (agentName.StartsWith("agents-"))
                agentName = agentName.Substring(6);

            Console.WriteLine("----------------");
            Console.WriteLine("Deploying : " + agentName);
            Console.WriteLine("---");

            AgentHelper.UpdateStatusWithInfo("gaia", $"Deploying agent {agentName}");


            V1Deployment d = new V1Deployment()
            {
                ApiVersion = "apps/v1",
                Kind = "Deployment",
                Metadata = new V1ObjectMeta()
                {
                    Name = "agents-" + agentName
                },
                Spec = new V1DeploymentSpec()
                {
                    Selector = new V1LabelSelector()
                    {

                    },
                    Strategy = new V1DeploymentStrategy()
                    {
                        Type = "Recreate"
                    },
                    Template = new V1PodTemplateSpec()
                    {
                        Metadata = new V1ObjectMeta()
                        {

                        },
                        Spec = new V1PodSpec()
                        {

                        }
                    }

                }
            };

            var lblsMatch = new Dictionary<string, string>();
            lblsMatch.Add("app", d.Metadata.Name);

            d.Spec.Selector.MatchLabels = lblsMatch;
            d.Spec.Template.Metadata.Labels = lblsMatch;

            var env = new List<V1EnvVar>();
            env.Add(new V1EnvVar()
            {
                Name = "APPCONFIG_CNSTRING",
                ValueFrom = new V1EnvVarSource()
                {
                    SecretKeyRef = new V1SecretKeySelector()
                    {
                        Name = "appconfig",
                        Key = "connection"
                    }
                }
            });

            var apiKey = Environment.GetEnvironmentVariable("HOMEAUTOMATION_APIKEY");
            if (!string.IsNullOrEmpty(apiKey))
            {
                env.Add(new V1EnvVar()
                {
                    Name = "HOMEAUTOMATION_APIKEY",
                    Value = apiKey
                });
            }

            env.Add(new V1EnvVar()
            {
                Name = "LAN_SERVER_IP",
                Value = HomeServerHelper.GetLocalIP()
            });

            var imgTag = Environment.GetEnvironmentVariable("IMAGE_TAG");
            if (!string.IsNullOrEmpty(imgTag))
            {
                env.Add(new V1EnvVar()
                {
                    Name = "IMAGE_TAG",
                    Value = imgTag
                });
            }

            var url = Environment.GetEnvironmentVariable("DOCKER_CONTAINER_REGISTRY");
            if (url == null)
                url = HomeServerHelper.GetLocalIP() + ":5000";

            List<V1Volume> volumes = new List<V1Volume>();
            volumes.Add(new V1Volume()
            {
                Name = agentName + "-storage",
                PersistentVolumeClaim = new V1PersistentVolumeClaimVolumeSource()
                {
                    ClaimName = "main-folder-claim"
                }
            });
            List<V1VolumeMount> volumesMount = new List<V1VolumeMount>();
            volumesMount.Add(new V1VolumeMount()
            {
                Name = agentName + "-storage",
                MountPath = "/home-automation/"
            });

            var cnts = new List<V1Container>();
            cnts.Add(new V1Container()
            {
                Image = url + "/" + agentName + ":" + GetImageTag(),
                Name = agentName,
                Env = env,
                VolumeMounts = volumesMount
            });
            d.Spec.Template.Spec.Containers = cnts;
            d.Spec.Template.Spec.Volumes = volumes;

            Console.WriteLine(KubernetesYaml.Serialize(d));

            try
            {
                var config = KubernetesClientConfiguration.InClusterConfig();
                using (var client = new Kubernetes(config))
                {
                    Deploy(d, client);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw ex;
            }

        }

        public static void RestartDeployment(string deploymentName)
        {
            _lastDeploy = DateTime.Now;
            AgentHelper.UpdateStatusWithInfo("gaia", $"Restarting {deploymentName}");

            var config = KubernetesClientConfiguration.InClusterConfig();
            using (var client = new Kubernetes(config))
            {
                RestartDeployment(client, deploymentName);
            }

            Thread.Sleep(2000);
            AgentHelper.UpdateStatusWithInfo("gaia", $"Restarted {deploymentName}");
            LogHelper.Log("agent", "gaia", $"Restarted {deploymentName}");
        }

        private static void RestartDeployment(Kubernetes client, string deploymentName)
        {
            V1Deployment dp = null;

            try
            {
                dp = client.ReadNamespacedDeployment(deploymentName, "default");
            }
            catch (HttpOperationException ex)
            {
                if (ex.Response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    dp = null;
                else
                    throw ex;
            }

            if (dp != null)
            {
                var patchStr = @" { ""spec"":
                        { ""template"": 
                            { ""metadata"": 
                                { ""annotations"": 
                                    {
                                    ""kubectl.kubernetes.io/restartedAt"": """ + DateTime.UtcNow.ToString("s") + @"""
                                    }
                                }
                            }
                        }
                }";

                var k8sPatch = new V1Patch(patchStr, V1Patch.PatchType.MergePatch);
                client.PatchNamespacedDeployment(k8sPatch, deploymentName, "default");
            }
        }

        public static void DeleteExtension(string extensionName)
        {
            var deploymentName = extensionName;
            var serviceName = extensionName;

            if (!deploymentName.StartsWith("extensions-"))
                deploymentName = "extensions-" + deploymentName;

            if (!serviceName.StartsWith("extensions-"))
                serviceName = "extensions-" + serviceName + "-service";

            DeleteDeployment(deploymentName);
            DeleteIngress(serviceName);
            DeleteService(serviceName);

            AgentHelper.UpdateStatusWithInfo("gaia", $"Extension {extensionName} uninstalled");

            for (int i = 0; i < 3; i++)
            {
                try
                {
                    using (var cli = new MainApiAgentWebClient("gaia"))
                    {
                        bool done = cli.DownloadData<bool>($"v1.0/system/mesh/local/extensions/{extensionName}/setinstalled?newStatus=false");
                        break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }

        }

        public static void DeleteIngress(string ingressName)
        {
            var config = KubernetesClientConfiguration.InClusterConfig();
            using (var client = new Kubernetes(config))
            {
                V1Ingress dp = null;

                try
                {
                    dp = client.ReadNamespacedIngress(ingressName, "default");
                }
                catch (HttpOperationException ex)
                {
                    if (ex.Response.StatusCode == System.Net.HttpStatusCode.NotFound)
                        dp = null;
                    else
                        throw ex;
                }

                if (dp == null)
                    return;

                var t = client.DeleteNamespacedIngress(ingressName, "default");

                for (int i = 0; i < 30; i++)  // on attends 30 sec max que le déploiement s'arrete
                {
                    Thread.Sleep(1000);
                    AgentHelper.UpdateStatusWithInfo("gaia", $"Waiting for {ingressName} deletion");
                    try
                    {
                        dp = client.ReadNamespacedIngress(ingressName, "default");
                    }
                    catch (HttpOperationException ex)
                    {
                        if (ex.Response.StatusCode == System.Net.HttpStatusCode.NotFound)
                        {
                            AgentHelper.UpdateStatusWithInfo("gaia", $"Deleted {ingressName}");
                            return;
                        }

                    }
                }
            }
        }

        public static void DeleteService(string serviceName)
        {
            var config = KubernetesClientConfiguration.InClusterConfig();
            using (var client = new Kubernetes(config))
            {
                V1Service dp = null;

                try
                {
                    dp = client.ReadNamespacedService(serviceName, "default");
                }
                catch (HttpOperationException ex)
                {
                    if (ex.Response.StatusCode == System.Net.HttpStatusCode.NotFound)
                        dp = null;
                    else
                        throw ex;
                }

                if (dp == null)
                    return;

                var t = client.DeleteNamespacedService(serviceName, "default");

                for (int i = 0; i < 30; i++)  // on attends 30 sec max que le déploiement s'arrete
                {
                    Thread.Sleep(1000);
                    AgentHelper.UpdateStatusWithInfo("gaia", $"Waiting for {serviceName} deletion");
                    try
                    {
                        dp = client.ReadNamespacedService(serviceName, "default");
                    }
                    catch (HttpOperationException ex)
                    {
                        if (ex.Response.StatusCode == System.Net.HttpStatusCode.NotFound)
                        {
                            AgentHelper.UpdateStatusWithInfo("gaia", $"Deleted {serviceName}");
                            return;
                        }

                    }
                }
            }
        }

        public static void DeleteDeployment(string deploymentName)
        {
            _lastDeploy = DateTime.Now;
            AgentHelper.UpdateStatusWithInfo("gaia", $"Deleting {deploymentName}");

            var config = KubernetesClientConfiguration.InClusterConfig();
            using (var client = new Kubernetes(config))
            {
                V1Deployment dp = null;

                try
                {
                    dp = client.ReadNamespacedDeployment(deploymentName, "default");
                }
                catch (HttpOperationException ex)
                {
                    if (ex.Response.StatusCode == System.Net.HttpStatusCode.NotFound)
                        dp = null;
                    else
                        throw ex;
                }

                if (dp == null)
                    return;

                var t = client.DeleteNamespacedDeployment(deploymentName, "default");

                for (int i = 0; i < 30; i++)  // on attends 30 sec max que le déploiement s'arrete
                {
                    Thread.Sleep(1000);
                    AgentHelper.UpdateStatusWithInfo("gaia", $"Waiting for {deploymentName} deletion");
                    try
                    {
                        dp = client.ReadNamespacedDeployment(deploymentName, "default");
                    }
                    catch (HttpOperationException ex)
                    {
                        if (ex.Response.StatusCode == System.Net.HttpStatusCode.NotFound)
                        {
                            AgentHelper.UpdateStatusWithInfo("gaia", $"Deleted {deploymentName}");
                            return;
                        }

                    }
                }
            }
        }
    }
}