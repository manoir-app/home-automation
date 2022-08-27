using Home.Common;
using Home.Common.HomeAutomation;
using Home.Common.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace Home.Agents.Sarah.Devices.Divoom
{
    public abstract class BaseDivoomDevice : DisplayDeviceBase
    {

        // http://doc.divoom-gz.com/web/#/12?page_id=89

        private string _name = null;
        public override string DeviceName
        {
            get
            {
                return _name;
            }
        }

        public string IpV4 { get; internal set; }

        public BaseDivoomDevice(string name, string ipV4)
        {
            _name = name;
            IpV4 = ipV4;
        }
    }
}
