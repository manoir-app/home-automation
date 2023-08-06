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
    partial class ShellyDeviceHelperGen1
    {
        internal static DeviceBase GetDeviceBase(ShellyInfoDataObjectGen1 sdi, string ipv4)
        {
            if (ipv4 == null || sdi == null)
                return null;

            var tmp = GetSettings<Shelly25DeviceBase.GetShelly25SettingsResponse>(ipv4);
            if (tmp == null)
            {
                Console.WriteLine($"Shelly - {ipv4} did not respond to get settings");
                return new UnknowShellyDevice(ipv4, sdi.mac);
            }

            Console.WriteLine($"{ipv4} is a {sdi.type}");

            string model = sdi.type?.ToUpperInvariant();
            string hostname = tmp.device.hostname;
            string mode = tmp.mode;

            return InstanciateDevice(ipv4, model, hostname, mode);
        }

        internal static DeviceBase InstanciateDevice(string ipv4, string model, string hostname, string mode)
        {
            // pour l'instant, on instancie "en dur"
            // TODO : faire propre dans ce coin ci :)
            switch (model)
            {
                case "SHHT-1":
                    return new ShellyTemperatureSensor(ipv4, hostname);
                case "SHBTN-1":
                    return new ShellyButton1(ipv4, hostname);
                case "SHPLG-S":
                    return new ShellySimpleSwitch(ipv4, hostname);
                case "SHSW-25":
                    if (mode == null)
                    {
                        Console.WriteLine($"{ipv4} is a {model} and should have a mode");
                        return new UnknowShellyDevice(ipv4, hostname);
                    }
                    switch (mode.ToLowerInvariant())
                    {
                        case "roller":
                        case "shutter":
                            return new Shelly25Roller(ipv4, hostname);
                        case "relay":
                            return new Shelly25MultiRelay(ipv4, hostname);
                        default:
                            return new UnknowShellyDevice(ipv4, hostname);
                    }
            }

            return null;
        }
    }
}
