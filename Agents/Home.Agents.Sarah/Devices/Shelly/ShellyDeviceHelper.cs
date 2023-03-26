using Home.Common;
using Home.Common.HomeAutomation;
using Home.Common.Messages;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Home.Agents.Sarah.Devices.Shelly
{
    public static partial class ShellyDeviceHelper
    {
        #region DataObjects
        public class GetShellyDeviceInfo
        {
            public string type { get; set; }
            public string mac { get; set; }
            public bool auth { get; set; }
            public string fw { get; set; }
            public int longid { get; set; }
            public bool sleep_mode { get; set; }
        }

        public class GetStatusResponse
        {
            public GetStatusWifiState wifi_sta { get; set; }
            public GetStatusCloudConfig cloud { get; set; }
            public GetStatusMqttStatus mqtt { get; set; }
            public string time { get; set; }
            public int unixtime { get; set; }
            public int serial { get; set; }
            public bool has_update { get; set; }
            public string mac { get; set; }
            public GetStatusUpdateStatus update { get; set; }
            public int ram_total { get; set; }
            public int ram_free { get; set; }
            public int ram_lwm { get; set; }
            public int fs_size { get; set; }
            public int fs_free { get; set; }
            public int uptime { get; set; }
        }

        public class GetStatusWifiState
        {
            public bool connected { get; set; }
            public string ssid { get; set; }
            public string ip { get; set; }
            public int rssi { get; set; }
        }

        public class GetStatusCloudConfig
        {
            public bool enabled { get; set; }
            public bool connected { get; set; }
        }

        public class GetStatusMqttStatus
        {
            public bool connected { get; set; }
        }

        public class GetStatusUpdateStatus
        {
            public string status { get; set; }
            public bool has_update { get; set; }
            public string new_version { get; set; }
            public string old_version { get; set; }
        }



        public class GetSettingsResponse
        {
            public GetSettingsDeviceInfo device { get; set; }
            public GetSettingsWifiAp wifi_ap { get; set; }
            public GetSettingsWifi wifi_sta { get; set; }
            public GetSettingsAPRoaming ap_roaming { get; set; }
            public GetSettingsMqtt mqtt { get; set; }
            public GetSettingsCoiot coiot { get; set; }
            public GetSettingsSntp sntp { get; set; }
            public GetSettingsLogin login { get; set; }
            public string pin_code { get; set; }
            public string name { get; set; }
            public string fw { get; set; }
            public bool discoverable { get; set; }
            public GetSettingsBuildInfo build_info { get; set; }
            public GetSettingsCloudConfig cloud { get; set; }
            public string timezone { get; set; }
            public float lat { get; set; }
            public float lng { get; set; }
            public bool tzautodetect { get; set; }
            public int tz_utc_offset { get; set; }
            public bool tz_dst { get; set; }
            public bool tz_dst_auto { get; set; }
            public string time { get; set; }
            public int unixtime { get; set; }
            public bool debug_enable { get; set; }
            public bool allow_cross_origin { get; set; }
            public bool wifirecovery_reboot_enabled { get; set; }
        }

        public class GetSettingsDeviceInfo
        {
            public string type { get; set; }
            public string mac { get; set; }
            public string hostname { get; set; }
        }

        public class GetSettingsWifiAp
        {
            public bool enabled { get; set; }
            public string ssid { get; set; }
            public string key { get; set; }
        }

        public class GetSettingsWifi
        {
            public bool enabled { get; set; }
            public string ssid { get; set; }
            public string ipv4_method { get; set; }
            public object ip { get; set; }
            public object gw { get; set; }
            public object mask { get; set; }
            public object dns { get; set; }
        }

        public class GetSettingsAPRoaming
        {
            public bool enabled { get; set; }
            public int threshold { get; set; }
        }

        public class GetSettingsMqtt
        {
            public bool enable { get; set; }
            public string server { get; set; }
            public string user { get; set; }
            public string id { get; set; }
            public decimal reconnect_timeout_max { get; set; }
            public decimal reconnect_timeout_min { get; set; }
            public bool clean_session { get; set; }
            public int keep_alive { get; set; }
            public int max_qos { get; set; }
            public bool retain { get; set; }
            public int update_period { get; set; }
        }

        public class GetSettingsCoiot
        {
            public bool enabled { get; set; }
            public int update_period { get; set; }
            public string peer { get; set; }
        }

        public class GetSettingsSntp
        {
            public string server { get; set; }
            public bool enabled { get; set; }
        }

        public class GetSettingsLogin
        {
            public bool enabled { get; set; }
            public bool unprotected { get; set; }
            public string username { get; set; }
        }

        public class GetSettingsBuildInfo
        {
            public string build_id { get; set; }
            public DateTime build_timestamp { get; set; }
            public string build_version { get; set; }
        }

        public class GetSettingsCloudConfig
        {
            public bool enabled { get; set; }
            public bool connected { get; set; }
        }


        public class GetLoginSettingsResponse
        {
            public bool enabled { get; set; }
            public bool unprotected { get; set; }
            public string username { get; set; }
        }


        public class MqttAnnounce
        {
            public string id { get; set; }
            public string model { get; set; }
            public string mac { get; set; }
            public string ip { get; set; }
            public bool new_fw { get; set; }
            public string fw_ver { get; set; }
        }



        #endregion

        public static GetStatusResponse GetStatus(string IpV4)
        {
            return GetStatus<GetStatusResponse>(IpV4);
        }

        public static T GetStatus<T>(string IpV4) where T : GetStatusResponse, new()
        {
            string url = $"http://{IpV4}/status";
            using (var cli = new ShellyWebClient())
            {
                try
                {
                    string json = cli.DownloadString(url);
                    var sdi = JsonConvert.DeserializeObject<T>(json);
                    return sdi;
                }
                catch (WebException)
                {
                    return null;
                }
            }
        }


        public static GetSettingsResponse GetSettings(string IpV4)
        {
            return GetSettings<GetSettingsResponse>(IpV4);
        }

        public static T GetSettings<T>(string IpV4) where T : GetSettingsResponse, new()
        {
            string url = $"http://{IpV4}/settings";
            using (var cli = new ShellyWebClient())
            {
                try
                {
                    string json = cli.DownloadString(url);
                    var sdi = JsonConvert.DeserializeObject<T>(json);
                    return sdi;
                }
                catch (WebException)
                {
                    return null;
                }
            }
        }

        public static GetLoginSettingsResponse GetLoginSettings(string IpV4)
        {
            string url = $"http://{IpV4}/settings/login";
            using (var cli = new ShellyWebClient())
            {
                try
                {
                    string json = cli.DownloadString(url);
                    var sdi = JsonConvert.DeserializeObject<GetLoginSettingsResponse>(json);
                    return sdi;
                }
                catch (WebException)
                {
                    return null;
                }
            }
        }

        internal static GetShellyDeviceInfo GetDeviceInfo(string ipV4)
        {
            using (WebClient cli = new WebClient())
            {
                string url = $"http://{ipV4}/shelly";
                try
                {
                    string json = cli.DownloadString(url);
                    return JsonConvert.DeserializeObject<GetShellyDeviceInfo>(json);
                }
                catch (WebException)
                {
                    return null;
                }
            }
        }

    }
}
