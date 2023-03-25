using Home.Agents.Sarah.Devices.Hue;
using Home.Common.HomeAutomation;
using Home.Common.Messages;
using Home.Common.Model;
using Home.Graph.Common;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static Home.Agents.Sarah.Devices.Hue.HueHelper;

namespace Home.Agents.Sarah.Devices.Zigbee2Mqtt
{
    internal abstract class Z2MqttDeviceBase : DeviceBase
    {
        public override string DeviceName
        {
            get
            {
                return "z2mqtt-" + _deviceIeeeID;
            }
        }

        protected internal string _deviceIeeeID = null;
        protected internal string _mqttPath = null;

        protected Z2MqttDeviceBase(string deviceIeeeID, string mqttPath)
        {
            _deviceIeeeID = deviceIeeeID;
            _mqttPath = mqttPath;
        }

        internal virtual void ForceRefresh()
        {

        }

        internal abstract void RefreshFromConfig(Z2MqttHelper.DeviceInfo device);

        internal abstract void RefreshStatusFromTopic(Dictionary<string, object> values);

        internal void RefreshSensors(Dictionary<string, string> values)
        {
            UpdateInTimeDb(values);

            var changed = new List<DeviceStateChangedMessage.DeviceStateValue>();
            foreach (var k in values.Keys)
            {
                changed.Add(new DeviceStateChangedMessage.DeviceStateValue() { Name = k, Value = values[k] });
            }
            Console.WriteLine($"Zigbee2MQTT - changing propertues {string.Join(',', (from z in changed select z.Name).ToArray())}");
            DeviceManager.OnDeviceStateChanged("Zigbee2Mqtt", DeviceName, Device.HomeAutomationMainRoleSensors, "", changed.ToArray());
        }

        internal void RefreshLightOrSwitchState(bool on, decimal? intensity, Color? color)
        {
            string globalStatus = "Eteint";
            if (on)
            {
                if (this is IIntensityGradientDevice && intensity.HasValue)
                {
                    int pct = (int)Math.Ceiling(intensity.Value);
                    globalStatus = $"Allumé à {pct} %";

                }
                else
                {
                    globalStatus = "Allumé";
                }
            }

            UpdateInTimeDb(on, intensity, color);

            var changed = new List<DeviceStateChangedMessage.DeviceStateValue>();

            changed.Add(new DeviceStateChangedMessage.DeviceStateValue() { Name = "on", Value = on ? "on" : "off" });

            if (changed.Count > 0)
                DeviceManager.OnDeviceStateChanged("Zigbee2Mqtt", DeviceName, Device.HomeAutomationRoleSwitch, globalStatus, changed.ToArray());

            changed.Clear();
            if (this is IIntensityGradientDevice && intensity.HasValue)
            {
                changed.Add(new DeviceStateChangedMessage.DeviceStateValue() { Name = "brightness", Value = Math.Ceiling(intensity.Value).ToString("0") });
                DeviceManager.OnDeviceStateChanged("Zigbee2Mqtt", DeviceName, Device.HomeAutomationRoleDimmer, globalStatus, changed.ToArray());
            }



            changed.Clear();
            if (this is IColorBoundDevice && color.HasValue)
            {
                try
                {
                    changed.Add(new DeviceStateChangedMessage.DeviceStateValue() { Name = "color", Value = color.Value.ToString() });
                }
                catch
                {

                }
                if (changed.Count > 0)
                    DeviceManager.OnDeviceStateChanged("Zigbee2Mqtt", DeviceName, Device.HomeAutomationRoleColorBound, globalStatus, changed.ToArray());
            }

        }


        private void UpdateInTimeDb(Dictionary<string, string> values)
        {
            foreach (var k in values.Keys)
            {

                try
                {
                    // on push en timedb
                    if (decimal.TryParse(values[k], System.Globalization.NumberStyles.Number, CultureInfo.InvariantCulture, out decimal val))
                    {
                        TimeDBHelper.Trace("home", "entities", k, val, new Dictionary<string, string>()
                        {
                            {"deviceId", DeviceName},
                            {"roomId", ""},
                            {"locationId", ""},
                        });
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Err updating timedb for Z2mqtt : " + ex);
                }
            }
        }

        private void UpdateInTimeDb(bool on, decimal? intensity, Color? color)
        {
            try
            {
                // on push en timedb
                if (on)
                {
                    TimeDBHelper.Trace("home", "entities", "brightness", intensity.GetValueOrDefault(100), new Dictionary<string, string>()
                        {
                            {"deviceId", DeviceName},
                            {"roomId", ""},
                            {"locationId", ""},
                        });
                }
                else
                {
                    TimeDBHelper.Trace("home", "entities", "brightness", 0, new Dictionary<string, string>()
                        {
                            {"deviceId", DeviceName},
                            {"roomId", ""},
                            {"locationId", ""},
                        });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Err updating timedb for Z2mqtt : " + ex);
            }
        }

    }
}
