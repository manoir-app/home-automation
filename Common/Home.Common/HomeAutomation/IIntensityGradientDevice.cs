using Home.Common.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace Home.Common.HomeAutomation
{
    public interface IIntensityGradientDevice
    {
        DeviceData ChangeIntensity(string elementName, string instanceId, decimal intensity);
        IEnumerable<DeviceData> GetIntensities();

    }
}
