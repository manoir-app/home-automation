using Home.Common;
using Home.Common.HomeAutomation;
using Home.Common.Messages;
using Home.Common.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Home.Agents.Sarah.Devices.Divoom
{
    partial class DivoomDeviceHelper
    {

        internal static MessageResponse HandleMessage(MessageOrigin origin, string topic, string messageBody)
        {
            if (topic.StartsWith("homeautomation.divoom"))
            {
                HomeAutomationMessageResponse resp = new HomeAutomationMessageResponse();

                var msg = JsonConvert.DeserializeObject<HomeAutomationMessage>(messageBody);

                foreach(var ope in msg.Operations)
                {
                    var devices = FindDeviceForOperation(ope.DeviceName);
                    if(devices!=null)
                    {
                        foreach(var device in devices)
                        {
                            Console.WriteLine($"Divoom - Executing {ope.Role}/{ope.ElementName} {ope.Value} on {device.DeviceName}");
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
                }

                return MessageResponse.OK;
            }
            else if(topic.StartsWith("display.divoom"))
            {
                // pour l'instant pas de gestion de ce type de message,
                // vu qu'on ne sait pas encore comment ils vont
                // fonctionner :p
                return MessageResponse.OK;
            }

            return MessageResponse.GenericFail;
        }

        private static bool ExecuteCommand(BaseDivoomDevice device, string role, string elementName, string value)
        {
            if (role.Equals(Device.DisplayRoleImageDisplay))
            {

                return true;
            }
            else
                return false;
        }

        private static IEnumerable<BaseDivoomDevice> FindDeviceForOperation(string deviceName)
        {
            BaseDivoomDevice dev = null;

            if (_allDevices.TryGetValue(deviceName, out dev))
                return new BaseDivoomDevice[] { dev };

            return null;
        }
    }
}