using Home.Common;
using Home.Common.HomeAutomation;
using Home.Common.Messages;
using Home.Common.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Home.Agents.Sarah.Devices.Wled
{
    internal static class WledHelper
    {
        public static bool _stop = false;
        public static void Start()
        {
            var t = new Thread(() => WledHelper.Run());
            t.Name = "Wled devices";
            t.Start();
        }

        private static void Run()
        {
            while (!_stop)
            {
                try
                {
                    Thread.Sleep(10000);

                    Thread.Sleep(20000);
                }
                catch (Exception ex)
                {
                    Console.Write("WLed - ERR in WLedThread - ");
                    Console.WriteLine(ex.ToString());
                }
            }
        }

        public static void Stop()
        {
            _stop = true;
        }


        private static Dictionary<string, WledDevice> _allDevices = new Dictionary<string, WledDevice>();


        internal static DeviceBase[] GetDevices(NetworkDeviceEnumerateMessageResponse alldevs)
        {
            List<DeviceBase> ret = new List<DeviceBase>();

            foreach (var dev in alldevs.ActiveDevices)
            {
                if (dev.Vendor != null && dev.Vendor.Contains("Espressif", StringComparison.InvariantCultureIgnoreCase))
                {
                    //Console.WriteLine($"w-led - Found {dev.IpV4} as a potential w-led device");

                    var dvb = GetWledDevice(dev);
                    if (dvb != null)
                    {
                        //Console.WriteLine($"w-led - Adding {dev.IpV4} as w-led device : {dvb.DeviceName}");
                        ret.Add(dvb);
                    }
                }
            }


            var toRegister = new List<Device>();
            foreach (WledDevice r in ret)
            {
                WledDevice already = null;
                if (!_allDevices.TryGetValue(r.DeviceName, out already))
                {
                    _allDevices.Add(r.DeviceName, r);
                    var devreg = new Device(r);
                    devreg.DeviceRoles.Add("wled");
                    devreg.DeviceAddresses.Add(r.IpV4);
                    devreg.DeviceGivenName = r.DeviceName;
                    toRegister.Add(devreg);
                }
                else
                {
                    if (already.IpV4 != r.IpV4)
                    {
                        already.IpV4 = r.IpV4;
                    }
                }
            }

            Console.WriteLine($"w-led - Registering {toRegister.Count} w-led devices");

            if (toRegister != null && toRegister.Count > 0)
            {
                RegisterDevice(toRegister.ToArray());
            }

            var t = _allDevices.Values.ToArray();
            foreach (WledDevice r in t)
            {
                if (r != null && r.DeviceName != null)
                {
                    var detected = (from z in ret
                                    where z.DeviceName.Equals(r.DeviceName)
                                    select z).FirstOrDefault();
                    if (detected == null)
                    {
                        _allDevices.Remove(r.DeviceName);
                    }
                }
                else if (r.DeviceName == null)
                    _allDevices.Remove(r.DeviceName);
            }


            return ret.ToArray();
        }

        private static DeviceBase GetWledDevice(NetworkDeviceData dev)
        {
            string ipV4 = dev.IpV4;
            return GetWledDevice(ipV4);
        }

        internal static DeviceBase GetWledDevice(string ipV4)
        {
            var sdi = GetDeviceInfo(ipV4);

            if (sdi == null)
            {
                //Console.WriteLine($"Wled - {ipV4} is not a w-led device");
                return null;
            }
            return GetWledDevice(sdi, ipV4);
        }

        private static DeviceBase GetWledDevice(GetWledDeviceInfo sdi, string ipV4)
        {
            return new WledDevice(sdi, ipV4);
        }

        public class GetWledDeviceInfo
        {
            public string ver { get; set; }
            public int vid { get; set; }
            public GetWledDeviceInfoLeds leds { get; set; }
            public string name { get; set; }
            public int udpport { get; set; }
            public bool live { get; set; }
            public int fxcount { get; set; }
            public int palcount { get; set; }
            public string arch { get; set; }
            public string core { get; set; }
            public int freeheap { get; set; }
            public int uptime { get; set; }
            public int opt { get; set; }
            public string brand { get; set; }
            public string product { get; set; }
            public string btype { get; set; }
            public string mac { get; set; }
        }

        public class GetWledDeviceInfoLeds
        {
            public int count { get; set; }
            public bool rgbw { get; set; }
            public int[] pin { get; set; }
            public int pwr { get; set; }
            public int maxpwr { get; set; }
            public int maxseg { get; set; }
        }

        private static void RegisterDevice(params Device[] devices)
        {
            foreach (var dev in devices)
                dev.DevicePlatform = "wled";

            for (int i = 0; i < 3; i++)
            {
                try
                {
                    using (var cli = new MainApiAgentWebClient("sarah"))
                    {
                        var exts = cli.UploadData<List<Device>, Device[]>(
                            $"/v1.0/devices/register/sarah",
                            "POST", devices);
                        break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }

            }
        }

        internal static GetWledDeviceInfo GetDeviceInfo(string ipV4)
        {
            using (WebClient cli = new WebClient())
            {
                string url = $"http://{ipV4}/json/info";
                try
                {
                    string json = cli.DownloadString(url);
                    return JsonConvert.DeserializeObject<GetWledDeviceInfo>(json);
                }
                catch (WebException)
                {
                    return null;
                }
            }
        }

    }
}
