using System;
using System.Collections.Generic;
using System.Text;

namespace Home.Common.Messages
{
    public class NetworkSsidDetectedMessage : BaseMessage
    {
        public NetworkSsidDetectedMessage() : base("system.network.wifi.ssid-detected")
        {
        }

        public string SsidName { get; set; }
    }
}
