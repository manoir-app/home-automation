using Home.Common.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace Home.Common.HomeAutomation
{
    
    public interface IActionButton
    {
        IEnumerable<DeviceActionnable> GetActionables();

        DeviceActionnable Configure(string name, string instanceId, DeviceActionnableActionType type, string parameter);
    }
}
