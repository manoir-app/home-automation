using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace Home.Common.Model
{
    public class Agent
    {
        public string Id { get; set; }
        public string AgentName { get; set; }
        public string AgentMesh { get; set; }
        public string AgentMachineName { get; set; }

        public string[] Roles { get; set; }
        public DateTimeOffset LastPing { get; set; }

        public AgentStatus CurrentStatus { get; set; }

        public string ConfigurationData { get; set; }

        public string ExtensionId { get; set; }
    }


    [JsonConverter(typeof(StringEnumConverter))]
    public enum AgentStatusKind
    {
        Info,
        Question,
        Error,
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum AgentStatusFormat
    {
        String,
        AdaptiveCard
    }

    public class AgentStatus
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public AgentStatusKind Kind { get; set; }

        public DateTimeOffset MessageDate { get; set; }
        public string Message { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public AgentStatusFormat Format { get; set; }

        public ElementCommand[] Options { get; set; }
    }



    public class AgentRegistration
    {
        public string AgentName { get; set; }
        public string AgentMachineName { get; set; }
        public string[] Roles { get; set; }

    }

}
