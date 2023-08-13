using Home.Common;
using Home.Common.HomeAutomation;
using Home.Common.Messages;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Home.Common.Model;
using System.Threading;
using Home.Graph.Common;

namespace Home.Agents.Sarah.Devices.Shelly
{
    partial class ShellyDeviceHelperGen2
    {
        internal static DeviceBase GetDeviceBase(ShellyInfoDataObjectGen2 sdi, string ipv4)
        {
            if (ipv4 == null || sdi == null)
                return null;

            //var tmp = GetSettings<Shelly25DeviceBase.GetShelly25SettingsResponse>(ipv4);
            //if (tmp == null)
            //{
            //    Console.WriteLine($"Shelly - {ipv4} did not respond to get settings");
            //    return new UnknowShellyDevice(ipv4, sdi.mac);
            //}

            Console.WriteLine($"{ipv4} is a {sdi.app}");

            string model = sdi.app?.ToUpperInvariant();
            string hostname = sdi.name;

            return InstanciateDevice(ipv4, model, hostname);
        }

        internal static DeviceBase InstanciateDevice(string ipv4, string model, string hostname)
        {
            switch (model)
            {
                case "Pro1PM":
                    return new ShellyProDinSwitchDevice(ipv4, hostname, 1);
            }

            return null;
        }
    }
}
