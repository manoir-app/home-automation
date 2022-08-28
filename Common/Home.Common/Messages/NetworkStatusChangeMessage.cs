using Home.Common.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace Home.Common.Messages
{
    public class NetworkStatusChangeMessage : BaseMessage
    {
        public NetworkStatusChangeMessage() : base("system.network.status.change")
        {
        }

        public string NetworkId { get; set; }
        public ConnectionStatus NewStatus { get; set; }
    }
}
