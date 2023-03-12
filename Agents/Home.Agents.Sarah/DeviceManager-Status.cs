using Home.Common;
using Home.Common.HomeAutomation;
using Home.Common.Messages;
using Home.Common.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Home.Agents.Sarah
{
    partial class DeviceManager
    {
        public static void OnDeviceStateChanged(string devicePlatform, string deviceId, string role, params DeviceStateChangedMessage.DeviceStateValue[] changedValues)
        {
            OnDeviceStateChanged(devicePlatform, deviceId, role, null, changedValues);
        }
        public static void OnDeviceStateChanged(string devicePlatform, string deviceId, string role, string mainStatus, params DeviceStateChangedMessage.DeviceStateValue[] changedValues)
        {
            if (changedValues == null)
                return;

            var msg = new DeviceStateChangedMessage(devicePlatform, deviceId, role, changedValues);
            Console.WriteLine($"Raising DeviceStateChanged for : {deviceId}/{role} : " + JsonConvert.SerializeObject(changedValues));

            // 1) on refresh sur le serveur

            foreach(var t in changedValues)
            {
                PostChangeState(deviceId, t, mainStatus);
            }
           
            // 2) on publie sur NATS
            NatsMessageThread.Push(msg);

            // 3) on refresh MQTT
            


        }

        private static void PostChangeState(string deviceId, DeviceData deviceData, string mainStatus)
        {
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    string url = $"v1.0/devices/all/{deviceId}/status/data";
                    if (!string.IsNullOrEmpty(mainStatus))
                        url += $"?mainStatus=" + (Uri.EscapeDataString(mainStatus));
                    using (var cli = new MainApiAgentWebClient("sarah"))
                    {
                        var b = cli.UploadData<bool, DeviceData>(url, "POST", deviceData);
                        return;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error updating device state for " + deviceId + " : " + ex);
                }
            }
        }
    }
}
