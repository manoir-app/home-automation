using Home.Common.HomeAutomation;
using Home.Common.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace Home.Agents.Sarah.Devices.Hue
{
    public class HueSimpleLight : HueLightSwitch, IIntensityGradientDevice
    {
        internal HueSimpleLight(string deviceId, HueHelper.HueLight light) : base(deviceId, light)
        {
        }

        public DeviceData ChangeIntensity(string elementName, decimal intensity)
        {
            if(intensity>=1M)
                intensity = Math.Ceiling(intensity * 2.54M);
            
            _light = HueHelper.SetLightStateRgb(_hueDeviceId, intensity>=1M, (short)intensity, null);
            return new DeviceData()
            {
                MinValue = "1",
                MaxValue = "100",
                Value = Math.Ceiling(_light.state.bri / 254.0M).ToString("0"),
                Name = "brightness",
                StandardDataType = DeviceData.DataTypeGradient,
                IsMainData = true
            };

        }

        public IEnumerable<DeviceData> GetIntensities()
        {
            var dvs = new List<DeviceData>();

            dvs.Add(new DeviceData()
            {

                MinValue = "1",
                MaxValue = "100",
                Value = Math.Ceiling(_light.state.bri / 254.0M).ToString("0"),
                Name = "brightness",
                StandardDataType = DeviceData.DataTypeGradient,
                IsMainData = true
            });

            return dvs;
        }
    }
}
