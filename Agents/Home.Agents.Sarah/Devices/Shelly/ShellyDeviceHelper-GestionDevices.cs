using Home.Common;
using Home.Common.HomeAutomation;
using Home.Common.Messages;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Home.Common.Model;
using System.Threading;

namespace Home.Agents.Sarah.Devices.Shelly
{
    partial class ShellyDeviceHelper
    {
        public static bool _stop = false;
        public static void Start()
        {
            var t = new Thread(() => ShellyDeviceHelper.Run());
            t.Name = "Shelly devices";
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
                    Console.Write("Shelly - ERR in ShellyThread - ");
                    Console.WriteLine(ex.ToString());
                }
            }
        }

        public static void Stop()
        {
            _stop = true;
        }


        private static Dictionary<string, ShellyDeviceBase> _allDevices = new Dictionary<string, ShellyDeviceBase>();


        internal static DeviceBase[] GetDevices(NetworkDeviceEnumerateMessageResponse alldevs)
        {
            if (!alldevs.Response.Equals("ok", StringComparison.InvariantCultureIgnoreCase))
                return null;

            List<DeviceBase> ret = new List<DeviceBase>();

            foreach (var dev in alldevs.ActiveDevices)
            {
                if (dev.Vendor != null && dev.Vendor.Contains("Espressif", StringComparison.InvariantCultureIgnoreCase))
                {
                    Console.WriteLine($"Shelly - Found {dev.IpV4} as a potential shelly device");

                    var dvb = GetDeviceBase(dev);
                    if (dvb != null)
                    {
                        Console.WriteLine($"Shelly - Adding {dev.IpV4} as shelly device : {dvb.DeviceName}");
                        ret.Add(dvb);
                    }
                }
            }

            var toRegister = new List<Device>();
            var toConfigure = new List<ShellyDeviceBase>();
            foreach (ShellyDeviceBase r in ret)
            {
                ShellyDeviceBase already = null;
                if (!_allDevices.TryGetValue(r.DeviceName, out already))
                {
                    _allDevices.Add(r.DeviceName, r);
                    var devreg = new Device(r);
                    devreg.DeviceRoles.Add("shelly");
                    devreg.DeviceAddresses.Add(r.IpV4);
                    devreg.DeviceGivenName = r.DeviceName;
                    toRegister.Add(devreg);
                    toConfigure.Add(r);
                }
                else
                {
                    if (already.IpV4 != r.IpV4)
                    {
                        already.IpV4 = r.IpV4;
                    }
                }
            }

            Console.WriteLine($"Shelly - Registering {toRegister.Count} shelly devices");

            if (toRegister != null && toRegister.Count > 0)
            {
                foreach (var r in toRegister)
                {
                    try
                    {
                        var settings = ShellyDeviceHelper.GetSettings(r.DeviceAddresses[0]);
                        if (!string.IsNullOrEmpty(settings.name))
                            r.DeviceGivenName = settings.name;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Shelly - Registering devices - get name failed with : {ex.Message}");
                    }
                }
                RegisterDevice(toRegister.ToArray());
                ThreadPool.QueueUserWorkItem((o) =>
                {
                    foreach (var dev in toConfigure)
                    {
                        try
                        {
                            if (dev is IAutoSetupDevice)
                            {
                                var sw = dev as Common.HomeAutomation.IAutoSetupDevice;
                                var resSetup = sw.GetSetupStatus();
                                if (resSetup == Common.HomeAutomation.AutoSetupDeviceStatus.NewDevice)
                                {
                                    Console.WriteLine($"Shelly - {dev.DeviceId} is a new device !");
                                    sw.InitialSetup();
                                    Console.WriteLine($"Shelly - {dev.DeviceId} configured !");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Shelly - {dev.DeviceId} error in autoconfig : " + ex);
                        }
                    }
                });
            }

            var t = _allDevices.Values.ToArray();
            foreach (ShellyDeviceBase r in t)
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

        private static void RegisterDevice(params Device[] devices)
        {
            foreach (var dev in devices)
                dev.DevicePlatform = "shelly";

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

        private static DeviceBase GetDeviceBase(NetworkDeviceData dev)
        {
            string ipV4 = dev.IpV4;
            return GetDeviceBase(ipV4);
        }

        internal static DeviceBase GetDeviceBase(string ipV4)
        {
            var sdi = GetDeviceInfo(ipV4);

            if (sdi == null)
            {
                Console.WriteLine($"Shelly - {ipV4} is not a shelly device");
                return null;
            }
            return GetDeviceBase(sdi, ipV4);
        }


        private static DeviceBase GetDeviceBase(GetShellyDeviceInfo sdi, string ipv4)
        {
            if (ipv4 == null || sdi == null)
                return null;

            var tmp = GetSettings<Shelly25DeviceBase.GetShelly25SettingsResponse>(ipv4);
            if (tmp == null)
            {
                Console.WriteLine($"Shelly - {ipv4} did not respond to get settings");
                return new UnknowShellyDevice(ipv4, sdi.mac);
            }

            Console.WriteLine($"{ipv4} is a {sdi.type}");

            switch (sdi.type?.ToUpperInvariant())
            {
                case "SHBTN-1":
                    return new ShellyButton1(ipv4, tmp.device.hostname);
                case "SHPLG-S":
                    return new ShellySimpleSwitch(ipv4, tmp.device.hostname);
                case "SHSW-25":
                    if (tmp.mode == null)
                    {
                        Console.WriteLine($"{ipv4} is a {sdi.type} and should have a mode");
                        return new UnknowShellyDevice(ipv4, tmp.device.hostname);
                    }
                    switch (tmp.mode.ToLowerInvariant())
                    {
                        case "roller":
                        case "shutter":
                            return new Shelly25Roller(ipv4, tmp.device.hostname);
                        case "relay":
                            return new Shelly25MultiRelay(ipv4, tmp.device.hostname);
                        default:
                            return new UnknowShellyDevice(ipv4, sdi.mac);
                    }
            }

            return null;
        }

    }
}
