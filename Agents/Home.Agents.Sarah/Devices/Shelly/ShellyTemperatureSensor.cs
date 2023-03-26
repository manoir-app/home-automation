using Home.Common.HomeAutomation;
using Home.Common.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Home.Agents.Sarah.Devices.Shelly
{
    public class ShellyTemperatureSensor : ShellyDeviceBase, ISensorDevice
    {
        public ShellyTemperatureSensor(string ipv4, string deviceId) : base(ipv4, deviceId)
        {
        }

        public IEnumerable<DeviceData> GetSensors()
        {
            var dvs = new List<DeviceData>();

            dvs.Add(
                new DeviceData()
                {
                    IsMainData = true,
                    Name = DeviceData.DataTypeSensorTemperature,
                    Value = _lastTemp.ToString("0.00", CultureInfo.InvariantCulture),
                    StandardDataType = DeviceData.DataTypeSensorTemperature,
                });
            dvs.Add(
                new DeviceData()
                {
                    IsMainData = true,
                    Name = DeviceData.DataTypeSensorHumidity,
                    Value = _lastHumidity.ToString("0.00", CultureInfo.InvariantCulture),
                    StandardDataType = DeviceData.DataTypeSensorHumidity,
                });

            return dvs;
        }

        private decimal _lastTemp = 0;
        private decimal _lastHumidity = 0;

        protected internal override void RefreshFromMqttTopic(string topicName, string value)
        {
            if (topicName.Equals("sensor/temperature"))
            {
                if (decimal.TryParse(value, System.Globalization.NumberStyles.Number, CultureInfo.InvariantCulture, out decimal temp))
                    _lastTemp = temp;
                else if (decimal.TryParse(value, out decimal temp2))
                    _lastTemp = temp2;
                else
                    Console.WriteLine($"Shelly : unable to parse {topicName} : {value}");
            }
            else if (topicName.Equals("sensor/humidity"))
            {
                if (decimal.TryParse(value, System.Globalization.NumberStyles.Number, CultureInfo.InvariantCulture, out decimal temp))
                    _lastHumidity = temp;
                else if (decimal.TryParse(value, out decimal temp2))
                    _lastHumidity = temp2;
                else
                    Console.WriteLine($"Shelly : unable to parse {topicName} : {value}");
            }
            else
                Console.WriteLine($"Shelly : {topicName} not mapped");

            base.RefreshSensors(new Dictionary<string, string>()
            {
                { DeviceData.DataTypeSensorTemperature, _lastTemp.ToString("0.00",  CultureInfo.InvariantCulture) },
                { DeviceData.DataTypeSensorHumidity, _lastHumidity.ToString("0.00",  CultureInfo.InvariantCulture) },
            });

        }
    }
}
