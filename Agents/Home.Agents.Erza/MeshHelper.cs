using Home.Common;
using Home.Common.Messages;
using Home.Common.Model;
using Home.Graph.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;

namespace Home.Agents.Erza
{
    internal static class MeshHelper
    {
        internal static MessageResponse GetMeshInfos()
        {
            var ret = new MeshInfoMessageResponse();

            ret.SupportedLanguages.Add("fr-fr");
            foreach (var tz in TimeZoneInfo.GetSystemTimeZones())
            {
                ret.AvailableTimeZones.Add(tz.Id, tz.DisplayName);
            }
            ret.KubernetesNamespace = "default";
            return ret;
        }

        internal static MessageResponse HandleMessage(MessageOrigin origin, string topic, string messageBody)
        {
            switch (topic.ToLowerInvariant())
            {
                case "erza.getmeshinfos":
                    return MeshHelper.GetMeshInfos();
                case "system.mesh.globalscenario.changed":
                    return MessageResponse.OK;
                case "system.mesh.globalscenario.set":
                    return SetGlobalScenario(JsonConvert.DeserializeObject<MeshScenarioMessage>(messageBody));
                default:
                    return MessageResponse.GenericFail;
            }
        }

        private static MessageResponse SetGlobalScenario(MeshScenarioMessage meshScenarioMessage)
        {
            if (meshScenarioMessage == null)
                return MessageResponse.GenericFail;

            for (int i = 0; i < 3; i++)
            {
                try
                {
                    using (var cli = new MainApiAgentWebClient("aurore"))
                    {
                        AutomationMesh t;
                        if (string.IsNullOrEmpty(meshScenarioMessage.Scenario))
                        {
                            t = cli.UploadData<AutomationMesh, string>($"v1.0/system/mesh/local/globalscenario/current", "DELETE", "");
                        }
                        else
                        {
                            t = cli.UploadData<AutomationMesh, string>($"v1.0/system/mesh/local/globalscenario/current", "POST", meshScenarioMessage.Scenario);
                        }

                        return MessageResponse.OK;
                    }
                }
                catch (WebException ex)
                {
                    Thread.Sleep(1000);
                }
            }

            return MessageResponse.GenericFail;
        }
    }
}
