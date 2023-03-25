using Home.Common.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace Home.Common.HomeAutomation
{
    public abstract class DeviceBase
    {
        public abstract string DeviceName { get; }

        public virtual List<string> GetRoles()
        {
            List<string> roles = new List<string>();    
            if (this is IActionButton)
                roles.Add(Device.HomeAutomationRoleActionnable);

            if (this is IToggleSwitchDevice)
                roles.Add(Device.HomeAutomationRoleSwitch);

            if (this is IColorBoundDevice)
                roles.Add(Device.HomeAutomationRoleColorBound);

            if (this is IIntensityGradientDevice)
                roles.Add(Device.HomeAutomationRoleDimmer);

            return roles;
        }


        public List<DeviceData> SecondaryProperties { get; } = new List<DeviceData>();

    }
}
