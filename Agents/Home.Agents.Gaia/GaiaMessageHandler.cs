using Home.Common;
using Home.Common.Messages;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

namespace Home.Agents.Gaia
{
    static class GaiaMessageHandler
    {

        public static void Start()
        {
            Thread t = new Thread(() => NatsMessageThread.Run(new string[] {"gaia.>",
                                    "system.extensions.>",
                                    "security.>", "monitoring.>"},
            GaiaMessageHandler.HandleMessage));
            t.Name = "NatsThread";
            t.Start();

            t = new Thread(() => ServiceBusMessageThread.Run("gaia", GaiaMessageHandler.HandleMessage));
            t.Name = "SericeBusThread";
            t.Start();

        }


        public static void Stop()
        {
            ServiceBusMessageThread.Stop();
            NatsMessageThread.Stop();
        }

        public static MessageResponse HandleMessage(MessageOrigin origin, string topic, string messageBody)
        {
            if (string.IsNullOrEmpty(messageBody))
                return MessageResponse.GenericFail;

            MeshExtensionOperationMessage msgExt;
            string deploymentName;

            switch (topic.ToLowerInvariant())
            {
                case "gaia.stop":
                    Program._stop = true;
                    break;

                case SystemReverseProxyChangeMessage.TopicName:
                    var msgFrp = SystemReverseProxyChangeMessage.ReadAs<SystemReverseProxyChangeMessage>(messageBody);
                    DeploymentHelper.RefreshOpaqueSecret("frpc-config", "frpc.ini", msgFrp.FrpConfigFile);
                    DeploymentHelper.RestartDeployment("frpc");
                    return MessageResponse.OK;

                case SystemCertificateChangeMessage.TopicName:
                    return ChangeCertificate(messageBody);

                case MeshExtensionOperationMessage.TopicCreate:
                    msgExt = MeshExtensionOperationMessage.ReadAs<MeshExtensionOperationMessage>(messageBody);
                    if (!DeploymentHelper.DeployExtension(msgExt.ExtensionId))
                        return MessageResponse.GenericFail;
                    return MessageResponse.OK;


                case MeshExtensionOperationMessage.TopicRestart:
                    msgExt = MeshExtensionOperationMessage.ReadAs<MeshExtensionOperationMessage>(messageBody);
                    deploymentName = msgExt.ExtensionId;
                    if (deploymentName == null)
                        return MessageResponse.GenericFail;
                    if (!deploymentName.StartsWith("extensions-"))
                        deploymentName = "extensions-" + deploymentName;
                    DeploymentHelper.RestartDeployment(deploymentName);
                    return MessageResponse.OK;

                case MeshExtensionOperationMessage.TopicTerminate:
                    msgExt = MeshExtensionOperationMessage.ReadAs<MeshExtensionOperationMessage>(messageBody);
                     deploymentName = msgExt.ExtensionId;
                    if (deploymentName == null)
                        return MessageResponse.GenericFail;
                    DeploymentHelper.DeleteExtension(deploymentName);
                    return MessageResponse.OK;
                    
                case SystemDeploymentMessage.TopicName:
                    var t = SystemDeploymentMessage.ReadAs<SystemDeploymentMessage>(messageBody);
                    if (t != null)
                    {
                        switch(t.Action)
                        {
                            case SystemDeploymentAction.Restart:
                                DeploymentHelper.RestartDeployment(t.DeploymentName);
                                break;
                            case SystemDeploymentAction.DeployGeneric:
                                DeploymentHelper.Deploy(t.SourceFileContent);
                                break;
                            case SystemDeploymentAction.DeployAgent:
                                DeploymentHelper.DeployAgent(t.DeploymentName);
                                break;
                            case SystemDeploymentAction.DeployWebApp:
                                DeploymentHelper.DeployWebApp(t.DeploymentName);
                                break;
                            default:
                                Console.Error.WriteLine("Message inconnu : " + messageBody);
                                break;
                        }
                    }
                    break;
                default:
                    Console.Error.WriteLine("Message inconnu : " + messageBody);
                    break;
            }

            return MessageResponse.OK;
        }

        private static MessageResponse ChangeCertificate(string messageBody)
        {
            var msgCertif = SystemCertificateChangeMessage.ReadAs<SystemCertificateChangeMessage>(messageBody);
            string pth = Path.Combine("/home-automation/frps/", "generic.pem.crt");
            if(File.Exists(pth))
                DeploymentHelper.RefreshOpaqueSecret("local-certs", "tls.crt", File.ReadAllText(pth));
            pth = Path.Combine("/home-automation/frps/", "generic.pem.key");
            if (File.Exists(pth))
                DeploymentHelper.RefreshOpaqueSecret("local-certs", "tls.key", File.ReadAllText(pth));
            return MessageResponse.OK;
        }
    }
}
