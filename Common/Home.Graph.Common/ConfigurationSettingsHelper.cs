using Microsoft.Extensions.Configuration;
using System;

namespace Home.Common
{
    public static class ConfigurationSettingsHelper
    {
        private static object _lockObject = new object();

        private static IConfigurationRoot _config = null;

        private static string _cnString = null;

        public static void Init(string cnString)
        {
            _cnString = cnString;
            var cfg = GetConfig();
        }

        private static IConfigurationRoot GetConfig()
        {
            lock (_lockObject)
            {
                if (_config == null)
                {
                    //CPointSoftware.Tools.Configuration.ConfigurationSettings.FileRequest += new CPointSoftware.Tools.Configuration.ConfigurationFileNeededEventHandler(ConfigurationSettings_FileRequest);
                    var builder = new ConfigurationBuilder();
                    // cette clef est en lecture seule, ca ne devrait donc pas poser de soucis
                    builder.AddAzureAppConfiguration(
                        options =>
                        {
                            options.Connect(_cnString);                           
                        });

                    var config = builder.Build();
                    _config = config;
                }
            }

            return _config;
        }

        public static string GetAzureAdClientId()
        {
            var cfg = GetConfig();
            return cfg["AzureAD:ClientId"];
        }

        public static string GetAzureAdClientSecret()
        {
            var cfg = GetConfig();
            return cfg["AzureAD:ClientSecret"];
        }

        public static string GetAzureStorageConnectionString()
        {
            var cfg = GetConfig();
            return cfg["HomeAutomation:StorageConnectionString"];
        }

        public static string GetYoutubeApiKey()
        {
            var cfg = GetConfig();
            return cfg["HomeAutomation:YoutubeApiKey"];
        }

        public static string GetServiceBusConnectionString()
        {
            var cfg = GetConfig();
            return cfg["HomeAutomation:ServiceBusConnection"];
        }

        public static string GetAzureSpeechKey()
        {
            var cfg = GetConfig();
            return cfg["HomeAutomation:AzureSpeechKey"];
        }

        public static string GetAzureSpeechRegion()
        {
            var cfg = GetConfig();
            return cfg["HomeAutomation:AzureSpeechRegion"];
        }

        public static string GetAzureNotificationHub()
        {
            var cfg = GetConfig();
            return cfg["HomeAutomation:NotificationHub"];
        }

        public static string GetTwitchClientId()
        {
            var cfg = GetConfig();
            return cfg["Twitch:ClientId"];
        }

        public static string GetTwitchClientSecret()
        {
            var cfg = GetConfig();
            return cfg["Twitch:ClientSecret"];
        }

        public static string GetSpotifyClientId()
        {
            var cfg = GetConfig();
            return cfg["Spotify:ClientId"];
        }

        public static string GetSpotifyClientSecret()
        {
            var cfg = GetConfig();
            return cfg["Spotify:ClientSecret"];
        }



        public static string GetSonosClientId()
        {
            var cfg = GetConfig();
            return cfg["Sonos:ClientId"];
        }

        public static string GetSonosClientSecret()
        {
            var cfg = GetConfig();
            return cfg["Sonos:ClientSecret"];
        }


        public const string OvhAppKey = "Ovh:ApplicationKey";
        public const string OvhAppSecret = "Ovh:ApplicationSecret";
        public const string OvhConsumerSecret = "Ovh:ConsumerSecret";

        public static string Get(string settingsName)
        {
            var cfg = GetConfig();
            return cfg[settingsName];
        }
    }
}
