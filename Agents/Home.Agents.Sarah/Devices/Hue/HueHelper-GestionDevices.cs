using Home.Common.HomeAutomation;
using Home.Common.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Home.Agents.Sarah.Devices.Hue
{
    partial class HueHelper
    {
        public static bool _stop = false;
        public static void Start()
        {
            var t = new Thread(() => HueHelper.Run());
            t.Name = "Hue devices";
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
                        Console.WriteLine("Hue - ERR in HueThread - Device discovery failed");
                    Thread.Sleep(5000);
                }
                catch (Exception ex)
                {
                    Console.Write("Hue - ERR in HueThread - ");
                    Console.WriteLine(ex.ToString());
                }
            }
        }

        public static void Stop()
        {
            _stop = true;
        }

        private static DeviceBase[] GetDevices()
        {
            List<DeviceBase> ret = new List<DeviceBase>();
            var alldevs = GetLights();

            Console.WriteLine($"Hue - Found {alldevs.Count} devices on hue bridge");

            foreach (var lightId in alldevs.Keys)
            {
                var dev = alldevs[lightId];
                var dvb = GetDeviceBase(lightId, dev);
                if (dvb != null)
                {
                    Console.WriteLine($"Hue - Adding {lightId} as hue device");
                    ret.Add(dvb);
                }
            }

            var toRegister = new List<Device>();
            foreach (HueDeviceBase r in ret)
            {
                HueDeviceBase already = null;
                if (!_allDevices.TryGetValue(r.DeviceName, out already))
                {
                    _allDevices.Add(r.DeviceName, r);
                    ConvertAndAddToRegister(toRegister, r);
                }
                else
                {
                    r.RefreshState(r._light);
                }
            }

            Console.WriteLine($"Hue - Registering {toRegister.Count} hue devices");

            if (toRegister != null && toRegister.Count > 0)
                RegisterHueDevice(toRegister.ToArray());

            var t = _allDevices.Values.ToArray();
            foreach (HueDeviceBase r in t)
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

        private static void ConvertAndAddToRegister(List<Device> toRegister, HueDeviceBase r)
        {
            var devreg = new Device(r);
            devreg.DeviceRoles.Add("hue");
            devreg.DeviceRoles.Add(Device.HomeAutomationMainRoleLight);
            devreg.DeviceGivenName = r._light.name;
            toRegister.Add(devreg);
        }
    }
}
