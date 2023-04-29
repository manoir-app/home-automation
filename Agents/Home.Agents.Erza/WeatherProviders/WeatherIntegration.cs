using Home.Common;
using Home.Common.Messages;
using Home.Common.Model;
using Home.Graph.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Home.Agents.Erza.WeatherProviders
{
    internal static class WeatherIntegration
    {
        public static void Start()
        {
            var t = new Thread(() => WeatherIntegration.RunCheck());
            t.Name = "Weather Checker";
            t.Start();
        }

        public static void Stop()
        {
            _stop = true;
        }


        private static string _weatherProvider = "met.no";
        private static string _hazardProvider = "meteofrance";

        public static bool _isSetup = false;
        public static bool _stop = false;
        public static DateTimeOffset? _offlineDate = null;

        private static Location _loc = null;
        public static void Reload()
        {
            _loc = AgentHelper.GetLocalMeshLocation("erza");
            RefreshIntegrationData();
        }

        private static void RunCheck()
        {
            while (!_stop)
            {
                try
                {
                    DoCheck();
                }
                catch (Exception ex)
                {

                }

                Thread.Sleep(900000);
            }
        }

        public static void RefreshIntegrationData()
        {
            var integ = AgentHelper.GetIntegration("erza", "erza.weather");
            if (integ == null)
            {
                _isSetup = false;
            }
            else
            {
                var cfg = integ.Instances.FirstOrDefault();
                if (cfg == null)
                    _isSetup = false;
                else
                {
                    _isSetup = cfg.IsSetup;
                    _weatherProvider = (from z in cfg.Settings where z.Key.Equals("provider") select z.Value).FirstOrDefault();
                    if (_weatherProvider == null)
                        _weatherProvider = "met.no";

                    _hazardProvider = (from z in cfg.Settings where z.Key.Equals("hazardProvider") select z.Value).FirstOrDefault();
                    if (_hazardProvider == null)
                        _hazardProvider = "meteofrance";
                }
            }
        }

        internal static void DoCheck()
        {
            if (_loc == null)
                Reload();

            if (!_isSetup)
                return;

            if (_loc != null)
            {
                if (_hazardProvider == null)
                    _hazardProvider = "meteofrance";
                switch (_hazardProvider)
                {
                    default:
                        new WeatherProviders.MeteoFranceWeatherHazards().Run(Graph.Common.TaskContextKind.Agent, "erza", _loc);
                        break;
                }

                if (_weatherProvider == null)
                    _weatherProvider = "met.no";
                switch (_weatherProvider)
                {
                    default:
                        new WeatherProviders.MetNoWeather().Run(Graph.Common.TaskContextKind.Agent, "erza", _loc);
                        break;
                }
            }
        }



        internal static MessageResponse HandleConfigurationMessage(IntegrationConfigurationMessage integrationConfigurationMessage)
        {
            RefreshIntegrationData();
            return IntegrationProviderHelper.GetDefaultResponse(integrationConfigurationMessage, "Weather info",
                "Provides weather info", new Dictionary<string, string>()
                {
                    { "provider" , "Main weather provider" }
                });
        }
    }
}
