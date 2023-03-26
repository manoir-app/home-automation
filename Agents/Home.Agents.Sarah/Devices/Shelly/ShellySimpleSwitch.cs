using Home.Common.HomeAutomation;
using Home.Common.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;

namespace Home.Agents.Sarah.Devices.Shelly
{
    public class ShellySimpleSwitch : ShellyDeviceBase, IToggleSwitchDevice
    {
        public ShellySimpleSwitch(string ipv4, string deviceId) : base(ipv4, deviceId)
        {
        }

        public DeviceData ChangeSwitchValue(string switchName, string switchNewValue)
        {
            if (string.IsNullOrEmpty(switchNewValue))
                throw new ArgumentNullException(nameof(switchName));

            var state = true;
            if (switchName.Equals("on") && !string.IsNullOrEmpty(switchNewValue))
            {
                if (switchNewValue.Equals("on", StringComparison.InvariantCultureIgnoreCase))
                    state = true;
                else if (switchNewValue.Equals("off", StringComparison.InvariantCultureIgnoreCase))
                    state = false;
                else
                    throw new ArgumentException("New value invalid !", nameof(switchNewValue));
            }

            var t = SetRelayStatus(state, null);
            if (t != null)
                return ConvertOnOffState(t);
            else
            {
                t = GetRelayStatus();
                if (t == null)
                    throw new ApplicationException("Device inaccessible");
                return ConvertOnOffState(t);
            }
        }

        public IEnumerable<DeviceData> GetSwitches()
        {
            var dvs = new List<DeviceData>();

            var t = GetRelayStatus();
            if (t == null)
                throw new ApplicationException("Device inaccessible");

            dvs.Add(ConvertOnOffState(t));

            return dvs;
        }

        private GetRelayResponse SetRelayStatus(bool isOn, int? flipBackTimerInSeconds)
        {
            string url = $"http://{IpV4}/relay/0?turn=";
            url += isOn ? "on" : "off";
            if (flipBackTimerInSeconds.HasValue)
                url += $"&timer={flipBackTimerInSeconds.Value.ToString("0")}";
            using (var cli = new ShellyWebClient())
            {
                try
                {
                    string json = cli.DownloadString(url);
                    var sdi = JsonConvert.DeserializeObject<GetRelayResponse>(json);
                    return sdi;
                }
                catch (WebException)
                {
                    return null;
                }
            }
        }


       

        private GetRelayResponse GetRelayStatus()
        {
            string url = $"http://{IpV4}/relay/0";
            using (var cli = new ShellyWebClient())
            {
                try
                {
                    string json = cli.DownloadString(url);
                    var sdi = JsonConvert.DeserializeObject<GetRelayResponse>(json);
                    return sdi;
                }
                catch (WebException)
                {
                    return null;
                }
            }
        }


        public class GetRelayResponse
        {
            public bool ison { get; set; }
            public bool has_timer { get; set; }
            public int timer_started { get; set; }
            public int timer_duration { get; set; }
            public int timer_remaining { get; set; }
            public bool overpower { get; set; }
            public string source { get; set; }
        }

        private GetMeterResponse GetMeter()
        {
            string url = $"http://{IpV4}/meter/0";
            using (var cli = new WebClient())
            {
                try
                {
                    string json = cli.DownloadString(url);
                    var sdi = JsonConvert.DeserializeObject<GetMeterResponse>(json);
                    return sdi;
                }
                catch (WebException)
                {
                    return null;
                }
            }
        }

        protected DeviceData ConvertOnOffState(GetRelayResponse resp)
        {
            return new DeviceData()
            {
                IsMainData = true,
                Name = "on",
                StandardDataType = "switch",
                ValidValues = new string[] { "on", "off" },
                Value = resp.ison ? "on" : "off"
            };
        }

        public class GetMeterResponse
        {
            public int power { get; set; }
            public float overpower { get; set; }
            public bool is_valid { get; set; }
            public int timestamp { get; set; }
            public int[] counters { get; set; }
            public int total { get; set; }
        }


    }
}
