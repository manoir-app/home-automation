using Home.Common.HomeAutomation;
using Home.Common.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Home.Agents.Sarah.Devices.Zigbee2Mqtt
{
    internal class Z2MqttPowerMetering : Z2MqttDeviceBase, IPowerMeteringDevice
    {
        public Z2MqttPowerMetering(string deviceIeeeID, string mqttPath) : base(deviceIeeeID, mqttPath)
        {
        }

        private decimal _lastTotal = decimal.MinValue;
        private decimal _lastInstant = decimal.MinValue;


        public IEnumerable<DeviceData> GetPowerStats()
        {
            var lst = new List<DeviceData>();

            if(_lastInstant!=decimal.MinValue)
            {
                lst.Add(new DeviceData()
                {
                    IsMainData = true,
                    Name = "TotalPower",
                    StandardDataType = DeviceData.DataTypePowerTotal,
                    Value = _lastTotal.ToString("0.00", CultureInfo.InvariantCulture)
                });
            }

            if (_lastInstant != decimal.MinValue)
            {
                lst.Add(new DeviceData()
                {
                    IsMainData = true,
                    Name = "CurrentConsumption",
                    StandardDataType = DeviceData.DataTypePowerCurrentConsumption,
                    Value = _lastTotal.ToString("0.00", CultureInfo.InvariantCulture)
                });
            }

            return lst;

        }

        internal override void RefreshFromConfig(Z2MqttHelper.DeviceInfo device)
        {
        }

        internal override void RefreshStatusFromTopic(Dictionary<string, object> values)
        {
        }
    }
}
