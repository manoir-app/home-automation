using Home.Common.HomeAutomation;
using Home.Common.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Home.Agents.Sarah.Devices.Shelly
{
    public class ShellyProDinSwitchDevice : ShellyDeviceBase, IToggleSwitchDevice
    {
        private int _switchCount = 1;

        public ShellyProDinSwitchDevice(string ipv4, string deviceId, int switchCount) : base(ipv4, deviceId)
        {
            _switchCount = switchCount;
        }

        public DeviceData ChangeSwitchValue(string switchName, string instanceId, string switchNewValue)
        {
            if (string.IsNullOrEmpty(switchNewValue))
                throw new ArgumentNullException(nameof(switchName));

            if (instanceId == null)
                instanceId = "0";

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

            var t = SetRelayStatus(0,state, null);
            if (t != null)
                return ConvertOnOffState(0,t);
            else
            {
                t = GetRelayStatus(0);
                if (t == null)
                    throw new ApplicationException("Device inaccessible");
                return ConvertOnOffState(0, t);
            }
        }

        public IEnumerable<DeviceData> GetSwitches()
        {
            var dvs = new List<DeviceData>();

            var t = GetRelayStatus(0);
            if (t == null)
                throw new ApplicationException("Device inaccessible");

            dvs.Add(ConvertOnOffState(0, t));

            return dvs;
        }

        private GetRelayResponse SetRelayStatus(int relayId, bool isOn, int? flipBackTimerInSeconds)
        {
            string url = $"http://{IpV4}/relay/{relayId}?turn=";
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




        private GetRelayResponse GetRelayStatus(int relayId)
        {
            string url = $"http://{IpV4}/relay/{relayId}";
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

        private GetMeterResponse GetMeter(int relayId)
        {
            string url = $"http://{IpV4}/meter/{relayId}";
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

        protected DeviceData ConvertOnOffState(int relayId, GetRelayResponse resp)
        {
            return new DeviceData()
            {
                IsMainData = true,
                InstanceId = relayId.ToString(),
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
