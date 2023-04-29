using AdaptiveCards;
using Home.Common;
using Home.Common.Messages;
using Home.Common.Model;
using Home.Graph.Common;
using Microsoft.Azure.Amqp.Framing;
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


        internal static AdaptiveCard GetConfigCard(IntegrationConfigurationMessage source)
        {
            AdaptiveCard c = new AdaptiveCard(new AdaptiveSchemaVersion(1, 4));

            c.Body.Add(new AdaptiveTextBlock()
            {
                Size = AdaptiveTextSize.Medium,
                Weight = AdaptiveTextWeight.Bolder,
                Text = "${label}"
            });

            c.Body.Add(new AdaptiveTextBlock()
            {
                Text = "Get weather data info from online service and publish it to entities and location data",
                Wrap = true
            });

            c.Body.Add(new AdaptiveTextBlock()
            {
                Text = "[more info](https://manoir.app/integrations/erza.weather.html)",
                HorizontalAlignment = AdaptiveHorizontalAlignment.Right,
                IsSubtle = true
            });

            c.Body.Add(new AdaptiveChoiceSetInput()
            {
                Choices = new List<AdaptiveChoice>()
                {
                    new AdaptiveChoice()
                    {
                        Title = "met.no (Norway Meteorologisk institutt)",
                        Value = "met.no"
                    }
                },
                Label = "Choose your weather provider",
                Style = AdaptiveChoiceInputStyle.Expanded,
                Id = "settings_provider",
                IsRequired = true,
                ErrorMessage  = "Please select a valid weather provider",
                Spacing = AdaptiveSpacing.Large,
                Separator = true,
                Value = "${settings.provider}"
            });

            c.Actions.Add(new AdaptiveSubmitAction()
            {
                Title = "Save"
            });

            return c;
        }


        internal static MessageResponse HandleConfigurationMessage(IntegrationConfigurationMessage source)
        {
            if (source.SetupValues != null && source.SetupValues.Count > 0)
            {
                foreach (var cfg in source.SetupValues)
                {
                    source.Instance.Settings[cfg.Key] = cfg.Value;
                }
            }

            var tmp = new IntegrationConfigurationResponse(source)
            {
                ConfigurationCardFormat = "adaptivecard+json",
                ConfigurationCard = GetConfigCard(source).ToJson(),
                IsFinalStep = true
            };

            new Thread(() =>
            {
                Thread.Sleep(5000);
                RefreshIntegrationData();
            }).Start();

            return tmp;
        }
    }
}
