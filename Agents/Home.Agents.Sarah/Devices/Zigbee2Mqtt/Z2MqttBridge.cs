using Home.Common.HomeAutomation;
using Home.Common.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Home.Agents.Sarah.Devices.Zigbee2Mqtt
{
    internal class Z2MqttBridge : IHubDevice
    {
            

        public IEnumerable<Device> GetDevices()
        {
            return null;
        }
    }
}
