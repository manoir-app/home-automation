using Home.Agents.Sarah.Devices.Hue;
using Home.Common.HomeAutomation;
using Home.Common.Model;
using Home.Graph.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Home.Agents.Sarah.Devices.Hue.HueHelper;

namespace Home.Agents.Sarah.Devices.Zigbee2Mqtt
{
    internal class Z2MqttLightSwitch : Z2MqttDeviceBase, IToggleSwitchDevice
    {
        private bool _on = false;

        internal Z2MqttLightSwitch(string deviceIeeeID, string mqttPath) : base(deviceIeeeID, mqttPath)
        {
        }

        public DeviceData ChangeSwitchValue(string switchName, string switchNewValue)
        {
            if (string.IsNullOrEmpty(switchNewValue))
                throw new ArgumentNullException(nameof(switchName));

            var state = _on;
            if (switchName.Equals("on") && !string.IsNullOrEmpty(switchNewValue))
            {
                if (switchNewValue.Equals("on", StringComparison.InvariantCultureIgnoreCase))
                    state = true;
                else if (switchNewValue.Equals("off", StringComparison.InvariantCultureIgnoreCase))
                    state = false;
                else
                    throw new ArgumentException("New value invalid !", nameof(switchNewValue));
            }

            //_light = HueHelper.SetLightStateRgb(_hueDeviceId, state, null, null);
            MqttHelper.PublishJson(_mqttPath + "/set", "{\"state\":\"" + (state ? "on" : "off") + "\"}");
            _on = state;
            return ConvertOnOffState();
        }

        private DeviceData ConvertOnOffState()
        {
            return new DeviceData()
            {
                IsMainData = true,
                Name = "on",
                StandardDataType = "switch",
                ValidValues = new string[] { "on", "off" },
                Value = _on ? "on" : "off"
            };
        }

        public IEnumerable<DeviceData> GetSwitches()
        {
            var dvs = new List<DeviceData>();

            dvs.Add(ConvertOnOffState());

            return dvs;
        }

        internal override void RefreshFromConfig(Z2MqttHelper.DeviceInfo device)
        {

        }

        internal override void RefreshStatusFromTopic(Dictionary<string, object> values)
        {
            if (values.TryGetValue("state", out object stateObj))
            {
                string state = stateObj as string;

                if (state == null)
                    state = "off";
                switch (state.ToLowerInvariant())
                {
                    case "on":
                        _on = true;
                        break;
                    case "off":
                        _on = false;
                        break;
                }
            }

            base.RefreshLightOrSwitchState(_on, null, null);
        }
    }
}
