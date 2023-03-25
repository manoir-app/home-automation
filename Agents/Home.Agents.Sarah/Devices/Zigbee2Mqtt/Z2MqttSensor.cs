using Home.Common.HomeAutomation;
using Home.Common.Model;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Home.Agents.Sarah.Devices.Zigbee2Mqtt
{
    internal class Z2MqttSensor : Z2MqttDeviceBase, ISensorDevice
    {
        private Dictionary<string, string> _mappings = new Dictionary<string, string>();
        private Dictionary<string, string> _lastReadings = new Dictionary<string, string>();

        public Z2MqttSensor(string deviceIeeeID, string mqttPath) : base(deviceIeeeID, mqttPath)
        {
        }

        internal static bool IsManagedProperty(Z2MqttHelper.DeviceExpose exp)
        {
            if (exp.property != null)
            {
                switch (exp.property.ToLowerInvariant())
                {
                    case "temperature":
                    case "humidity":
                    case "pressure":
                        return true;
                }
            }

            return false;
        }

        public IEnumerable<DeviceData> GetSensors()
        {

            var dvs = new List<DeviceData>();

            foreach (var r in _lastReadings.Keys)
            {
                dvs.Add(
                    new DeviceData()
                    {
                        IsMainData = true,
                        Name = r,
                        Value = _lastReadings[r],
                        StandardDataType = r
                    });
            }

            return dvs;
        }

        internal override void RefreshFromConfig(Z2MqttHelper.DeviceInfo device)
        {
            if (device.definition != null &&
               device.definition.exposes != null)
            {
                foreach (var exp in device.definition.exposes)
                {
                    if (exp.property != null)
                    {
                        switch (exp.property.ToLowerInvariant())
                        {
                            case "temp":
                            case "temperature":
                                _mappings.Add(exp.property, DeviceData.DataTypeSensorTemperature);
                                Console.WriteLine($"Zigbee2MQTT - matching {exp.property} to temperature");
                                break;
                            case "humidity":
                                _mappings.Add(exp.property, DeviceData.DataTypeSensorHumidity);
                                Console.WriteLine($"Zigbee2MQTT - matching {exp.property} to humidity");
                                break;
                            case "pressure":
                                _mappings.Add(exp.property, DeviceData.DataTypeSensorPressure);
                                Console.WriteLine($"Zigbee2MQTT - matching {exp.property} to pressure");
                                break;
                        }
                    }
                }
            }
        }

        internal override void RefreshStatusFromTopic(Dictionary<string, object> values)
        {
            foreach (var k in values.Keys)
            {
                if (_mappings.TryGetValue(k, out string valueName))
                {
                    var o = values[k];
                    if (o == null)
                        _lastReadings.Add(valueName, "");
                    else if (o is bool)
                        _lastReadings.Add(valueName, ((bool)o) ? "on" : "off");
                    else if (o is float)
                        _lastReadings.Add(valueName, ((float)o).ToString("0.00", CultureInfo.InvariantCulture));
                    else if (o is decimal)
                        _lastReadings.Add(valueName, ((decimal)o).ToString("0.00", CultureInfo.InvariantCulture));
                    else if (o is double)
                        _lastReadings.Add(valueName, ((double)o).ToString("0.00", CultureInfo.InvariantCulture));
                    else if (o is int)
                        _lastReadings.Add(valueName, ((int)o).ToString("0", CultureInfo.InvariantCulture));
                    else if (o is long)
                        _lastReadings.Add(valueName, ((long)o).ToString("0", CultureInfo.InvariantCulture));
                    else if (o is short)
                        _lastReadings.Add(valueName, ((short)o).ToString("0", CultureInfo.InvariantCulture));
                    else if (o is byte)
                        _lastReadings.Add(valueName, ((byte)o).ToString("0", CultureInfo.InvariantCulture));
                    else
                        _lastReadings.Add(valueName, (o).ToString());
                }
                else
                {
                    Console.WriteLine($"Zigbee2MQTT - property {k} not mapped");

                }
            }

            base.RefreshSensors(_lastReadings);

        }
    }
}
