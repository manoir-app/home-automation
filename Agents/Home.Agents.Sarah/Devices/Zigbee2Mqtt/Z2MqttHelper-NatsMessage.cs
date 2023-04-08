using AdaptiveCards.Rendering;
using Home.Agents.Sarah.Devices.Hue;
using Home.Common;
using Home.Common.HomeAutomation;
using Home.Common.Messages;
using Home.Common.Model;
using Home.Graph.Common;
using MongoDB.Bson;
using NATS.Client;
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
    partial class Z2MqttHelper
    {
        internal static MessageResponse HandleMessage(MessageOrigin origin, string topic, string messageBody)
        {
                                    Console.WriteLine($"Handling message on  {topic} in Z2MqttHelper");

            if (topic.Equals("homeautomation.zigbee2mqtt.directcommand"))
            {
                var msgDirect = JsonConvert.DeserializeObject<HomeAutomationPlatformCommandMessage>(messageBody);
                return HandleDirectCommandMessage(msgDirect);
            }

            if (topic.StartsWith("homeautomation.zigbee2mqtt"))
            {
                string deviceId, action, value;
                DeviceManager.ParseFromTopic(topic, out deviceId, out action, out value);

                string[] lights = null;
                if (!string.IsNullOrWhiteSpace(deviceId))
                {
                    if (deviceId.Equals("*"))
                    {
                        lights = (from z in _allDevices.Keys
                                  select z).ToArray();
                    }
                    else
                    {
                        if (_allDevices.Keys.Contains(deviceId))
                            lights = new string[] { deviceId };
                    }
                }

                HomeAutomationMessageResponse resp = new HomeAutomationMessageResponse();

                var msg = JsonConvert.DeserializeObject<HomeAutomationMessage>(messageBody);
                if (msg.Operations.Count == 0)
                {
                    if (lights != null
                        && !string.IsNullOrWhiteSpace(action)
                        && !string.IsNullOrWhiteSpace(value))
                    {
                        foreach (var light in lights)
                        {
                            msg.Operations.Add(GetOperation(light, action, value));
                        }
                    }
                }

                if (msg.Operations != null && msg.Operations.Count > 0)
                {
                    foreach (var ope in msg.Operations)
                    {
                        try
                        {
                            var devices = FindDeviceForOperation(ope.DeviceName);
                            if (devices != null)
                            {
                                foreach (var device in devices)
                                {
                                    Console.WriteLine($"Executing {ope.Role}/{ope.ElementName} {ope.Value} on {device.DeviceName}");
                                    if (ExecuteCommand(device, ope.Role, ope.ElementName, ope.Value))
                                    {
                                        if (!resp.SucceededOperations.Contains(ope)
                                                && !resp.FailedOperations.Contains(ope))
                                            resp.SucceededOperations.Add(ope);
                                    }
                                    else
                                    {
                                        if (!resp.SucceededOperations.Contains(ope)
                                                && !resp.FailedOperations.Contains(ope))
                                            resp.FailedOperations.Add(ope);
                                    }
                                }
                            }
                            else
                                resp.FailedOperations.Add(ope);

                        }
                        catch
                        {
                            resp.FailedOperations.Add(ope);
                        }
                    }
                }
                else
                    Console.WriteLine($"No operation in Z2MqttHelper : {topic}");


            }
            return MessageResponse.GenericFail;
        }

        private static MessageResponse HandleDirectCommandMessage(HomeAutomationPlatformCommandMessage msg)
        {
            var resp = new HomeAutomationPlatformDirectCommandMessageResponse();

            if (msg.Operations != null && msg.Operations.Count > 0)
            {
                foreach (var ope in msg.Operations)
                {
                    var devices = FindDeviceForOperation(ope.DeviceName);
                    var topic = ope.Command;
                    var value = ope.Data;
                    if (devices != null)
                    {
                        foreach(var device in devices)
                        {
                            MqttHelper.PublishJson($"{device._mqttPath}/{topic}", value);
                        }
                    }

                    resp.SucceededOperations.Add(ope);
                }
            }

            return resp;
        }

        private static Z2MqttDeviceBase[] FindDeviceForOperation(string deviceName)
        {
            Z2MqttDeviceBase dev = null;

            if (deviceName != null && deviceName == "*")
                return _allDevices.Values.ToArray();

            if (_allDevices.TryGetValue(deviceName, out dev))
            {
                Console.Write($"found device {deviceName} with id {dev._deviceIeeeID}");
                return new Z2MqttDeviceBase[] { dev };
            }

            foreach (var light in _allDevices.Values)
            {
                if (light._deviceIeeeID.Equals(deviceName))
                {
                    Console.Write($"found device {deviceName} with id {light._deviceIeeeID}");
                    return new Z2MqttDeviceBase[] { light };
                }
            }

            Console.WriteLine($"Device not found in Z2MqttHelper : {deviceName} in {_allDevices.Count} devices");

            return null;
        }

        private static HomeAutomationMessageOperation GetOperation(string lightId, string role, string value)
        {
            // on le récupère soit via le nom "hue-xxxxxxx-id";
            var dev = (from Z2MqttDeviceBase z in _allDevices.Values
                       where z.DeviceName.Equals(lightId)
                       select z).FirstOrDefault();
            if (dev == null)
            {
                if (!_allDevices.TryGetValue(lightId, out dev))
                    return null;
            }

            switch (role.ToLowerInvariant())
            {

            }
            return null;
        }

        private static bool ExecuteCommand(Z2MqttDeviceBase device, string role, string elementName, string value)
        {
            try
            {
                if (role == null)
                    role = "generic";
                switch (role)
                {
                    case Device.HomeAutomationRoleSwitch:
                        if (!(device is IToggleSwitchDevice))
                            return false;
                        var sw = device as IToggleSwitchDevice;
                        if (elementName == null)
                        {
                            var dda = sw.GetSwitches();
                            elementName = (from z in dda where z.IsMainData select z.Name).FirstOrDefault();
                        }
                        if (elementName == null)
                            return false;
                        DeviceManager.OnDeviceStateChanged("Zigbee2Mqtt", device.DeviceName, Device.HomeAutomationRoleSwitch, sw.ChangeSwitchValue(elementName, value));
                        break;
                    case Device.HomeAutomationRoleDimmer:
                        if (!(device is IIntensityGradientDevice))
                            return false;
                        var dim = device as IIntensityGradientDevice;
                        if (elementName == null)
                        {
                            var dda = dim.GetIntensities();
                            elementName = (from z in dda where z.IsMainData select z.Name).FirstOrDefault();
                        }
                        if (elementName == null)
                            return false;

                        decimal decimalvalue = 0;
                        if (!decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out decimalvalue))
                            return false;
                        DeviceManager.OnDeviceStateChanged("Zigbee2Mqtt", device.DeviceName, Device.HomeAutomationRoleDimmer, dim.ChangeIntensity(elementName, decimalvalue));
                        break;
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                return false;
            }
        }

    }
}
