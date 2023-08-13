using Home.Common.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace Home.Common.HomeAutomation
{
    public interface IToggleSwitchDevice
    {
        IEnumerable<DeviceData> GetSwitches();

        DeviceData ChangeSwitchValue(string switchName, string instanceId, string switchNewValue);
    }
}
