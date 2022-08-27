using Home.Common;
using Home.Common.Messages;
using Home.Common.Model;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using Home.Common.HomeAutomation;

namespace Home.Agents.Sarah.Devices.Hue
{
    partial class HueHelper
    {
        public static Device GetHueBridge()
        {
#if DEBUG
            var dev = new Device()
            {
                DeviceInternalName = "Bridge Hue"
            };
            dev.DeviceAddresses.Add("192.168.2.7");
            dev.DeviceKind = Device.DeviceKindHomeAutomation;
            dev.DeviceRoles.Add("bridge");
            dev.DeviceRoles.Add("hue-bridge");
            return dev;
#endif

            if (_bridge != null)
                return _bridge;

            for (int i = 0; i < 3; i++)
            {
                using (var cli = new MainApiAgentWebClient("sarah"))
                {
                    try
                    {

                        var exts = cli.DownloadData<List<Device>>(
                            $"/v1.0/devices/find?kind={Device.DeviceKindHomeAutomation}&role=hue-bridge");
                        if (exts != null && exts.Count >= 1)
                        {
                            _bridge = exts[0];
                            Console.WriteLine("Hue - found bridge at : " + _bridge.DeviceAddresses.First());
                        }
                        else
                        {
                            Console.WriteLine("Hue - found no bridge on server");
                            FindAndRegisterBridge(cli);
                            return _bridge;
                        }
                        break;
                    }
                    catch (Exception ex)
                    {
                        Console.Write("Hue - ");
                        Console.WriteLine(ex);
                        FindAndRegisterBridge(cli);
                        return _bridge;
                    }
                }

            }

            return null;

        }
        private static void FindAndRegisterBridge(MainApiAgentWebClient cli)
        {
            Console.WriteLine("Hue - Requesting devices from network agent");

            var msg = new NetworkDeviceEnumerateMessage();
            var alldevs = NatsMessageThread.Request<NetworkDeviceEnumerateMessageResponse>(
                msg.Topic, msg);

            Console.WriteLine($"Hue - Received {alldevs.ActiveDevices.Count} devices from network agent");

            foreach (var dev in alldevs.ActiveDevices)
            {
                if (dev.Vendor != null && dev.Vendor.Contains("philips lighting", StringComparison.InvariantCultureIgnoreCase))
                {
                    if (string.IsNullOrWhiteSpace(dev.IpV4))
                        continue;

                    if (TestIfHueBridge(dev))
                    {
                        var device = new Device()
                        {
                            DeviceAgentId = "sarah",
                            DeviceInternalName = dev.Name,
                            DeviceKind = Device.DeviceKindHomeAutomation
                        };
                        device.DeviceGivenName = "Hue Bridge";
                        device.DeviceRoles.Add("hue-bridge");
                        device.DeviceAddresses.Add(dev.IpV4);
                        device.MeshId = "local";
                        RegisterBridgeDevice(device);
                    }
                }
            }
        }

        private static void RegisterBridgeDevice(params Device[] devices)
        {
            foreach (var dev in devices)
                dev.DevicePlatform = "hue";

            for (int i = 0; i < 3; i++)
            {
                try
                {
                    using (var cli = new MainApiAgentWebClient("sarah"))
                    {
                        var exts = cli.UploadData<List<Device>, Device[]>(
                            $"/v1.0/devices/register/sarah",
                            "POST", devices);
                        if (exts != null && exts.Count >= 1)
                            _bridge = exts[0];
                        else
                            _bridge = devices.FirstOrDefault();
                        break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    _bridge = devices.FirstOrDefault();
                }

            }
        }

        private static bool TestIfHueBridge(NetworkDeviceData dev)
        {
            try
            {
                using (var cli = new WebClient())
                {
                    Console.WriteLine($"Hue - checking for Hue API on {dev.IpV4}");
                    string html = cli.DownloadString($"http://{dev.IpV4}/debug/clip.html");
                    if (!string.IsNullOrEmpty(html))
                    {
                        Console.WriteLine($"Hue - checking for Hue API on {dev.IpV4} : found !");
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Hue - checking for Hue API on {dev.IpV4} : NOT found !");
                return false;
            }

            Console.WriteLine($"Hue - checking for Hue API on {dev.IpV4} : NOT found !");
            return false;
        }
    }
}
