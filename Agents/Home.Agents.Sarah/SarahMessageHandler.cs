using Home.Common;
using Home.Common.Messages;
using Home.Graph.Common.Scripting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Home.Agents.Sarah
{
    public static class SarahMessageHandler
    {
        public static string[] AllTopics = new string[] { "sarah.>",
                "homeautomation.scenario.>",
                "pim.scheduler.update.wakeuptime",
                SystemScriptExecuteMessage.ExecuteScriptTopic,
                "system.triggers.change"};

        public static void Start()
        {
            // Attention, dans cet agent, il y a une deuxième classe
            // qui lit les messages NATS => DeviceManager
            Thread t = new Thread(() => NatsMessageThread.Run(AllTopics,
            SarahMessageHandler.HandleMessage));
            t.Name = "NatsThread";
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
            Console.Write("Message Recu:");
            Console.WriteLine(topic);
            Console.WriteLine(messageBody);
            Console.WriteLine("------------------------");

            switch (topic.ToLowerInvariant())
            {
                case "pim.scheduler.update.wakeuptime":
                    TriggersChecker.RefreshRoutines();
                    return MessageResponse.OK;
                case "sarah.stop":
                    Program._stop = true;
                    return MessageResponse.OK;
                case "system.script.execute.direct":
                    return ScriptingHelper.HandleMessage("sarah", new SystemScriptExecuteMessage(SystemScriptExecuteMessage.ExecuteScriptTopic) 
                            { ScriptContent = messageBody});
                case SystemScriptExecuteMessage.ExecuteScriptTopic:
                    return ScriptingHelper.HandleMessage("sarah", JsonConvert.DeserializeObject<SystemScriptExecuteMessage>(messageBody));   
                case "sarah.integration.configure":
                    return Program.ConfigureIntegration(JsonConvert.DeserializeObject<IntegrationConfigurationMessage>(messageBody));
                case "system.triggers.change":
                    TriggersChecker.Reload();
                    return MessageResponse.OK;
                case "homeautomation.scenario.execute":
                    return Scenes.ScenesHelper.ExecuteScenario(origin, topic, messageBody);
                case "homeautomation.scenario.disable":
                    return Scenes.ScenesHelper.DeactivateScenario(origin, topic, messageBody);
                case "homeautomation.scenario.refreshdata":
                    Scenes.ScenesHelper.RefreshCache();
                    return MessageResponse.OK;
            }

            return MessageResponse.OK;
        }
    }
}
