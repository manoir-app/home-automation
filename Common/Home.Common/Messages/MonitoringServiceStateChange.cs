using Home.Common.Messages;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Text;

namespace Home.Common.Messages
{
    public enum MonitoringServiceState 
    {
        Started,
        Failed,
        Stopped
    }

    public class MonitoringServiceStateChange : BaseMessage
    {
        public MonitoringServiceStateChange() : base("monitoring.services.statechanged")
        {
        }

        public string ServiceName { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public MonitoringServiceState NewStatus { get; set; }

        public DateTimeOffset ChangeDate { get; set; }
    }
}
