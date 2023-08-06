using Home.Common.HomeAutomation;
using Home.Common.Model;
using System.Collections.Generic;

namespace Home.Agents.Sarah.Devices.Shelly
{
    internal class ShellyI3CommandDevice : ShellyDeviceBase, IActionButton
    {
        public ShellyI3CommandDevice(string ipv4, string deviceId) : base(ipv4, deviceId)
        {
        }

        public DeviceActionnable Configure(string name, DeviceActionnableActionType type, string parameter)
        {
            return null;
        }

        public IEnumerable<DeviceActionnable> GetActionables()
        {
            return new DeviceActionnable[]
            {
                new DeviceActionnable
                {
                    ActionType = DeviceActionnableActionType.None,
                    StandardActionnableType = DeviceActionnable.ActionnableTypePushButton,
                    Name = "1"
                },
                new DeviceActionnable
                {
                    ActionType = DeviceActionnableActionType.None,
                    StandardActionnableType = DeviceActionnable.ActionnableTypePushButton,
                    Name = "2"
                },
                new DeviceActionnable
                {
                    ActionType = DeviceActionnableActionType.None,
                    StandardActionnableType = DeviceActionnable.ActionnableTypePushButton,
                    Name = "3"
                }
            };
        }
    }



}
