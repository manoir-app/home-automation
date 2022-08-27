using Home.Common.HomeAutomation;
using Home.Common.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace Home.Agents.Sarah.Devices.Hue
{
    public class HueLightSwitch : HueDeviceBase, IToggleSwitchDevice
    {
        internal HueLightSwitch(string deviceId, HueHelper.HueLight light) : base(deviceId, light)
        {
        }

        public DeviceData ChangeSwitchValue(string switchName, string switchNewValue)
        {
            if (string.IsNullOrEmpty(switchNewValue))
                throw new ArgumentNullException(nameof(switchName));

            var state = _light.state.on;
            if(switchName.Equals("on") && !string.IsNullOrEmpty(switchNewValue))
            {
                if (switchNewValue.Equals("on", StringComparison.InvariantCultureIgnoreCase))
                    state = true;
                else if (switchNewValue.Equals("off", StringComparison.InvariantCultureIgnoreCase))
                    state = false;
                else
                    throw new ArgumentException("New value invalid !", nameof(switchNewValue));
            }
            _light = HueHelper.SetLightStateRgb(_hueDeviceId, state, null, null);
            return ConvertOnOffState();
        }

        internal static DeviceData ChangeSwitchValue(HueDeviceBase device, string switchName, string switchNewValue)
        {
            if (string.IsNullOrEmpty(switchNewValue))
                throw new ArgumentNullException(nameof(switchName));

            var state = device._light.state.on;
            if (switchName.Equals("on") && !string.IsNullOrEmpty(switchNewValue))
            {
                if (switchNewValue.Equals("on", StringComparison.InvariantCultureIgnoreCase))
                    state = true;
                else if (switchNewValue.Equals("off", StringComparison.InvariantCultureIgnoreCase))
                    state = false;
                else
                    throw new ArgumentException("New value invalid !", nameof(switchNewValue));
            }
            device._light = HueHelper.SetLightStateRgb(device._hueDeviceId, state, null, null);
            return device.ConvertOnOffState();
        }

        public IEnumerable<DeviceData> GetSwitches()
        {
            var dvs = new List<DeviceData>();

            dvs.Add(ConvertOnOffState());

            return dvs;
        }

        protected internal override DeviceData ConvertOnOffState()
        {
            return new DeviceData()
            {
                IsMainData = true,
                Name = "on",
                StandardDataType = "switch",
                ValidValues = new string[] { "on", "off" },
                Value = _light.state.on ? "on" : "off"
            };
        }
    }
}
