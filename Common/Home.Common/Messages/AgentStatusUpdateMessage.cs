using Home.Common.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace Home.Common.Messages
{
    public class AgentStatusUpdateMessage : BaseMessage
    {
        public const string TopicName = "system.agents.statusupdate";

        public AgentStatusUpdateMessage(string agentName) : base(TopicName+$".{agentName}")
        {
            AgentName = agentName;
        }

        public AgentStatus NewStatus { get; set; }
        public string AgentName { get; set; }
    }
}
