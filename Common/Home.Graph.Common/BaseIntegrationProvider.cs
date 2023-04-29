using AdaptiveCards;
using Home.Common;
using Home.Common.Messages;
using Home.Common.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Home.Graph.Common
{
    public static class IntegrationProviderHelper
    {
        public static bool IsIntegrationActive(string integrationId, List<Integration> activeIntegrations)
        {
            var i = (from z in activeIntegrations
                     where z.Id.Equals(integrationId, StringComparison.InvariantCultureIgnoreCase)
                     select z).Count();
            return i > 0;
        }

        public static List<IntegrationInstance> GetInstances(string integrationId, List<Integration> activeIntegrations)
        {
            var i = (from z in activeIntegrations
                     where z.Id.Equals(integrationId, StringComparison.InvariantCultureIgnoreCase)
                     select z.Instances).FirstOrDefault();
            return i;
        }

        public static IntegrationConfigurationResponse GetDefaultResponse(IntegrationConfigurationMessage source, 
            string title, string description, Dictionary<string, string> properties, bool finalStep = true)
        {
            if (source.SetupValues != null && source.SetupValues.Count > 0)
            {
                foreach (var cfg in source.SetupValues)
                {
                    source.Instance.Settings[cfg.Key] = cfg.Value;
                }
            }

            AdaptiveCard c = new AdaptiveCard(new AdaptiveSchemaVersion(1, 4));

            c.Body.Add(new AdaptiveTextBlock()
            {
                Size = AdaptiveTextSize.Medium,
                Weight = AdaptiveTextWeight.Bolder,
                Text = "${label}"
            });
            foreach (var prop in properties)
            {
                c.Body.Add(new AdaptiveTextInput()
                {
                    Label = prop.Value,
                    IsRequired = true,
                    Id = "settings_" +prop.Key,
                    Value = "${settings." + prop.Key + "}"
                });
            }
            c.Actions.Add(new AdaptiveSubmitAction()
            {
                Title = "Save"
            });
            return new IntegrationConfigurationResponse(source)
            {
                ConfigurationCardFormat = "adaptivecard+json",
                ConfigurationCard = c.ToJson(),
                IsFinalStep = finalStep
            };
        }


        public static void InitIntegrations(IEnumerable<Integration> integrations, string agentId, out List<Integration> activeIntegrations)
        {
            activeIntegrations = new List<Integration>();   
            try
            {
                foreach (var t in integrations)
                {
                    t.AgentId = agentId;
                    AgentHelper.PushIntegration(agentId, t);
                }

                activeIntegrations = AgentHelper.GetMyIntegrations(agentId, true);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

    }
}
