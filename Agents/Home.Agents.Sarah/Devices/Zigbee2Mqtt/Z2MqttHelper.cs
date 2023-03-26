using AdaptiveCards.Rendering;
using Home.Agents.Sarah.Devices.Hue;
using Home.Common;
using Home.Common.HomeAutomation;
using Home.Common.Messages;
using Home.Common.Model;
using Home.Graph.Common;
using MongoDB.Bson;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
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
        private static DateTimeOffset _lastStart = DateTimeOffset.Now;
        private static DateTimeOffset _lastDeviceInfo = DateTimeOffset.MinValue;

        public static bool _stop = false;
        public static void Start()
        {
            string pth = _root;
            if (!pth.EndsWith("/"))
                pth = pth + "/";
            pth = pth + "#";

            MqttHelper.AddChangeHandler(pth, HandleChange);
        }

        public static void Stop()
        {
            _stop = true;
        }


        private static void HandleChange(string topic, string data)
        {
            string subPath = topic;
            if (subPath.StartsWith(_root))
                subPath = subPath.Substring(_root.Length);
            if (subPath.StartsWith("/"))
                subPath = subPath.Substring(1);

            if (subPath.Equals("bridge/info"))
            {
                var tmp = JsonConvert.DeserializeObject<Z2MqttInfo>(data);
                Console.WriteLine($"Zigbee2MQTT - update of bridge info");
                if (tmp != null)
                    ParseInfo(tmp);
                return;
            }

            if (subPath.Equals("bridge/devices"))
            {
                var tmp = JsonConvert.DeserializeObject<DeviceInfo[]>(data);
                _lastDeviceInfo = DateTimeOffset.Now;
                Console.WriteLine($"Zigbee2MQTT - update of device list");
                if (tmp != null)
                    ParseDevices(tmp);
                return;
            }

            foreach (var t in _allDevices.Values)
            {
                var pth = t._mqttPath;
                if (pth.EndsWith("/"))
                    pth = pth.Substring(pth.Length - 1);
                var tpc = topic;
                if (tpc.EndsWith("/"))
                    tpc = tpc.Substring(tpc.Length - 1);

                if (pth.Equals(tpc, StringComparison.InvariantCultureIgnoreCase))
                {
                    Console.WriteLine($"Zigbee2MQTT - update of device " + t._deviceIeeeID);
                    t.RefreshStatusFromTopic(JsonConvert.DeserializeObject<Dictionary<string, object>>(data));
                    return;
                }

            }

            // on détecte le fait que le /bridge/devices soit KO
            if(_allDevices.Count==0)
            {
                // si on a pas reçu de bridge/devices et qu'on
                // est démarré depuis 5 min, on restart zigbee2mqtt
                if(_lastDeviceInfo < _lastStart 
                    && Math.Abs((DateTimeOffset.Now - _lastStart).TotalMinutes) > 5)
                {
                    string pth = _root;
                    if (!pth.EndsWith("/"))
                        pth = pth + "/";
                    pth += "bridge/request/restart";
                    _lastStart= DateTimeOffset.Now;
                    MqttHelper.PublishJson(pth, "");
                }
            }

            //Console.WriteLine($"Zigbee2MQTT - update of {topic} => unknown");
        }

        private static void ParseDevices(DeviceInfo[] allDevices)
        {
            var ret = new List<Z2MqttDeviceBase>();

            foreach (var device in allDevices)
            {
                if (device.type == null || device.type.Equals("coordinator", StringComparison.InvariantCultureIgnoreCase))
                    continue;

                Z2MqttDeviceBase bs = null;
                if (!_allDevices.TryGetValue(device.ieee_address, out bs))
                {
                    bs = InstantiateFromDevice(device);
                    if (bs != null)
                        ret.Add(bs);
                }
                else
                {

                }
            }

            RefreshDeviceList(ret.ToArray());
        }

        private static Z2MqttDeviceBase InstantiateFromDevice(DeviceInfo device)
        {
            string type = DiscoverDeviceType(device);
            if (string.IsNullOrEmpty(type))
                return null;

            string fullMqttPath = _root;
            if (!fullMqttPath.EndsWith("/"))
                fullMqttPath += "/";

            fullMqttPath += device.friendly_name;
            Z2MqttDeviceBase ret = null;

            switch (type)
            {
                case "color-light":
                case "brightness-light":
                case "basic-switch-light":
                    ret = new Z2MqttLightSwitch(device.ieee_address, fullMqttPath);
                    ret.RefreshFromConfig(device);
                    break;
                case "sensor":
                    ret = new Z2MqttSensor(device.ieee_address, fullMqttPath);
                    ret.RefreshFromConfig(device);
                    break;
            }

            return ret;
        }

        private static string DiscoverDeviceType(DeviceInfo device)
        {
            if (device.definition != null &&
                device.definition.exposes != null)
            {
                foreach (var exp in device.definition.exposes)
                {
                    if (exp.type != null)
                    {
                        switch (exp.type.ToLowerInvariant())
                        {
                            case "switch":
                            case "light":
                                if (exp.features != null)
                                {
                                    bool hasSwitch = (from z in exp.features where z.property.Equals("state") select z).FirstOrDefault() != null;
                                    bool hasBrightness = (from z in exp.features where z.property.Equals("brightness") select z).FirstOrDefault() != null;
                                    bool hasColorTemperature = false;

                                    if (hasSwitch)
                                    {
                                        if (hasBrightness)
                                        {
                                            if (hasColorTemperature)
                                                return "color-light";
                                            else
                                                return "brightness-light";
                                        }
                                        else
                                            return "basic-switch-light";
                                    }
                                    return null;
                                }
                                break;
                            default:
                                if (exp.name != null)
                                {
                                    if (Z2MqttSensor.IsManagedProperty(exp))
                                        return "sensor";
                                }
                                break;
                        }
                    }
                }
            }

            return null;
        }

        private static void ParseInfo(Z2MqttInfo tmp)
        {

        }

        public static void RefreshDeviceList(Z2MqttDeviceBase[] ret)
        {

            var toRegister = new List<Device>();
            foreach (Z2MqttDeviceBase r in ret)
            {
                Z2MqttDeviceBase already = null;
                if (!_allDevices.TryGetValue(r.DeviceName, out already))
                {
                    _allDevices.Add(r.DeviceName, r);
                    ConvertAndAddToRegister(toRegister, r);
                }
                else
                {
                }
            }

            Console.WriteLine($"Z2MQTT - Registering {toRegister.Count} zigbee devices");

            if (toRegister != null && toRegister.Count > 0)
                RegisterDevice(toRegister.ToArray());

            var t = _allDevices.Values.ToArray();
            foreach (Z2MqttDeviceBase r in t)
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
        }

        private static void ConvertAndAddToRegister(List<Device> toRegister, Z2MqttDeviceBase r)
        {
            var devreg = new Device(r);
            devreg.DeviceRoles.Add("zigbee");
            if (r is Z2MqttLightSwitch)
                devreg.DeviceRoles.Add(Device.HomeAutomationMainRoleLight);
            devreg.DeviceAddresses.Add(r._mqttPath);
            devreg.DeviceGivenName = r._mqttPath;
            toRegister.Add(devreg);
        }

        private static void RegisterDevice(params Device[] devices)
        {
            foreach (var dev in devices)
                dev.DevicePlatform = "zigbee2mqtt";

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




    }
}
