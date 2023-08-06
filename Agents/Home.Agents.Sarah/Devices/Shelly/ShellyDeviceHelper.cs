using Home.Common;
using Home.Common.HomeAutomation;
using Home.Common.Messages;
using Home.Common.Model;
using Home.Graph.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using static Home.Agents.Sarah.Devices.Shelly.ShellyDeviceHelperGen1;

namespace Home.Agents.Sarah.Devices.Shelly
{
    public static partial class ShellyDeviceHelper
    {


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
                        var settings = ShellyDeviceHelperGen1.GetSettings(r.DeviceAddresses[0]);
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


        public static void HandleChange(string topic, string data)
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
                                switch(mdl.Gen)
                                {
                                    case 1:
                                        dev = ShellyDeviceHelperGen1.InstanciateDevice(announce.ip, announce.model, announce.id, null) as ShellyDeviceBase;
                                        break;
                                }
                                
                            }

                            if (dev != null)
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
            { "SHBTN-1", new ShellyModel() {Name = "Shelly Button 1", Type=typeof(ShellyButton1), AlwaysConnected = null, Gen = 1 } },
            { "SHPLG-S", new ShellyModel() {Name = "Shelly Plug S", Type=typeof(ShellySimpleSwitch), AlwaysConnected = true , Gen = 1} },
            { "SHSW-25", new ShellyModel() {Name = "Shelly Plug S", Type=null, AlwaysConnected = true, Gen = 1 } },
            { "SHHT-1", new ShellyModel() {Name = "Shelly Temp & Humidity Sensor", Type=null, AlwaysConnected = false, Gen = 1 } },
        };

        private class ShellyModel
        {
            public string Name { get; set; }
            public Type Type { get; set; }

            public bool? AlwaysConnected { get; set; }

            public int Gen { get; set; }
        }


        internal static ShellyInfoDataObject GetDeviceInfo(string ipV4)
        {
            using (WebClient cli = new WebClient())
            {
                string url = $"http://{ipV4}/shelly";
                try
                {
                    string json = cli.DownloadString(url);
                    return ShellyInfoDataObject.FromJson(json);
                }
                catch (WebException)
                {
                    return null;
                }
            }
        }

        internal static DeviceBase GetDeviceBase(NetworkDeviceData dev)
        {
            string ipV4 = dev.IpV4;
            return GetDeviceBase(ipV4);
        }

        internal static DeviceBase GetDeviceBase(string ipV4)
        {
            var sdi = ShellyDeviceHelper.GetDeviceInfo(ipV4);

            if (sdi == null)
            {
                //Console.WriteLine($"Shelly - {ipV4} is not a shelly device");
                return null;
            }

            if (sdi is ShellyInfoDataObjectGen1)
                return ShellyDeviceHelperGen1.GetDeviceBase(sdi as ShellyInfoDataObjectGen1, ipV4);


            return null;

        }

    }
}
