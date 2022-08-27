using Home.Common;
using Home.Common.Messages;
using Home.Common.Model;
using Home.Graph.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Home.Agents.Sarah.Scenes
{
    partial class ScenesHelper
    {
        internal static MessageResponse DeactivateScenario(MessageOrigin origin, string topic, string messageBody)
        {
            var msg = JsonConvert.DeserializeObject<ExecuteScenarioHomeAutomationMessage>(messageBody);
            if (msg == null)
                return MessageResponse.GenericFail;

            Console.WriteLine($"Scenes - disable : " + msg.SceneId);
            // on trouve la scene
            var scene = (from z in _Scenes
                         where z.Id.Equals(msg.SceneId)
                         select z).FirstOrDefault();
            if (scene == null)
            {
                RefreshCache();
                scene = (from z in _Scenes
                         where z.Id.Equals(msg.SceneId)
                         select z).FirstOrDefault();
                if (scene == null)
                    return MessageResponse.GenericFail;
            }

            Console.WriteLine($"Scenes - {msg.SceneId} scene found");

            // on va chercher sur le serveur l'état actuel du groupe
            // parce que le cache peut avoir 2 minutes
            var group = GetGroupFromServer(scene.GroupId);
            if (group == null)
                return MessageResponse.GenericFail;

            DesactiverScene(scene, group);

            AgentHelper.UpdateStatusWithInfo("sarah", "Deactivated scene : " + scene.Label);
            LogHelper.Log("agent", "sarah", "Deactivated scene : " + scene.Label);

            return MessageResponse.OK;

        }

        internal static MessageResponse ExecuteScenario(MessageOrigin origin, string topic, string messageBody)
        {
            var msg = JsonConvert.DeserializeObject<ExecuteScenarioHomeAutomationMessage>(messageBody);
            if (msg == null)
                return MessageResponse.GenericFail;

            Console.WriteLine($"Scenes - activate : " + msg.SceneId);

            // on trouve la scene
            var scene = (from z in _Scenes
                         where z.Id.Equals(msg.SceneId)
                         select z).FirstOrDefault();
            if (scene == null)
            {
                RefreshCache();
                scene = (from z in _Scenes
                         where z.Id.Equals(msg.SceneId)
                         select z).FirstOrDefault();
                if (scene == null)
                    return MessageResponse.GenericFail;
            }

            Console.WriteLine($"Scenes - {msg.SceneId} scene found");

            // on va chercher sur le serveur l'état actuel du groupe
            // parce que le cache peut avoir 2 minutes
            var group = GetGroupFromServer(scene.GroupId);
            if (group == null)
                return MessageResponse.GenericFail;
            // on désactive si exclusif et quelque chose d'actif

            if (group.SceneIsExclusive && group.CurrentActiveScenes != null
                && group.CurrentActiveScenes.Count > 0)
            {
                Console.WriteLine($"Scenes - {msg.SceneId} will deactivate {string.Join(",", group.CurrentActiveScenes)}");

                foreach (var adesactiverId in group.CurrentActiveScenes)
                {
                    var sceneADesactiver = (from z in _Scenes
                                            where z.Id.Equals(adesactiverId)
                                            select z).FirstOrDefault();
                    if (sceneADesactiver != null 
                        && !sceneADesactiver.Id.Equals(scene.Id))
                    {
                        DesactiverScene(sceneADesactiver, group);
                    }
                }
            }

            // on va chercher sur le serveur l'état actuel du groupe
            // parce que le cache peut avoir 2 minutes
            group = GetGroupFromServer(scene.GroupId);
            if (group == null)
                return MessageResponse.GenericFail;

            List<string> resultScenes = new List<string>();

            // on ajoute le nouveau si non exclusif ou
            // on remplace si exclusif
            if (!group.SceneIsExclusive && (group.CurrentActiveScenes != null
                && group.CurrentActiveScenes.Count > 0))
            {
                resultScenes.AddRange(group.CurrentActiveScenes);
            }
            resultScenes.Add(scene.Id);
            ActiverScene(scene, group);
            SetGroupActiveScenes(group.Id, resultScenes.ToArray());

            AgentHelper.UpdateStatusWithInfo("sarah", "Executed scene : " + scene.Label);
            LogHelper.Log("agent", "sarah", "Executed scene : " + scene.Label);

            return MessageResponse.OK;
        }

        private static void ActiverScene(Scene scene, SceneGroup group)
        {
            Console.WriteLine($"Scenes - activating {scene.Id}");

            foreach (var step in scene.ActivationSteps)
            {
                ExecuterEtapeScene(scene, step);
            }
        }

        private static void DesactiverScene(Scene sceneADesactiver, SceneGroup group)
        {
            Console.WriteLine($"Scenes - deactivating {sceneADesactiver.Id}");

            foreach (var step in sceneADesactiver.DeactivationSteps)
            {
                ExecuterEtapeScene(sceneADesactiver, step);
            }

        }

        private class SimpleMessageForParsing : BaseMessage
        {
            public SimpleMessageForParsing() : base("")
            {
            }
        }

        private static void ExecuterEtapeScene(Scene scene, SceneStep step)
        {
            switch (step.TargetKind)
            {
                case SceneStepTargetKind.Agent:
                    Console.WriteLine($"Scenes - execute step for {scene.Id} : {step.Message}");

                    if (string.IsNullOrEmpty(step.MessageBody))
                    {
                        Console.WriteLine($"Scenes - execute step for {scene.Id} : {step.Message}, no message body");
                        break;
                    }
                    else
                    {
                        Console.WriteLine($"Scenes - execute step for {scene.Id} : {step.Message}, message body = { step.MessageBody}");
                    }
                    try
                    {
                        var msg = JsonConvert.DeserializeObject<SimpleMessageForParsing>(step.MessageBody);
                        bool done = false;
                        foreach (var topic in DeviceManager.AllTopics)
                        {
                            if (topic.EndsWith(".>"))
                            {
                                Console.WriteLine($"Scenes try matching : {step.Message} to {topic.Substring(0, topic.Length - 2)}");

                                if (step.Message.StartsWith(topic.Substring(0, topic.Length - 2), StringComparison.InvariantCultureIgnoreCase))
                                {
                                    DeviceManager.HandleMessage(MessageOrigin.Local, step.Message, step.MessageBody);
                                    done = true;
                                    break;
                                }
                            }
                            else if (step.Message.Equals(topic, StringComparison.InvariantCultureIgnoreCase))
                            {
                                DeviceManager.HandleMessage(MessageOrigin.Local, step.Message, step.MessageBody);
                                done = true;
                                break;
                            }
                        }
                        foreach (var topic in SarahMessageHandler.AllTopics)
                        {
                            if (topic.EndsWith(".>"))
                            {
                                Console.WriteLine($"Scenes try matching : {step.Message} to {topic.Substring(0, topic.Length - 2)}");

                                if (step.Message.StartsWith(topic.Substring(0, topic.Length - 2), StringComparison.InvariantCultureIgnoreCase))
                                {
                                    SarahMessageHandler.HandleMessage(MessageOrigin.Local, step.Message, step.MessageBody);
                                    done = true;
                                    break;
                                }
                            }
                            else if (step.Message.Equals(topic, StringComparison.InvariantCultureIgnoreCase))
                            {
                                SarahMessageHandler.HandleMessage(MessageOrigin.Local, step.Message, step.MessageBody);
                                done = true;
                                break;
                            }
                        }

                        if (!done)
                            NatsMessageThread.Push(step.Message, step.MessageBody);
                        else
                            Console.WriteLine($"Scenes - execute step for {scene.Id} : {step.Message} was done on this agent (sarah)");
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine($"Scenes - execute step error {ex.ToString()})");
                    }
                    break;
            }
        }

        private static SceneGroup GetGroupFromServer(string groupId)
        {
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    using (var cli = new MainApiAgentWebClient("sarah"))
                    {
                        var exts = cli.DownloadData<SceneGroup>(
                            $"/v1.0/homeautomation/scenes/groups/{groupId}");
                        return exts;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }

            }

            return null;
        }

        private static SceneGroup SetGroupActiveScenes(string groupId, string[] scenes)
        {
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    using (var cli = new MainApiAgentWebClient("sarah"))
                    {
                        var exts = cli.UploadData<SceneGroup, string[]>(
                            $"/v1.0/homeautomation/scenes/groups/{groupId}/activeScenes",
                            "POST", scenes);
                        return exts;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }

            }

            return null;
        }

        private static SceneGroup RemoveGroupActiveScenes(string groupId, string scene)
        {
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    using (var cli = new MainApiAgentWebClient("sarah"))
                    {
                        var exts = cli.UploadData<SceneGroup, string>(
                            $"/v1.0/homeautomation/scenes/groups/{groupId}/activeScenes?scene=" + scene,
                            "DELETE", "");
                        return exts;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }

            }

            return null;
        }
    }
}
