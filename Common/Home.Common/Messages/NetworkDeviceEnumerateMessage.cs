using System;
using System.Collections.Generic;
using System.Text;

namespace Home.Common.Messages
{
    public class NetworkDeviceEnumerateMessage : BaseMessage
    {
        
        public NetworkDeviceEnumerateMessage() : base("system.network.devices.enumerate")
        {
        }


    }

    public class NetworkDeviceEnumerateMessageResponse : MessageResponse
    {
        public NetworkDeviceEnumerateMessageResponse()
        {
            ActiveDevices = new List<NetworkDeviceData>();
            InactiveDevices = new List<NetworkDeviceData>();
        }

        public string Network { get; set; }
        public string Agent { get; set; }

        public List<NetworkDeviceData> ActiveDevices { get; set; }
        public List<NetworkDeviceData> InactiveDevices { get; set; }
    }

    public class NetworkDeviceData
    {
        public string Id { get; set; }
        public string IpV4 { get; set; }
        public string IpV6 { get; set; }
        public string Name { get; set; }
        public string Vendor { get; set; }
        public string Model { get; set; }
        public string MacAddress { get; set; }
    }
}
