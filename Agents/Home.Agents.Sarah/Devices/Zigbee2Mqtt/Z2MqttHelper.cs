using Home.Agents.Sarah.Devices.Hue;
using Home.Common;
using Home.Graph.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Home.Agents.Sarah.Devices.Zigbee2Mqtt
{
    internal static partial class Z2MqttHelper
    {
        private static string _root = "zigbee2mqtt";

        private static Z2MqttBridge _bridge = null;
        private static Dictionary<string, Z2MqttDeviceBase> _allDevices = new Dictionary<string, Z2MqttDeviceBase>();


        public static bool _stop = false;
        public static void Start()
        {
            string pth = _root;
            if (!pth.EndsWith("/"))
                pth = pth + "/";
            pth = pth + "#";

            //var devices = 


            MqttHelper.AddChangeHandler(pth, HandleChange);
        }

        private static void HandleChange(string topic, string data)
        {
            string subPath = topic;
            if (subPath.StartsWith(_root))
                subPath = subPath.Substring(_root.Length);

            if(topic.Equals("zigbee2mqtt/bridge/info"))
            {
                
            }

            if (topic.Equals("zigbee2mqtt/bridge/devices"))
            {

            }

            Console.WriteLine($"Zigbee2MQTT - update of {subPath}");
        }
    }
}
