using Home.Common.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace Home.Common.Messages
{
    public class DeviceDiscoveredMessage : BaseMessage
    {
        public DeviceDiscoveredMessage(string messageTopic) : base(messageTopic)
        {
        }

        public DiscoveredDevice Device { get; set; }
        public DateTimeOffset DiscoveryTime { get; set; }
    }
}
