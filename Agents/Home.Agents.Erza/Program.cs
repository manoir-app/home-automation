using Home.Agents.Erza.SourceIntegration;
using Home.Common;
using Home.Common.Messages;
using Home.Common.Model;
using Home.Graph.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Home.Agents.Erza
{
    class Program
    {
        public static bool _stop = false;

        internal static MessageResponse ConfigureIntegration(IntegrationConfigurationMessage integrationConfigurationMessage)
        {
            if (integrationConfigurationMessage == null
                || integrationConfigurationMessage.Integration == null
                || integrationConfigurationMessage.Instance == null)
            {
                return MessageResponse.GenericFail;
            }

            switch (integrationConfigurationMessage.Integration.Id.ToLowerInvariant())
            {
                case "erza.weather":
                    return WeatherProviders.WeatherIntegration.HandleConfigurationMessage(integrationConfigurationMessage);
            }

            return MessageResponse.GenericFail;
        }

        static void Main(string[] args)
        {
            AgentHelper.WriteStartupMessage("Erza", typeof(Program).Assembly);

            DatabaseMaintenanceThread.CheckDb();

#if DEBUG
            GitSync.SyncPagesFromServer();
            GitSync.SyncPagesFromSources();
            GitSync.SyncPagesFromServer();
#endif
            AgentHelper.SetupReporting("erza");
            AgentHelper.SetupLocaleFromServer("erza");
            AgentHelper.ReportStart("erza", "monitoring", "security");

            ErzaIntegrationsProvider.InitIntegrations();

            ErzaMessageHandler.Start();
            MqttHelper.Start("agents-erza");
            WeatherProviders.WeatherIntegration.Start();
            NetworkChecker.Start();
            DatabaseMaintenanceThread.Start();
            SystemCleanup.Start();
            PresenceHelper.Start();
            UserHelper.Start();

            while (!_stop)
            {
                Thread.Sleep(500);
                AgentHelper.Ping("erza");
            }

            UserHelper.Stop();
            PresenceHelper.Stop();
            SystemCleanup.Stop();
            ErzaMessageHandler.Stop();
            NetworkChecker.Stop();
            WeatherProviders.WeatherIntegration.Stop();
            DatabaseMaintenanceThread.Stop();
            MqttHelper.Stop();
        }


        

       
    }
}
