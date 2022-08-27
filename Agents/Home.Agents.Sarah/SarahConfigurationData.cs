using System;
using System.Collections.Generic;
using System.Text;

namespace Home.Agents.Sarah
{
    public class SarahConfigurationData
    {
        public SarahConfigurationData()
        {
            DeviceFamilies = new List<string>();
        }

        public List<string> DeviceFamilies { get; set; }
    }
}
