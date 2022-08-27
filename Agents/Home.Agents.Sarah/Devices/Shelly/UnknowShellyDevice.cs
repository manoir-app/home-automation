using System;
using System.Collections.Generic;
using System.Text;

namespace Home.Agents.Sarah.Devices.Shelly
{
    public class UnknowShellyDevice : ShellyDeviceBase
    {
        public UnknowShellyDevice(string ipv4, string deviceId) : base(ipv4, deviceId)
        {
        }

        public override List<string> GetRoles()
        {
            var ret = base.GetRoles();
            ret.Add("unknown");
            return ret;
        }
    }
}
