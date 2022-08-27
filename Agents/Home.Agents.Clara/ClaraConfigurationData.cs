using System;
using System.Collections.Generic;
using System.Text;

namespace Home.Agents.Clara
{
    public class ClaraConfigurationData
    {
        public ClaraConfigurationData()
        {
            NewsSources = new List<ClaraNewsSource>();
            TorrentSources = new List<RssTorrentSource>();
            HomeServices = new Dictionary<string, HomeServicesConfig>();
        }

        public List<ClaraNewsSource> NewsSources { get; set; }

        public List<RssTorrentSource> TorrentSources { get; set; }

        public Dictionary<string, HomeServicesConfig> HomeServices { get; set; }
    }

    public class HomeServicesConfig
    {
        public string UserId { get; set; }
        public string ListId { get; set; }
        public string ServiceUsername { get; set; }
        public string ServicePassword { get; set; }
        public string ScenarioOnStart { get; set; }
        public string ScenarioOnEnd { get; set; }
        public string[] ExternalUserIds { get; set; }

    }


    public class RssTorrentSource
    {
        public string Label { get; set; }

        public string[] Tags { get; set; }
        public string Url { get; set; }
        public bool IsPrivate { get; set; } 
    }


    public class ClaraNewsSource
    {
        public string UserId { get; set; }
        public string Source { get; set; }
        public string SourceUri { get; set; }
        public DateTimeOffset LastChecked { get; set; }

        public string BuckedId { get; set; }
    }
}
