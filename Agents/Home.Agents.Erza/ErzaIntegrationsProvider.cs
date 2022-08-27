using Home.Common.Model;
using Home.Graph.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Home.Agents.Erza
{
    internal static class ErzaIntegrationsProvider
    {
        private static readonly Integration[] _allIntegrations =
        {
            new Integration() {
                AgentId = "erza",
                CanInstallMultipleTimes = false,
                Category = "weather",
                Description = "Gets weather info",
                Id = "erza.weather",
                Label = "Weather & Forecast",
                Image = "https://www.manoir.app/integrations/erza.weather.png"
            },
            new Integration() {
                AgentId = "erza",
                CanInstallMultipleTimes = false,
                Category = "development",
                Description = "Synchronise configuration and automations with a GIT source repository",
                Id = "erza.sourcecode.git",
                Label = "Sync config with GIT",
                Image = "https://www.manoir.app/integrations/erza.sourcecode.git.png"
            },
        };

        static List<Integration> _activeIntegrations = null;

        public static void InitIntegrations()
        {
            IntegrationProviderHelper.InitIntegrations(_allIntegrations, "erza", out _activeIntegrations);
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
