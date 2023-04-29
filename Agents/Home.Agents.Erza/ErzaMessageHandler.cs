using Home.Common;
using Home.Common.Messages;
using Home.Graph.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Home.Agents.Erza
{
    public static class ErzaMessageHandler
    {
        public static void Start()
        {

            Thread t = new Thread(() => NatsMessageThread.Run(new string[] { "erza.>",
                                    "users.presence.>",
                                    "system.mesh.globalscenario.changed",
                                    "system.mesh.globalscenario.set",
                                    "system.triggers.change",
                                    LogMessage.LogMessageTopic,
                                    IntegrationInstancesListChangedMessage.InstancesChangedTopic,
                                    IntegrationListChangedMessage.ListChangedTopic,
                                    ScenarioContentChangedMessage.ScenarioChangedTopic,
                                    SourceCodeChangedMessage.SourceCodeChangedTopic,
                                    JournalPageChangedMessage.PageUpdatedTopic,
                                    "security.>", "monitoring.>"},
            ErzaMessageHandler.HandleMessage));
            t.Name = "NatsThread";
            t.Start();

            t = new Thread(() => ServiceBusMessageThread.Run("erza", ErzaMessageHandler.HandleMessage));
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
            if (messageBody == null)
                return MessageResponse.GenericFail;

            messageBody = messageBody.Trim();
            Console.WriteLine("------------------------");
            Console.WriteLine("Message SERVICE BUS Recu:");
            Console.WriteLine(messageBody);
            Console.WriteLine("------------------------");
            
            if(topic.StartsWith("users.presence.", StringComparison.InvariantCultureIgnoreCase))
            {
                return PresenceHelper.HandleMessage(origin, topic, messageBody);
            }
            else
            {
                switch(topic.ToLowerInvariant())
                {
                    case "erza.stop":
                        Program._stop = true;
                        return MessageResponse.OK;
                    case "erza.getmeshinfos":
                    case "system.mesh.globalscenario.changed":
                    case "system.mesh.globalscenario.set":
                        return MeshHelper.HandleMessage(origin, topic, messageBody);
                    case IntegrationInstancesListChangedMessage.InstancesChangedTopic:
                    case IntegrationListChangedMessage.ListChangedTopic:
                        SourceIntegration.GitSync.SyncIntegrationsFromServer(messageBody);
                        break;
                    case "system.triggers.change":
                        SourceIntegration.GitSync.SyncTriggersFromServer(messageBody);
                        break;
                    case "erza.integration.configure":
                        return Program.ConfigureIntegration(JsonConvert.DeserializeObject<IntegrationConfigurationMessage>(messageBody));

                    case JournalPageChangedMessage.PageUpdatedTopic:
                        SourceIntegration.GitSync.SyncPagesFromServer(messageBody);
                        return MessageResponse.OK;
                    case ScenarioContentChangedMessage.ScenarioChangedTopic:
                        SourceIntegration.GitSync.SyncSceneGroupsFromServer(messageBody);
                        return MessageResponse.OK;
                    case SourceCodeChangedMessage.SourceCodeChangedTopic:
                        SourceIntegration.GitSync.SyncFromSources(messageBody);
                        return MessageResponse.OK;
                    case LogMessage.LogMessageTopic:
                        LogMessage logmsg = JsonConvert.DeserializeObject<LogMessage>(messageBody);
                        if (logmsg.Data != null)
                            LogHelper.Log(logmsg.Data);
                        return MessageResponse.OK;
                }
            }

            return MessageResponse.OK;
        }

        
    }
}
