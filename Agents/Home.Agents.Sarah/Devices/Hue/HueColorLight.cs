using Emby.ApiClient;
using Emby.ApiClient.Model;
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
            var tmp = new DeviceData()
            {
                MinValue = "1",
                MaxValue = "100",
                Name = "color",
                StandardDataType = DeviceData.DataTypeColor,
                IsMainData = true,
            };
            switch (_light.state.colormode)
            {
                default: // (x,y)
                    _light = HueHelper.SetLightStateRgb(_hueDeviceId, null, null, newColor);
                    tmp.Value = HueHelper.FromXyz(_light.state.xy).ToString();
                    break;
            }
            return tmp;
        }
        public IEnumerable<DeviceData> GetColors()
        {
            throw new NotImplementedException();
        }

    }
}
