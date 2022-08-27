using Home.Common.HomeAutomation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Home.Agents.Sarah
{
    partial class DeviceManager
    {
        public static void ReloadConfig()
        {
           var newList = new List<DeviceBase>();

            AllDevices = newList;
        }
    }
}
