using System;
using System.Collections.Generic;
using System.Text;

namespace Home.Common.HomeAutomation
{
    public enum AutoSetupDeviceStatus
    {
        NewDevice,
        InitialSetupDone,
        FullySetUp
    }
    public interface IAutoSetupDevice
    {
        AutoSetupDeviceStatus GetSetupStatus();
        bool InitialSetup();
        bool Setup(Model.Device deviceData);
    }
}
