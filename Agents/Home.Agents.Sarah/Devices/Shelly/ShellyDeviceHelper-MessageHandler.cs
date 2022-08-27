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
        internal static MessageResponse HandleMessage(MessageOrigin origin, string topic, string messageBody)
        {
            if (topic.StartsWith("homeautomation.shelly"))
            {
                // on ne gère pas les trucs depuis les noms de topics pour l'instant
                HomeAutomationMessageResponse resp = new HomeAutomationMessageResponse();

                var msg = JsonConvert.DeserializeObject<HomeAutomationMessage>(messageBody);

                Console.WriteLine($"Shelly - Received a message with {msg.Operations?.Count()} operations");
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
                                    Console.WriteLine($"Shelly - Executing {ope.Role}/{ope.ElementName} {ope.Value} on {device.DeviceName}");
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
            }
            else if (topic.StartsWith("homeautomation.discovery.shelly"))
            {

            }
            return MessageResponse.GenericFail;
        }

        private static bool ExecuteCommand(ShellyDeviceBase device, string role, string elementName, string value)
        {
            try
            {
                if (role == null)
                    role = "generic";
                switch (role)
                {
                    case "switch":
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
                        DeviceManager.OnDeviceStateChanged("Shelly", device.DeviceId, Device.HomeAutomationRoleSwitch, sw.ChangeSwitchValue(elementName, value));
                        break;
                }

                return false;
            }
            catch(Exception ex)
            {
                Console.Error.WriteLine(ex);
                return false;
            }
        }

     

        private static IEnumerable<ShellyDeviceBase> FindDeviceForOperation(string deviceName)
        {
            ShellyDeviceBase dev = null;

            if (deviceName != null && deviceName == "*")
                return _allDevices.Values.ToArray();

            if (_allDevices.TryGetValue(deviceName, out dev))
                return new ShellyDeviceBase[] { dev };

            return null;
        }
    }
}
