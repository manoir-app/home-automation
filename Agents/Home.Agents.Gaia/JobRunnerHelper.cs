using k8s.Models;
using k8s;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Home.Agents.Gaia
{
    internal static class JobRunnerHelper
    {




        internal static void CheckJobVolume(Kubernetes client, string localFolder, string volumeName)
        {
            // si on est dans home automation, on peut regarder si
            // le dossier existe
            if(localFolder.StartsWith("/home-automation/"))
            {
                if(!Directory.Exists(localFolder))
                    throw new FileNotFoundException(localFolder);
            }


            var vol = DeploymentHelper.GetVolume(client, volumeName);
            var dicCap = new Dictionary<string, ResourceQuantity>();
            dicCap.Add("storage", new ResourceQuantity("20Gi"));

            if (vol == null)
            {
                vol = new V1PersistentVolume()
                {
                    Metadata = new V1ObjectMeta()
                    {
                        Name = volumeName
                    },
                    Spec = new V1PersistentVolumeSpec()
                    {
                        StorageClassName = "manual",
                        AccessModes = new string[] { "ReadWriteOnce" },
                        Capacity = dicCap,
                        HostPath = new V1HostPathVolumeSource(localFolder)
                    }
                };
                client.CreatePersistentVolume(vol);
            }

            var cla = DeploymentHelper.GetVolumeClaim(client, $"{volumeName}-claim");
            if (cla == null)
            {
                cla = new V1PersistentVolumeClaim()
                {
                    ApiVersion = "v1",
                    Metadata = new V1ObjectMeta()
                    {
                        Name = $"{volumeName}-claim"
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

        internal static string RunYoutubeDlJob(Kubernetes client, string sourceUrl)
        {
            Guid g = Guid.NewGuid();
            V1Job job = new V1Job()
            {
                Metadata = new V1ObjectMeta()
                {
                    Name = "ytdl-" + g.ToString("N")
                },
                Spec = new V1JobSpec()
                {
                    TtlSecondsAfterFinished = 100,
                    BackoffLimit= 2,
                    Template = new V1PodTemplateSpec()
                    {
                        Metadata = new V1ObjectMeta()
                        {
                            Name = "ytdl-" + g.ToString("N")
                        },
                        Spec = new V1PodSpec()
                        {
                            RestartPolicy = "Never",
                            Volumes = new V1Volume[]
                            {
                                new V1Volume()
                                {
                                    Name="youtubedl-storage",
                                    PersistentVolumeClaim = new V1PersistentVolumeClaimVolumeSource()
                                    {
                                        ClaimName = "youtubedl-storage-claim"
                                    }
                                }
                            },
                            Containers = new V1Container[]
                            {
                                new V1Container()
                                {
                                    Name = "youtubedl",
                                    Image = "mikenye/youtube-dl",
                                    VolumeMounts = new V1VolumeMount[]
                                    {
                                        new V1VolumeMount()
                                        {
                                            Name = "youtubedl-storage",
                                            MountPath = "/workdir"
                                        }
                                    },
                                    Args = new string[]
                                    {
                                        sourceUrl,
                                        "--write-thumbnail"
                                    }
                                    
                                }
                            }
                        }
                    }
                }
            };

            var ret = client.CreateNamespacedJob(job, "default");
            Console.WriteLine("Résultat :");
            Console.WriteLine(KubernetesYaml.Serialize(ret));

            return job.Metadata.Name;
        }
    }
}
