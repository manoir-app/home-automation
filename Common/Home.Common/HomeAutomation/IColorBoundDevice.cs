using Home.Common.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Home.Common.HomeAutomation
{
    public interface IColorBoundDevice
    {
        IEnumerable<DeviceData> GetColors();

        DeviceData ChangeColor(string elementName, string instanceId, Color newColor);
    }
}
