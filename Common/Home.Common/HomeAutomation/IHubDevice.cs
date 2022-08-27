using Home.Common.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace Home.Common.HomeAutomation
{
    public interface IHubDevice
    {
        IEnumerable<Device> GetDevices();
    }
}
