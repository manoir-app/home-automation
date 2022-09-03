using Home.Common;
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
