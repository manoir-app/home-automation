using System;
using System.Collections.Generic;
using System.Text;

namespace Home.Common.Messages
{
    public class SystemGetContainersMessage : BaseMessage
    {
        public const string TopicName = "gaia.getcontainers";

        public SystemGetContainersMessage(string messageTopic) : base(messageTopic)
        {
        }

    }

    public class SystemGetContainersResponse
    {
        
    }

    public class SystemGetContainersResponseItem
    {
        public string Name { get; set; }
        
        public DateTimeOffset LastUpdate { get; set; }
    }
}
