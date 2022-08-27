using Home.Common.HomeAutomation;
using Home.Common.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Home.Agents.Sarah.Devices.Shelly
{
    public abstract class Shelly25DeviceBase : ShellyDeviceBase
    {
        protected Shelly25DeviceBase(string ipv4, string deviceId) : base(ipv4, deviceId)
        {

        }

        public class GetShelly25StatusResponse : ShellyDeviceHelper.GetStatusResponse
        {
            public GetShelly25StatusRelay[] relays { get; set; }
            public GetShelly25StatusMeter[] meters { get; set; }
            public GetShelly25StatusInput[] inputs { get; set; }
            public float temperature { get; set; }
            public bool overtemperature { get; set; }
            public GetShelly25StatusTemperature tmp { get; set; }
            public string temperature_status { get; set; }
        }

        public class GetShelly25StatusTemperature
        {
            public float tC { get; set; }
            public float tF { get; set; }
            public bool is_valid { get; set; }
        }

        public class GetShelly25StatusRelay
        {
            public bool ison { get; set; }
            public bool has_timer { get; set; }
            public int timer_started { get; set; }
            public int timer_duration { get; set; }
            public int timer_remaining { get; set; }
            public bool overpower { get; set; }
            public bool overtemperature { get; set; }
            public bool is_valid { get; set; }
            public string source { get; set; }
        }

        public class GetShelly25StatusMeter
        {
            public decimal power { get; set; }
            public float overpower { get; set; }
            public bool is_valid { get; set; }
            public int timestamp { get; set; }
            public int[] counters { get; set; }
            public int total { get; set; }
        }

        public class GetShelly25StatusInput
        {
            public int input { get; set; }
            public string _event { get; set; }
            public int event_cnt { get; set; }
        }


        public class GetShelly25SettingsResponse : ShellyDeviceHelper.GetSettingsResponse
        {
            public bool factory_reset_from_switch { get; set; }
            public string mode { get; set; }
            public decimal max_power { get; set; }
            public int longpush_time { get; set; }
            public bool led_status_disable { get; set; }
            public GetShelly25SettingsActions actions { get; set; }
            public GetShelly25SettingsRelay[] relays { get; set; }
            public GetShelly25SettingsRoller[] rollers { get; set; }
            public bool favorites_enabled { get; set; }
            public GetShelly25SettingsFavorite[] favorites { get; set; }
        }

        public class GetShelly25SettingsActions
        {
            public bool active { get; set; }
            public string[] names { get; set; }
        }

        public class GetShelly25SettingsRelay
        {
            public object name { get; set; }
            public string appliance_type { get; set; }
            public bool ison { get; set; }
            public bool has_timer { get; set; }
            public bool overpower { get; set; }
            public string default_state { get; set; }
            public string btn_type { get; set; }
            public int btn_reverse { get; set; }
            public decimal auto_on { get; set; }
            public decimal auto_off { get; set; }
            public decimal max_power { get; set; }
            public bool schedule { get; set; }
            public object[] schedule_rules { get; set; }
        }

        public class GetShelly25SettingsRoller
        {
            public decimal maxtime { get; set; }
            public decimal maxtime_open { get; set; }
            public decimal maxtime_close { get; set; }
            public string default_state { get; set; }
            public bool swap { get; set; }
            public bool swap_inputs { get; set; }
            public string input_mode { get; set; }
            public string button_type { get; set; }
            public int btn_reverse { get; set; }
            public string state { get; set; }
            public decimal power { get; set; }
            public bool is_valid { get; set; }
            public bool safety_switch { get; set; }
            public bool schedule { get; set; }
            public object[] schedule_rules { get; set; }
            public string obstacle_mode { get; set; }
            public string obstacle_action { get; set; }
            public int obstacle_power { get; set; }
            public decimal obstacle_delay { get; set; }
            public string safety_mode { get; set; }
            public string safety_action { get; set; }
            public string safety_allowed_on_trigger { get; set; }
            public int off_power { get; set; }
            public bool positioning { get; set; }
        }

        public class GetShelly25SettingsFavorite
        {
            public string name { get; set; }
            public int pos { get; set; }
        }


        public class GetRollerStatus
        {
            public string state { get; set; }
            public decimal power { get; set; }
            public bool is_valid { get; set; }
            public bool safety_switch { get; set; }
            public bool overtemperature { get; set; }
            public string stop_reason { get; set; }
            public string last_direction { get; set; }
            public int current_pos { get; set; }
            public bool calibrating { get; set; }
            public bool positioning { get; set; }
        }


    }

    public class Shelly25Roller : Shelly25DeviceBase, IToggleSwitchDevice
    {
        public Shelly25Roller(string ipv4, string deviceId) : base(ipv4, deviceId)
        {

        }

        public override List<string> GetRoles()
        {
            var ret = base.GetRoles();
            ret.Add(Device.HomeAutomationMainRoleShutterSwitch);
            return ret;
        }

        public DeviceData ChangeSwitchValue(string switchName, string switchNewValue)
        {
            if (string.IsNullOrEmpty(switchNewValue))
                throw new ArgumentNullException(nameof(switchName));

            var state = 0;
            if (switchName.Equals("open") && !string.IsNullOrEmpty(switchNewValue))
            {
                if (switchNewValue.Equals("on", StringComparison.InvariantCultureIgnoreCase))
                    state = 100;
                else if (switchNewValue.Equals("off", StringComparison.InvariantCultureIgnoreCase))
                    state = 0;
                else if(!int.TryParse(switchNewValue, out state))
                    throw new ArgumentException("New value invalid !", nameof(switchNewValue));
            }

            var t = SetRollerStatus(state.ToString("0"));
            if (t != null)
                return ConvertState(t);
            else
            {
                t = GetRollerStatus();
                if (t == null)
                    throw new ApplicationException("Device inaccessible");
                return ConvertState(t);
            }
        }

        public IEnumerable<DeviceData> GetSwitches()
        {
            var dvs = new List<DeviceData>();

            var t = GetRollerStatus();
            if (t == null)
                throw new ApplicationException("Device inaccessible");

            dvs.Add(ConvertState(t));

            return dvs;

        }

        private DeviceData ConvertState(GetRollerStatus t)
        {
            var ret = new DeviceData()
            {
                IsMainData = true,
                Name = "open",
                StandardDataType = "switch",
            };

            if (t.positioning)
                ret.ValidValues = new string[] { "off", "25", "50", "75", "on" };
            else
                ret.ValidValues = new string[] { "off", "on" };

            if (t.current_pos < 2)
                ret.Value = "off";
            else if (t.current_pos <= 27)
                ret.Value = "25";
            else if (t.current_pos <= 55)
                ret.Value = "50";
            else if (t.current_pos <= 90)
                ret.Value = "75";
            else
                ret.Value = "on";


            return ret;
        }

        protected GetShelly25SettingsRoller GetRollerSettings(int index = 0)
        {
            string url = $"http://{IpV4}/settings/roller/{index}";
            using (var cli = new ShellyWebClient())
            {
                try
                {
                    string json = cli.DownloadString(url);
                    var sdi = JsonConvert.DeserializeObject<GetShelly25SettingsRoller>(json);
                    return sdi;
                }
                catch (WebException)
                {
                    return null;
                }
            }
        }

        protected GetRollerStatus GetRollerStatus(int index = 0)
        {
            string url = $"http://{IpV4}/roller/{index}";
            using (var cli = new ShellyWebClient())
            {
                try
                {
                    string json = cli.DownloadString(url);
                    var sdi = JsonConvert.DeserializeObject<GetRollerStatus>(json);
                    return sdi;
                }
                catch (WebException)
                {
                    return null;
                }
            }
        }

        protected GetRollerStatus SetRollerStatus(string go, int index = 0)
        {
            string to_pos = null;
            int pos = 0;

            if (go.Equals("on") || go.Equals("100"))
                go = "open";
            else if (go.Equals("off") || go.Equals("0"))
                go = "close";
            else if (int.TryParse(go, out pos))
            {
                go = "to_pos";
                to_pos = pos.ToString();
            }


            string url = $"http://{IpV4}/roller/{index}?go=" + go;
            if (!string.IsNullOrEmpty(to_pos))
                url += "&roller_pos=" + to_pos;

            using (var cli = new ShellyWebClient())
            {
                try
                {
                    string json = cli.DownloadString(url);
                    var sdi = JsonConvert.DeserializeObject<GetRollerStatus>(json);
                    return sdi;
                }
                catch (WebException)
                {
                    return null;
                }
            }
        }
    }

    public class Shelly25MultiRelay : Shelly25DeviceBase
    {
        public Shelly25MultiRelay(string ipv4, string deviceId) : base(ipv4, deviceId)
        {

        }


    }

}
