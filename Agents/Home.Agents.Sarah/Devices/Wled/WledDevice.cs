using Home.Common.HomeAutomation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Home.Agents.Sarah.Devices.Wled
{
    public class WledDevice : DeviceBase
    {
        private WledHelper.GetWledDeviceInfo sdi;
        private string ipV4;

        public string IpV4 {  get { return ipV4; } set { ipV4 = value; } }


        internal WledDevice(WledHelper.GetWledDeviceInfo sdi, string ipV4)
        {
            this.sdi = sdi;
            this.ipV4 = ipV4;
        }

        public override string DeviceName
        {
            get
            {
                return "wled-" + sdi.mac;
            }
        }
    }
}
