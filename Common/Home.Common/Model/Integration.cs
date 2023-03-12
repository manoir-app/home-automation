using System;
using System.Collections.Generic;
using System.Text;

namespace Home.Common.Model
{
    public class Integration
    {
        public const string CategoryHomeAutomation = "home-automation";
        public const string CategoryPim = "personnal-info";

        public const string ComponentRoutine = "routine";
        public const string ComponentDevice = "device";
        public const string ComponentDataCollector = "data-collector";
        public const string ComponentCalendarProvider = "calendar-provider";

        public Integration()
        {
            Instances = new List<IntegrationInstance>();
            Components = new List<string>();
        }

        public string Id { get; set; }
        public string AgentId { get; set; }

        public bool Hidden { get; set; }


        public string Label { get; set; }
        public string Image { get; set; }
        public string Description { get; set; }

        public string Category { get; set; }

        public List<string> Components { get; set; }

        public List<IntegrationInstance> Instances { get; set; }

        public bool CanInstallMultipleTimes { get; set; }
    }

    public class IntegrationInstance
    {
        public string Id { get; set; }
        public string Label { get; set; }
        public bool IsSetup { get; set; }

        public Dictionary<string, string> Settings { get; set; }
    }

}
