using Home.Common;
using Home.Common.HomeAutomation;
using Home.Common.Messages;
using Home.Common.Model;
using Home.Graph.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

namespace Home.Agents.Sarah.Devices.Shelly
{
    public class ShellyDeviceBase : DeviceBase, IAutoSetupDevice
    {
        public override string DeviceName
        {
            get
            {
                return DeviceId;
            }
        }

        public string DevicePublicName { get; internal set; }

        public string IpV4 { get; internal set; }
        public string DeviceId { get; private set; }

        protected ShellyDeviceBase(string ipv4, string deviceId) : base()
        {
            this.IpV4 = ipv4;
            this.DeviceId = deviceId;
        }

        protected internal virtual void RefreshFromMqttTopic(string topicName, string value)
        {
            if (topicName.Equals("relay/0/energy"))
            {
                UpdateInTimeDb(new Dictionary<string, string>()
                {
                    {"power_consumption_total", value}
                });
            }
            else if (topicName.Equals("relay/energy"))
            {
                UpdateInTimeDb(new Dictionary<string, string>()
                {
                    {"power_consumption_total", value}
                });
            }
        }


        internal void RefreshSensors(Dictionary<string, string> values)
        {
            UpdateInTimeDb(values);

            var changed = new List<DeviceStateChangedMessage.DeviceStateValue>();
            foreach (var k in values.Keys)
            {
                changed.Add(new DeviceStateChangedMessage.DeviceStateValue() { Name = k, Value = values[k] });
            }
            Console.WriteLine($"Shelly - changing properties {string.Join(',', (from z in changed select z.Name).ToArray())}");
            DeviceManager.OnDeviceStateChanged("Shelly", DeviceName, Device.HomeAutomationMainRoleSensors, "", changed.ToArray());
        }

        protected void UpdateInTimeDb(Dictionary<string, string> values)
        {
            foreach (var k in values.Keys)
            {

                try
                {
                    // on push en timedb
                    if (decimal.TryParse(values[k], System.Globalization.NumberStyles.Number, CultureInfo.InvariantCulture, out decimal val))
                    {
                        TimeDBHelper.Trace("home", "devices", k, val, new Dictionary<string, string>()
                        {
                            {"deviceId", DeviceId},
                            {"roomId", ""},
                            {"locationId", ""},
                        });
                    }
                    else if (decimal.TryParse(values[k], out decimal val2))
                    {
                        TimeDBHelper.Trace("home", "devices", k, val2, new Dictionary<string, string>()
                        {
                            {"deviceId", DeviceId},
                            {"roomId", ""},
                            {"locationId", ""},
                        });
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Err updating timedb for Shelly : " + ex);
                }
            }
        }


        public void ApplyStandardConfig()
        {
            //var sdi = ShellyGen1DeviceHelper.GetDeviceInfo(this.IpV4);
            //if (!sdi.auth)
            //{

            //}
        }


        public AutoSetupDeviceStatus GetSetupStatus()
        {
            var t = ShellyDeviceHelper.GetDeviceInfo(this.IpV4);
            if (t == null)
                return AutoSetupDeviceStatus.NewDevice; // si le device ne répond plus...

            if(t is ShellyInfoDataObjectGen1)
                return GetSetupGen1Device(t as ShellyInfoDataObjectGen1);

            return AutoSetupDeviceStatus.FullySetUp;
        }

        private AutoSetupDeviceStatus GetSetupGen1Device(ShellyInfoDataObjectGen1 t)
        {
            if (!t.auth)
                return AutoSetupDeviceStatus.NewDevice;
            var stl = ShellyDeviceHelperGen1.GetLoginSettings(this.IpV4);
            if (stl == null)
                return AutoSetupDeviceStatus.NewDevice;
            if (!stl.enabled)
                return AutoSetupDeviceStatus.NewDevice;

            var st = ShellyDeviceHelperGen1.GetSettings(this.IpV4);
            if (st == null)
                return AutoSetupDeviceStatus.NewDevice;
            if (st.mqtt == null || !st.mqtt.enable)
                return AutoSetupDeviceStatus.NewDevice;


            return AutoSetupDeviceStatus.FullySetUp;
        }

        
        public bool InitialSetup()
        {
            try
            {
                var t = ShellyDeviceHelper.GetDeviceInfo(this.IpV4);
                if (t == null)
                    return false; // le device ne répond plus...

                if (t is ShellyInfoDataObjectGen1)
                {
                    var tGen1 = t as ShellyInfoDataObjectGen1;
                    if (!tGen1.auth)
                        ShellyDeviceHelperGen1.SetupLogin(IpV4);

                    var st = ShellyDeviceHelperGen1.GetSettings(this.IpV4);
                    if (st == null)
                        return false; // le device ne répond plus...

                    ShellyDeviceHelperGen1.SetMqtt(IpV4);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool Setup(Device deviceData)
        {
            return false;
        }


        

    }
}