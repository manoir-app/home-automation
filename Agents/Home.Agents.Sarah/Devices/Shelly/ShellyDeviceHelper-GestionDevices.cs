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
    partial class ShellyDeviceHelper
    {
        private static string _root = "shellies";

        public static bool _stop = false;
        public static void Start()
        {
            string pth = _root;
            if (!pth.EndsWith("/"))
                pth = pth + "/";
            pth = pth + "#";

            MqttHelper.AddChangeHandler(pth, HandleChange);


            var t = new Thread(() => ShellyDeviceHelper.Run());
            t.Name = "Shelly devices";
            t.Start();



        }

        private static void Run()
        {
            while (!_stop)
            {
                try
                {
                    Thread.Sleep(10000);

                    Thread.Sleep(20000);
                }
                catch (Exception ex)
                {
                    Console.Write("Shelly - ERR in ShellyThread - ");
                    Console.WriteLine(ex.ToString());
                }
            }
        }

        public static void Stop()
        {
            _stop = true;

            string pth = _root;
            if (!pth.EndsWith("/"))
                pth = pth + "/";
            pth = pth + "#";
            MqttHelper.RemoveChangeHandler(pth, HandleChange);

        }


        private static void HandleChange(string topic, string data)
        {
            string subPath = topic;
            if (subPath.StartsWith(_root))
                subPath = subPath.Substring(_root.Length);
            if (subPath.StartsWith("/"))
                subPath = subPath.Substring(1);

            if (subPath.Equals("announce", StringComparison.InvariantCultureIgnoreCase))
            {
                // on regarde si le device existe, sinon on
                // va le créer pour une prochaine execution
                // et on essaie de le configurer
                var announce = JsonConvert.DeserializeObject<MqttAnnounce>(data);
                if (announce != null)
                {
                    var dev = (from z in _allDevices.Values
                               where z.DeviceId.Equals(announce.id, StringComparison.InvariantCultureIgnoreCase)
                               select z).FirstOrDefault();
                    if (dev == null)
                    {
                        // identifions le modèle pour savoir si c'est
                        // un device tout le temps connecté ou non
                        var mdl = IdentifyModel(announce.model);
                        if (mdl != null)
                        {
                            if (mdl.AlwaysConnected.GetValueOrDefault(false))
                            {
                                dev = GetDeviceBase(announce.ip) as ShellyDeviceBase;
                            }
                            else
                            {
                                dev = InstanciateDevice(announce.ip, announce.model, announce.id, null) as ShellyDeviceBase;
                            }

                            if(dev!=null)
                            {
                                List<Device> reg = new List<Device>();
                                List<ShellyDeviceBase> conf = new List<ShellyDeviceBase>();
                                Add(reg, conf, dev);
                                RegisterDevices(reg, conf);
                            }
                        }
                    }
                }
            }

            if (subPath.Contains("/")) // très probablement un device
            {
                var devId = subPath.Substring(0, subPath.IndexOf("/"));
                foreach (var dev in _allDevices.Values)
                {
                    if (dev.DeviceId.Equals(devId, StringComparison.InvariantCultureIgnoreCase))
                    {
                        subPath = subPath.Substring(subPath.IndexOf("/") + 1);
                        dev.RefreshFromMqttTopic(subPath, data);
                    }
                }
            }
        }


        private static Dictionary<string, ShellyDeviceBase> _allDevices = new Dictionary<string, ShellyDeviceBase>();


        internal static DeviceBase[] GetDevices(NetworkDeviceEnumerateMessageResponse alldevs)
        {
            if (!alldevs.Response.Equals("ok", StringComparison.InvariantCultureIgnoreCase))
                return null;

            List<DeviceBase> ret = new List<DeviceBase>();

            foreach (var dev in alldevs.ActiveDevices)
            {
                if (dev.Vendor != null && dev.Vendor.Contains("Espressif", StringComparison.InvariantCultureIgnoreCase))
                {
                    //Console.WriteLine($"Shelly - Found {dev.IpV4} as a potential shelly device");

                    var dvb = GetDeviceBase(dev);
                    if (dvb != null)
                    {
                        //Console.WriteLine($"Shelly - Adding {dev.IpV4} as shelly device : {dvb.DeviceName}");
                        ret.Add(dvb);
                    }
                }
            }

            var toRegister = new List<Device>();
            var toConfigure = new List<ShellyDeviceBase>();
            foreach (ShellyDeviceBase r in ret)
            {
                Add(toRegister, toConfigure, r);
            }

            if (toRegister != null && toRegister.Count > 0)
            {
                foreach (var r in toRegister)
                {
                    try
                    {
                        var settings = ShellyDeviceHelper.GetSettings(r.DeviceAddresses[0]);
                        if (!string.IsNullOrEmpty(settings.name))
                            r.DeviceGivenName = settings.name;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Shelly - Registering devices - get name failed with : {ex.Message}");
                    }
                }
            
                RegisterDevices(toRegister, toConfigure);
            }

            var t = _allDevices.Values.ToArray();
            foreach (ShellyDeviceBase r in t)
            {
                if (r != null && r.DeviceName != null)
                {
                    var detected = (from z in ret
                                    where z.DeviceName.Equals(r.DeviceName)
                                    select z).FirstOrDefault();
                    if (detected == null)
                    {
                        _allDevices.Remove(r.DeviceName);
                    }
                }
                else if (r.DeviceName == null)
                    _allDevices.Remove(r.DeviceName);
            }

            return ret.ToArray();
        }

        private static void RegisterDevices(List<Device> toRegister, List<ShellyDeviceBase> toConfigure)
        {
            Console.WriteLine($"Shelly - Registering {toRegister.Count} shelly devices");

            if (toRegister != null && toRegister.Count > 0)
            {
                RegisterDevice(toRegister.ToArray());
                ThreadPool.QueueUserWorkItem((o) =>
                {
                    foreach (var dev in toConfigure)
                    {
                        try
                        {
                            if (dev is IAutoSetupDevice)
                            {
                                var sw = dev as Common.HomeAutomation.IAutoSetupDevice;
                                var resSetup = sw.GetSetupStatus();
                                if (resSetup == Common.HomeAutomation.AutoSetupDeviceStatus.NewDevice)
                                {
                                    Console.WriteLine($"Shelly - {dev.DeviceId} is a new device !");
                                    sw.InitialSetup();
                                    Console.WriteLine($"Shelly - {dev.DeviceId} configured !");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Shelly - {dev.DeviceId} error in autoconfig : " + ex);
                        }
                    }
                });
            }
        }

        private static void Add(List<Device> toRegister, List<ShellyDeviceBase> toConfigure, ShellyDeviceBase r)
        {
            ShellyDeviceBase already = null;
            if (!_allDevices.TryGetValue(r.DeviceName, out already))
            {
                _allDevices.Add(r.DeviceName, r);
                var devreg = new Device(r);
                devreg.DeviceRoles.Add("shelly");
                devreg.DeviceAddresses.Add(r.IpV4);
                devreg.DeviceGivenName = r.DeviceName;
                toRegister.Add(devreg);
                toConfigure.Add(r);
            }
            else
            {


                if (already.IpV4 != r.IpV4)
                {
                    already.IpV4 = r.IpV4;
                }
            }
        }

        private static void RegisterDevice(params Device[] devices)
        {
            foreach (var dev in devices)
                dev.DevicePlatform = "shelly";

            for (int i = 0; i < 3; i++)
            {
                try
                {
                    using (var cli = new MainApiAgentWebClient("sarah"))
                    {
                        var exts = cli.UploadData<List<Device>, Device[]>(
                            $"/v1.0/devices/register/sarah",
                            "POST", devices);
                        break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }

            }
        }

        private static DeviceBase GetDeviceBase(NetworkDeviceData dev)
        {
            string ipV4 = dev.IpV4;
            return GetDeviceBase(ipV4);
        }

        internal static DeviceBase GetDeviceBase(string ipV4)
        {
            var sdi = GetDeviceInfo(ipV4);

            if (sdi == null)
            {
                //Console.WriteLine($"Shelly - {ipV4} is not a shelly device");
                return null;
            }
            return GetDeviceBase(sdi, ipV4);
        }

        private static bool IsPermanentConnectModel(string model)
        {
            if (model == null)
                return false;
            if (_models.TryGetValue(model, out var result))
                return result.AlwaysConnected.GetValueOrDefault(false);

            return false;
        }

        private static ShellyModel IdentifyModel(string model)
        {
            if (model == null)
                return null;
            if (_models.TryGetValue(model, out var result))
                return result;
            return null;
        }

        private static Dictionary<string, ShellyModel> _models = new Dictionary<string, ShellyModel>()
        {
            { "SHBTN-1", new ShellyModel() {Name = "Shelly Button 1", Type=typeof(ShellyButton1), AlwaysConnected = null } },
            { "SHPLG-S", new ShellyModel() {Name = "Shelly Plug S", Type=typeof(ShellySimpleSwitch), AlwaysConnected = true } },
            { "SHSW-25", new ShellyModel() {Name = "Shelly Plug S", Type=null, AlwaysConnected = true } },
            { "SHHT-1", new ShellyModel() {Name = "Shelly Temp & Humidity Sensor", Type=null, AlwaysConnected = false } },
        };

        private class ShellyModel
        {
            public string Name { get; set; }
            public Type Type { get; set; }

            public bool? AlwaysConnected { get; set; }
        }


        private static DeviceBase GetDeviceBase(GetShellyDeviceInfo sdi, string ipv4)
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

        private static DeviceBase InstanciateDevice(string ipv4, string model, string hostname, string mode)
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
