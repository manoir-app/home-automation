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

namespace Home.Agents.Sarah.Devices.Divoom
{
    public static partial class DivoomDeviceHelper
    {
        public static bool _stop = false;
        public static void Start()
        {
            var t = new Thread(() => DivoomDeviceHelper.Run());
            t.Name = "Divoom devices";
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
                        Console.WriteLine("Divoom - ERR in DivoomThread - Device discovery failed");
                    Thread.Sleep(20000);
                }
                catch (Exception ex)
                {
                    Console.Write("Divoom - ERR in DivoomThread - ");
                    Console.WriteLine(ex.ToString());
                }
            }
        }

        public static void Stop()
        {
            _stop = true;
        }

        private static Dictionary<string, BaseDivoomDevice> _allDevices = new Dictionary<string, BaseDivoomDevice>();


        private class GetLanDevice
        {
            public int ReturnCode { get; set; }
            public string ReturnMessage { get; set; }
            public LanDevice[] DeviceList { get; set; }
        }

        private class LanDevice
        {
            public string DeviceName { get; set; }
            public int DeviceId { get; set; }
            public string DevicePrivateIP { get; set; }
        }


        public static DeviceBase[] GetDevices()
        {
            // appeler : https://app.divoom-gz.com/Device/ReturnSameLANDevice
            return new DeviceBase[] { };
        }

        private static void RegisterDevice(params Device[] devices)
        {
            foreach (var dev in devices)
                dev.DevicePlatform = "divoom";

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

        private static DisplayDeviceBase GetDeviceBase(NetworkDeviceData dev)
        {
            try
            {
                var t = SendCommand<GetIndexResponse>(dev.IpV4, new CommandData() { Command = "Channel/GetIndex" });
                if (t != null)
                {
                    // pour l'instant, on met le 64 en dur
                    // vu que c'est pour nous :)
                    return new Pixoo64(dev.IpV4);
                }
            }
            catch (WebException ex)
            {
                return null;
            }

            return null;
        }

    }
}
