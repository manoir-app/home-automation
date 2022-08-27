using Home.Common.HomeAutomation;
using Home.Common.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Home.Agents.Sarah.Devices.Hue
{
    public class HueColorLight : HueLightSwitch, IColorBoundDevice
    {
        internal HueColorLight(string deviceId, HueHelper.HueLight light) : base(deviceId, light)
        {
        }

        public DeviceData ChangeColor(string elementName, Color newColor)
        {
            return null;
        }
        public IEnumerable<DeviceData> GetColors()
        {
            throw new NotImplementedException();
        }

    }
}
