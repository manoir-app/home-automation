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
using System.Drawing;

namespace Home.Agents.Sarah.Devices.Hue
{
    internal static partial class HueHelper
    {
        private static Dictionary<string, HueDeviceBase> _allDevices = new Dictionary<string, HueDeviceBase>();

        public static HueLight SetLightStateRgb(string lightId, bool? on, short? brightness, Color? color)
        {
            if (!on.HasValue && !brightness.HasValue)
                return null;

            var id = GetToken()?.Token;
            if (id == null)
                return null;

            var dev = GetHueBridge();
            if (dev == null)
                return null;


            Dictionary<string, object> changes = new Dictionary<string, object>();

            if (on.HasValue)
                changes.Add("on", on.Value);
            if (brightness.HasValue)
                changes.Add("bri", (short)(brightness.Value));

            if (color.HasValue)
                changes.Add("xy", new XYLight(color.Value));

            string ipV4 = dev.DeviceAddresses.FirstOrDefault();
            using (var cli = new WebClient())
            {
                cli.Headers.Add(HttpRequestHeader.ContentType, "application/json");
                string json = cli.UploadString(
                    $"http://{ipV4}/api/{id}/lights/{lightId}/state", "PUT", JsonConvert.SerializeObject(changes));
                var ret = JsonConvert.DeserializeObject<ChangeResponse[]>(json);

                json = cli.DownloadString(
                    $"http://{ipV4}/api/{id}/lights/{lightId}");
                var ret2 = JsonConvert.DeserializeObject<HueLight>(json);
                return ret2;
            }

        }

        public static Dictionary<string, HueLight> GetLights()
        {
            var id = GetToken()?.Token;
            if (id == null)
                return new Dictionary<string, HueLight>();

            var dev = GetHueBridge();
            if (dev == null)
                return new Dictionary<string, HueLight>();

            string ipV4 = dev.DeviceAddresses.FirstOrDefault();
            using (var cli = new WebClient())
            {
                string json = cli.DownloadString($"http://{ipV4}/api/{id}/lights");
                var ret = JsonConvert.DeserializeObject<GetHueLightResponse>(json);
                return ret;
            }
        }

        public static HueLight GetLight(string lightId)
        {
            var id = GetToken()?.Token;
            if (id == null)
                return null;

            var dev = GetHueBridge();
            if (dev == null)
                return null;

            string ipV4 = dev.DeviceAddresses.FirstOrDefault();
            using (var cli = new WebClient())
            {
                string json = cli.DownloadString($"http://{ipV4}/api/{id}/lights/{lightId}");
                var ret = JsonConvert.DeserializeObject<HueLight>(json);
                return ret;
            }
        }

        private static void RegisterLight(HueDeviceBase baseDevice)
        {
            var device = new Device(baseDevice);
            device.DeviceRoles.Add("hue-light");
            device.DeviceRoles.Add(Device.HomeAutomationRoleLight);
            device.MeshId = "local";
            RegisterHueDevice(device);
        }

        private static void RegisterHueDevice(params Device[] devices)
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
                        break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }

            }
        }



        public static DeviceBase GetDeviceBase(string lightId)
        {
            var light = GetLight(lightId);
            var dev = GetDeviceBase(lightId, light);

            HueDeviceBase alreadyReg = null;
            if (_allDevices.TryGetValue(lightId, out alreadyReg))
                _allDevices[lightId] = dev;
            else
                _allDevices.Add(lightId, dev);

            return dev;
        }

        private static HueDeviceBase GetDeviceBase(string lightId, HueLight light)
        {
            if (light == null)
                return null;

            HueDeviceBase alreadyReg = null;
            if (_allDevices.TryGetValue(lightId, out alreadyReg))
            {
                if (alreadyReg.Equals(light))
                    return alreadyReg;
                else
                    _allDevices.Remove(lightId);
            }


            if (light.type == null)
                return new HueLightSwitch(lightId, light);

            switch (light.type.ToLowerInvariant())
            {
                case "on/off light":
                case "on/off plug-in unit":
                    return new HueLightSwitch(lightId, light);
                case "dimmable light":
                case "color temperature light":
                    return new HueSimpleLight(lightId, light);
                case "extended color light":
                case "color light":
                    return new HueColorLight(lightId, light);
                default:
                    return new HueLightSwitch(lightId, light);
            }
        }
    }
}
