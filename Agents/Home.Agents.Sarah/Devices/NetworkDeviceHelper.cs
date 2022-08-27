using Home.Common;
using Home.Common.HomeAutomation;
using Home.Common.Messages;
using Home.Common.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Home.Agents.Sarah.Devices
{
    internal class NetworkDeviceHelper
    {
        private static SarahConfigurationData cfg;
        public static bool _stop = false;
        public static void Start(SarahConfigurationData data)
        {
            var t = new Thread(() => NetworkDeviceHelper.Run());
            t.Name = "Network devices";
            t.Start();
        }

        private static void Run()
        {
            while (!_stop)
            {
                try
                {
                    Thread.Sleep(10000);
                    var t = GetDevices();
                    if (t == null)
                        Console.WriteLine("NetworkDeviceHelper - ERR in NetworkDeviceThread - Device discovery failed");
                    Thread.Sleep(20000);
                }
                catch (Exception ex)
                {
                    Console.Write("NetworkDeviceHelper - ERR in NetworkDeviceThread - ");
                    Console.WriteLine(ex.ToString());
                }
            }
        }

        public static DeviceBase[] GetDevices()
        {
            var msg = new NetworkDeviceEnumerateMessage();
            Console.WriteLine("NetworkDeviceHelper - Requesting devices from network agent");
            var alldevs = NatsMessageThread.Request<NetworkDeviceEnumerateMessageResponse>(
                msg.Topic, msg);
            return GetDevices(alldevs);
        }

        private static DeviceBase[] GetDevices(NetworkDeviceEnumerateMessageResponse alldevs)
        {
            if (!alldevs.Response.Equals("ok", StringComparison.InvariantCultureIgnoreCase))
            {
                Console.WriteLine($"NetworkDeviceHelper - Received message {alldevs.Response} from network agent");
                return null;
            }

            List<DeviceBase> ret = new List<DeviceBase>();

            DeviceBase[] tmp = null;
            if (Program.SupportDeviceFamily("shelly", cfg))
            {
                tmp = Shelly.ShellyDeviceHelper.GetDevices(alldevs);
                if (tmp != null)
                    ret.AddRange(tmp);
            }

            if (Program.SupportDeviceFamily("wled", cfg))
            {
                tmp = Wled.WledHelper.GetDevices(alldevs);
                if (tmp != null)
                    ret.AddRange(tmp);
            }
            return ret.ToArray();
        }


        public static void Stop()
        {
            _stop = true;
        }




        
    }
}
