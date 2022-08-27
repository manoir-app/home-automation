using Home.Common;
using Home.Common.Model;
using Home.Graph.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Home.Agents.Sarah
{
    internal static class SarahIntegrationsProvider 
    {
        private static readonly Integration[] _allIntegrations =
        {
            new Integration() {
                AgentId = "sarah",
                CanInstallMultipleTimes = false,
                Category = "home-automation",
                Description = "Integrates Philips Hue",
                Id = "sarah.philips.hue",
                Label = "Philips Hue",
                Image = "https://www.manoir.app/integrations/sarah.philips.hue.png"
            },
            new Integration() {
                AgentId = "sarah",
                CanInstallMultipleTimes = false,
                Category = "home-automation",
                Description = "Integrates Shelly devices (without Cloud)",
                Id = "sarah.shelly.local",
                Label = "Shelly (local)",
                Image = "https://www.manoir.app/integrations/sarah.shelly.local.png"
            },
            new Integration() {
                AgentId = "sarah",
                CanInstallMultipleTimes = false,
                Category = "home-automation",
                Description = "Integrates with WLED devices",
                Id = "sarah.wled",
                Label = "Wled",
                Image = "https://www.manoir.app/integrations/sarah.wled.png"
            }
        };

        static List<Integration> _activeIntegrations = null;

        public static void InitIntegrations()
        {
            IntegrationProviderHelper.InitIntegrations(_allIntegrations, "sarah", out _activeIntegrations);
        }

        public static bool IsIntegrationActive(string integrationId, List<Integration> activeIntegrations)
        {
            return IntegrationProviderHelper.IsIntegrationActive(integrationId, _activeIntegrations);
        }

        public static List<IntegrationInstance> GetInstances(string integrationId, List<Integration> activeIntegrations)
        {
            return IntegrationProviderHelper.GetInstances(integrationId, _activeIntegrations);
        }

    }
}
