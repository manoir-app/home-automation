using Home.Agents.Sarah.Devices.Hue;
using Home.Common;
using Home.Graph.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Home.Agents.Sarah.Devices.Zigbee2Mqtt
{
    partial class Z2MqttHelper
    {

        public class Z2MqttInfo
        {
            public string commit { get; set; }
            public Config config { get; set; }
            //public Config_Schema config_schema { get; set; }
            public Coordinator coordinator { get; set; }
            public string log_level { get; set; }
            public Network network { get; set; }
            public bool permit_join { get; set; }
            public bool restart_required { get; set; }
            public string version { get; set; }
        }

        public class Config
        {
            public Advanced advanced { get; set; }
            public Availability availability { get; set; }
            public object[] blocklist { get; set; }
            public Device_Options device_options { get; set; }
            public Dictionary<string, DeviceConfigBase> devices { get; set; }
            //public object[] external_converters { get; set; }
            public Frontend frontend { get; set; }
            public Dictionary<string, Group> groups { get; set; }
            public bool homeassistant { get; set; }
            public Map_Options map_options { get; set; }
            public Mqtt mqtt { get; set; }
            public Ota ota { get; set; }
            public object[] passlist { get; set; }
            public bool permit_join { get; set; }
            public Serial serial { get; set; }
        }

        public class Advanced
        {
            public object adapter_concurrent { get; set; }
            public object adapter_delay { get; set; }
            public object[] availability_blacklist { get; set; }
            public object[] availability_blocklist { get; set; }
            public object[] availability_passlist { get; set; }
            public object[] availability_whitelist { get; set; }
            public bool cache_state { get; set; }
            public bool cache_state_persistent { get; set; }
            public bool cache_state_send_on_startup { get; set; }
            public int channel { get; set; }
            public bool elapsed { get; set; }
            public int[] ext_pan_id { get; set; }
            public bool homeassistant_legacy_entity_attributes { get; set; }
            public string last_seen { get; set; }
            public bool legacy_api { get; set; }
            public bool legacy_availability_payload { get; set; }
            public string log_directory { get; set; }
            public string log_file { get; set; }
            public string log_level { get; set; }
            public string[] log_output { get; set; }
            public bool log_rotation { get; set; }
            public bool log_symlink_current { get; set; }
            public Log_Syslog log_syslog { get; set; }
            public string output { get; set; }
            public int pan_id { get; set; }
            public bool report { get; set; }
            public int soft_reset_timeout { get; set; }
            public string timestamp_format { get; set; }
        }

        public class Log_Syslog
        {
            public string app_name { get; set; }
            public string eol { get; set; }
            public string host { get; set; }
            public string localhost { get; set; }
            public string path { get; set; }
            public string pid { get; set; }
            public int port { get; set; }
            public string protocol { get; set; }
            public string type { get; set; }
        }

        public class Availability
        {
        }

        public class Device_Options
        {
            public bool legacy { get; set; }
        }

        public class DeviceConfigBase
        {
            public string friendly_name { get; set; }
            public int ? temperature_calibration { get; set; }
            public string temperature_precision { get; set; }
            public HomeassistantConfig homeassistant { get; set; }
            public bool? legacy { get; set; }
            public bool? optimistic { get; set; }
            public bool? retain { get; set; }
            public int? debounce { get; set; }
        }

        public class HomeassistantConfig
        {
        }

        public class Frontend
        {
            public string host { get; set; }
            public int port { get; set; }
        }

        public class Group
        {
            public string[] devices { get; set; }
            public string friendly_name { get; set; }
        }

        public class Map_Options
        {
            public Graphviz graphviz { get; set; }
        }

        public class Graphviz
        {
            public Colors colors { get; set; }
        }

        public class Colors
        {
            public Fill fill { get; set; }
            public Font font { get; set; }
            public Line line { get; set; }
        }

        public class Fill
        {
            public string coordinator { get; set; }
            public string enddevice { get; set; }
            public string router { get; set; }
        }

        public class Font
        {
            public string coordinator { get; set; }
            public string enddevice { get; set; }
            public string router { get; set; }
        }

        public class Line
        {
            public string active { get; set; }
            public string inactive { get; set; }
        }

        public class Mqtt
        {
            public string base_topic { get; set; }
            public bool force_disable_retain { get; set; }
            public bool include_device_information { get; set; }
            public string server { get; set; }
        }

        public class Ota
        {
            public bool disable_automatic_update_check { get; set; }
            public int update_check_interval { get; set; }
        }

        public class Serial
        {
            public string adapter { get; set; }
            public bool disable_led { get; set; }
            public string port { get; set; }
        }

        //public class Config_Schema
        //{
        //    public Definitions definitions { get; set; }
        //    public Properties3 properties { get; set; }
        //    public string[] required { get; set; }
        //    public string type { get; set; }
        //}

        //public class Definitions
        //{
        //    public Device device { get; set; }
        //    public Group group { get; set; }
        //}

        //public class Device
        //{
        //    public Properties properties { get; set; }
        //    public string[] required { get; set; }
        //    public string type { get; set; }
        //}

        //public class Properties
        //{
        //    public Debounce debounce { get; set; }
        //    public Debounce_Ignore debounce_ignore { get; set; }
        //    public Disabled disabled { get; set; }
        //    public Filtered_Attributes filtered_attributes { get; set; }
        //    public Filtered_Cache filtered_cache { get; set; }
        //    public Filtered_Optimistic filtered_optimistic { get; set; }
        //    public Friendly_Name friendly_name { get; set; }
        //    public Homeassistant1 homeassistant { get; set; }
        //    public Icon icon { get; set; }
        //    public Optimistic optimistic { get; set; }
        //    public Qos qos { get; set; }
        //    public Retain retain { get; set; }
        //    public Retention retention { get; set; }
        //}

        //public class Debounce
        //{
        //    public string description { get; set; }
        //    public string title { get; set; }
        //    public string type { get; set; }
        //}

        //public class Debounce_Ignore
        //{
        //    public string description { get; set; }
        //    public string[] examples { get; set; }
        //    public Items items { get; set; }
        //    public string title { get; set; }
        //    public string type { get; set; }
        //}

        //public class Items
        //{
        //    public string type { get; set; }
        //}

        //public class Disabled
        //{
        //    public string description { get; set; }
        //    public bool requiresRestart { get; set; }
        //    public string title { get; set; }
        //    public string type { get; set; }
        //}

        //public class Filtered_Attributes
        //{
        //    public string description { get; set; }
        //    public string[] examples { get; set; }
        //    public Items1 items { get; set; }
        //    public string title { get; set; }
        //    public string type { get; set; }
        //}

        //public class Items1
        //{
        //    public string type { get; set; }
        //}

        //public class Filtered_Cache
        //{
        //    public string description { get; set; }
        //    public string[] examples { get; set; }
        //    public Items2 items { get; set; }
        //    public string title { get; set; }
        //    public string type { get; set; }
        //}

        //public class Items2
        //{
        //    public string type { get; set; }
        //}

        //public class Filtered_Optimistic
        //{
        //    public string description { get; set; }
        //    public string[] examples { get; set; }
        //    public Items3 items { get; set; }
        //    public string title { get; set; }
        //    public string type { get; set; }
        //}

        //public class Items3
        //{
        //    public string type { get; set; }
        //}

        //public class Friendly_Name
        //{
        //    public string description { get; set; }
        //    public bool readOnly { get; set; }
        //    public string title { get; set; }
        //    public string type { get; set; }
        //}

        //public class Homeassistant1
        //{
        //    public Properties1 properties { get; set; }
        //    public string title { get; set; }
        //    public string[] type { get; set; }
        //}

        //public class Properties1
        //{
        //    public Name name { get; set; }
        //}

        //public class Name
        //{
        //    public string description { get; set; }
        //    public string title { get; set; }
        //    public string type { get; set; }
        //}

        //public class Icon
        //{
        //    public string description { get; set; }
        //    public string title { get; set; }
        //    public string type { get; set; }
        //}

        //public class Optimistic
        //{
        //    public bool _default { get; set; }
        //    public string description { get; set; }
        //    public string title { get; set; }
        //    public string type { get; set; }
        //}

        //public class Qos
        //{
        //    public string description { get; set; }
        //    public string title { get; set; }
        //    public string type { get; set; }
        //}

        //public class Retain
        //{
        //    public string description { get; set; }
        //    public string title { get; set; }
        //    public string type { get; set; }
        //}

        //public class Retention
        //{
        //    public string description { get; set; }
        //    public string title { get; set; }
        //    public string type { get; set; }
        //}

        //public class Group
        //{
        //    public Properties2 properties { get; set; }
        //    public string[] required { get; set; }
        //    public string type { get; set; }
        //}

        //public class Properties2
        //{
        //    public Devices1 devices { get; set; }
        //    public Filtered_Attributes1 filtered_attributes { get; set; }
        //    public Friendly_Name1 friendly_name { get; set; }
        //    public Off_State off_state { get; set; }
        //    public Optimistic1 optimistic { get; set; }
        //    public Qos1 qos { get; set; }
        //    public Retain1 retain { get; set; }
        //}

        //public class Devices1
        //{
        //    public ItemsType items { get; set; }
        //    public string type { get; set; }
        //}

        //public class Filtered_Attributes1
        //{
        //    public ItemsType items { get; set; }
        //    public string type { get; set; }
        //}

        //public class ItemsType
        //{
        //    public string type { get; set; }
        //}

        //public class Friendly_Name1
        //{
        //    public string type { get; set; }
        //}

        //public class Off_State
        //{
        //    public string _default { get; set; }
        //    public string description { get; set; }
        //    public string[] _enum { get; set; }
        //    public bool requiresRestart { get; set; }
        //    public string title { get; set; }
        //    public string[] type { get; set; }
        //}

        //public class Optimistic1
        //{
        //    public string type { get; set; }
        //}

        //public class Qos1
        //{
        //    public string type { get; set; }
        //}

        //public class Retain1
        //{
        //    public string type { get; set; }
        //}

        //public class Properties3
        //{
        //    public Advanced1 advanced { get; set; }
        //    public Availability1 availability { get; set; }
        //    public Ban ban { get; set; }
        //    public Blocklist blocklist { get; set; }
        //    public Device_Options1 device_options { get; set; }
        //    public Devices2 devices { get; set; }
        //    public External_Converters external_converters { get; set; }
        //    public Frontend1 frontend { get; set; }
        //    public Groups1 groups { get; set; }
        //    public Homeassistant2 homeassistant { get; set; }
        //    public Map_Options1 map_options { get; set; }
        //    public Mqtt1 mqtt { get; set; }
        //    public Ota1 ota { get; set; }
        //    public Passlist passlist { get; set; }
        //    public Permit_Join permit_join { get; set; }
        //    public Serial1 serial { get; set; }
        //    public Whitelist whitelist { get; set; }
        //}

        //public class Advanced1
        //{
        //    public Properties4 properties { get; set; }
        //    public string title { get; set; }
        //    public string type { get; set; }
        //}

        //public class Properties4
        //{
        //    public Adapter_Concurrent adapter_concurrent { get; set; }
        //    public Adapter_Delay adapter_delay { get; set; }
        //    public Cache_State cache_state { get; set; }
        //    public Cache_State_Persistent cache_state_persistent { get; set; }
        //    public Cache_State_Send_On_Startup cache_state_send_on_startup { get; set; }
        //    public Channel channel { get; set; }
        //    public Elapsed elapsed { get; set; }
        //    public Ext_Pan_Id ext_pan_id { get; set; }
        //    public Last_Seen last_seen { get; set; }
        //    public Legacy_Api legacy_api { get; set; }
        //    public Legacy_Availability_Payload legacy_availability_payload { get; set; }
        //    public Log_Directory log_directory { get; set; }
        //    public Log_File log_file { get; set; }
        //    public Log_Level log_level { get; set; }
        //    public Log_Output log_output { get; set; }
        //    public Log_Rotation log_rotation { get; set; }
        //    public Log_Symlink_Current log_symlink_current { get; set; }
        //    public Log_Syslog1 log_syslog { get; set; }
        //    public Network_Key network_key { get; set; }
        //    public Output output { get; set; }
        //    public Pan_Id pan_id { get; set; }
        //    public Timestamp_Format timestamp_format { get; set; }
        //    public Transmit_Power transmit_power { get; set; }
        //}

        //public class Adapter_Concurrent
        //{
        //    public string description { get; set; }
        //    public bool requiresRestart { get; set; }
        //    public string title { get; set; }
        //    public string[] type { get; set; }
        //}

        //public class Adapter_Delay
        //{
        //    public string description { get; set; }
        //    public bool requiresRestart { get; set; }
        //    public string title { get; set; }
        //    public string[] type { get; set; }
        //}

        //public class Cache_State
        //{
        //    public bool _default { get; set; }
        //    public string description { get; set; }
        //    public string title { get; set; }
        //    public string type { get; set; }
        //}

        //public class Cache_State_Persistent
        //{
        //    public bool _default { get; set; }
        //    public string description { get; set; }
        //    public string title { get; set; }
        //    public string type { get; set; }
        //}

        //public class Cache_State_Send_On_Startup
        //{
        //    public bool _default { get; set; }
        //    public string description { get; set; }
        //    public string title { get; set; }
        //    public string type { get; set; }
        //}

        //public class Channel
        //{
        //    public int _default { get; set; }
        //    public string description { get; set; }
        //    public int[] examples { get; set; }
        //    public int maximum { get; set; }
        //    public int minimum { get; set; }
        //    public bool requiresRestart { get; set; }
        //    public string title { get; set; }
        //    public string type { get; set; }
        //}

        //public class Elapsed
        //{
        //    public bool _default { get; set; }
        //    public string description { get; set; }
        //    public string title { get; set; }
        //    public string type { get; set; }
        //}

        //public class Ext_Pan_Id
        //{
        //    public string description { get; set; }
        //    public Items6 items { get; set; }
        //    public bool requiresRestart { get; set; }
        //    public string title { get; set; }
        //    public string type { get; set; }
        //}

        //public class Items6
        //{
        //    public string type { get; set; }
        //}

        //public class Last_Seen
        //{
        //    public string _default { get; set; }
        //    public string description { get; set; }
        //    public string[] _enum { get; set; }
        //    public string title { get; set; }
        //    public string type { get; set; }
        //}

        //public class Legacy_Api
        //{
        //    public bool _default { get; set; }
        //    public string description { get; set; }
        //    public bool requiresRestart { get; set; }
        //    public string title { get; set; }
        //    public string type { get; set; }
        //}

        //public class Legacy_Availability_Payload
        //{
        //    public bool _default { get; set; }
        //    public string description { get; set; }
        //    public bool requiresRestart { get; set; }
        //    public string title { get; set; }
        //    public string type { get; set; }
        //}

        //public class Log_Directory
        //{
        //    public string description { get; set; }
        //    public string[] examples { get; set; }
        //    public bool requiresRestart { get; set; }
        //    public string title { get; set; }
        //    public string type { get; set; }
        //}

        //public class Log_File
        //{
        //    public string _default { get; set; }
        //    public string description { get; set; }
        //    public string[] examples { get; set; }
        //    public bool requiresRestart { get; set; }
        //    public string title { get; set; }
        //    public string type { get; set; }
        //}

        //public class Log_Level
        //{
        //    public string _default { get; set; }
        //    public string description { get; set; }
        //    public string[] _enum { get; set; }
        //    public string title { get; set; }
        //    public string type { get; set; }
        //}

        //public class Log_Output
        //{
        //    public string description { get; set; }
        //    public Items7 items { get; set; }
        //    public bool requiresRestart { get; set; }
        //    public string title { get; set; }
        //    public string type { get; set; }
        //}

        //public class Items7
        //{
        //    public string[] _enum { get; set; }
        //    public string type { get; set; }
        //}

        //public class Log_Rotation
        //{
        //    public bool _default { get; set; }
        //    public string description { get; set; }
        //    public bool requiresRestart { get; set; }
        //    public string title { get; set; }
        //    public string type { get; set; }
        //}

        //public class Log_Symlink_Current
        //{
        //    public bool _default { get; set; }
        //    public string description { get; set; }
        //    public bool requiresRestart { get; set; }
        //    public string title { get; set; }
        //    public string type { get; set; }
        //}

        //public class Log_Syslog1
        //{
        //    public Properties5 properties { get; set; }
        //    public string title { get; set; }
        //    public string type { get; set; }
        //}

        //public class Properties5
        //{
        //    public App_Name app_name { get; set; }
        //    public Eol eol { get; set; }
        //    public Host host { get; set; }
        //    public Localhost localhost { get; set; }
        //    public Path path { get; set; }
        //    public Pid pid { get; set; }
        //    public Port port { get; set; }
        //    public Protocol protocol { get; set; }
        //    public Type type { get; set; }
        //}

        //public class App_Name
        //{
        //    public string _default { get; set; }
        //    public string description { get; set; }
        //    public string title { get; set; }
        //    public string type { get; set; }
        //}

        //public class Eol
        //{
        //    public string _default { get; set; }
        //    public string description { get; set; }
        //    public string title { get; set; }
        //    public string type { get; set; }
        //}

        //public class Host
        //{
        //    public string _default { get; set; }
        //    public string description { get; set; }
        //    public string title { get; set; }
        //    public string type { get; set; }
        //}

        //public class Localhost
        //{
        //    public string _default { get; set; }
        //    public string description { get; set; }
        //    public string title { get; set; }
        //    public string type { get; set; }
        //}

        //public class Path
        //{
        //    public string _default { get; set; }
        //    public string description { get; set; }
        //    public string[] examples { get; set; }
        //    public string title { get; set; }
        //    public string type { get; set; }
        //}

        //public class Pid
        //{
        //    public string _default { get; set; }
        //    public string description { get; set; }
        //    public string title { get; set; }
        //    public string type { get; set; }
        //}

        //public class Port
        //{
        //    public int _default { get; set; }
        //    public string description { get; set; }
        //    public string title { get; set; }
        //    public string type { get; set; }
        //}

        //public class Protocol
        //{
        //    public string _default { get; set; }
        //    public string description { get; set; }
        //    public string[] examples { get; set; }
        //    public string title { get; set; }
        //    public string type { get; set; }
        //}

        //public class Type
        //{
        //    public string _default { get; set; }
        //    public string description { get; set; }
        //    public string title { get; set; }
        //    public string type { get; set; }
        //}

        //public class Network_Key
        //{
        //    public string description { get; set; }
        //    public Oneof[] oneOf { get; set; }
        //    public bool requiresRestart { get; set; }
        //    public string title { get; set; }
        //}

        //public class Oneof
        //{
        //    public string title { get; set; }
        //    public string type { get; set; }
        //    public Items8 items { get; set; }
        //}

        //public class Items8
        //{
        //    public string type { get; set; }
        //}

        //public class Output
        //{
        //    public string description { get; set; }
        //    public string[] _enum { get; set; }
        //    public string title { get; set; }
        //    public string type { get; set; }
        //}

        //public class Pan_Id
        //{
        //    public string description { get; set; }
        //    public Oneof1[] oneOf { get; set; }
        //    public bool requiresRestart { get; set; }
        //    public string title { get; set; }
        //}

        //public class Oneof1
        //{
        //    public string title { get; set; }
        //    public string type { get; set; }
        //}

        //public class Timestamp_Format
        //{
        //    public string description { get; set; }
        //    public string[] examples { get; set; }
        //    public bool requiresRestart { get; set; }
        //    public string title { get; set; }
        //    public string type { get; set; }
        //}

        //public class Transmit_Power
        //{
        //    public string description { get; set; }
        //    public bool requiresRestart { get; set; }
        //    public string title { get; set; }
        //    public string[] type { get; set; }
        //}

        //public class Availability1
        //{
        //    public string description { get; set; }
        //    public Oneof2[] oneOf { get; set; }
        //    public bool requiresRestart { get; set; }
        //    public string title { get; set; }
        //}

        //public class Oneof2
        //{
        //    public string title { get; set; }
        //    public string type { get; set; }
        //    public Properties6 properties { get; set; }
        //}

        //public class Properties6
        //{
        //    public Active active { get; set; }
        //    public Passive passive { get; set; }
        //}

        //public class Active
        //{
        //    public string description { get; set; }
        //    public Properties7 properties { get; set; }
        //    public bool requiresRestart { get; set; }
        //    public string title { get; set; }
        //    public string type { get; set; }
        //}

        //public class Properties7
        //{
        //    public Timeout timeout { get; set; }
        //}

        //public class Timeout
        //{
        //    public int _default { get; set; }
        //    public string description { get; set; }
        //    public bool requiresRestart { get; set; }
        //    public string title { get; set; }
        //    public string type { get; set; }
        //}

        //public class Passive
        //{
        //    public string description { get; set; }
        //    public Properties8 properties { get; set; }
        //    public bool requiresRestart { get; set; }
        //    public string title { get; set; }
        //    public string type { get; set; }
        //}

        //public class Properties8
        //{
        //    public Timeout1 timeout { get; set; }
        //}

        //public class Timeout1
        //{
        //    public int _default { get; set; }
        //    public string description { get; set; }
        //    public bool requiresRestart { get; set; }
        //    public string title { get; set; }
        //    public string type { get; set; }
        //}

        //public class Ban
        //{
        //    public Items9 items { get; set; }
        //    public bool readOnly { get; set; }
        //    public bool requiresRestart { get; set; }
        //    public string title { get; set; }
        //    public string type { get; set; }
        //}

        //public class Items9
        //{
        //    public string type { get; set; }
        //}

        //public class Blocklist
        //{
        //    public string description { get; set; }
        //    public Items10 items { get; set; }
        //    public bool requiresRestart { get; set; }
        //    public string title { get; set; }
        //    public string type { get; set; }
        //}

        //public class Items10
        //{
        //    public string type { get; set; }
        //}

        //public class Device_Options1
        //{
        //    public string title { get; set; }
        //    public string type { get; set; }
        //}

        //public class Devices2
        //{
        //    public Patternproperties patternProperties { get; set; }
        //    public Propertynames propertyNames { get; set; }
        //    public string type { get; set; }
        //}

        //public class Patternproperties
        //{
        //    public _ _ { get; set; }
        //}

        //public class _
        //{
        //    public string _ref { get; set; }
        //}

        //public class Propertynames
        //{
        //    public string pattern { get; set; }
        //}

        //public class External_Converters
        //{
        //    public string description { get; set; }
        //    public string[] examples { get; set; }
        //    public Items11 items { get; set; }
        //    public bool requiresRestart { get; set; }
        //    public string title { get; set; }
        //    public string type { get; set; }
        //}

        //public class Items11
        //{
        //    public string type { get; set; }
        //}

        //public class Frontend1
        //{
        //    public Oneof3[] oneOf { get; set; }
        //    public bool requiresRestart { get; set; }
        //    public string title { get; set; }
        //}

        //public class Oneof3
        //{
        //    public string title { get; set; }
        //    public string type { get; set; }
        //    public Properties9 properties { get; set; }
        //}

        //public class Properties9
        //{
        //    public Auth_Token auth_token { get; set; }
        //    public Host1 host { get; set; }
        //    public Port1 port { get; set; }
        //    public Ssl_Cert ssl_cert { get; set; }
        //    public Ssl_Key ssl_key { get; set; }
        //    public Url url { get; set; }
        //}

        //public class Auth_Token
        //{
        //    public string description { get; set; }
        //    public bool requiresRestart { get; set; }
        //    public string title { get; set; }
        //    public string[] type { get; set; }
        //}

        //public class Host1
        //{
        //    public string _default { get; set; }
        //    public string description { get; set; }
        //    public bool requiresRestart { get; set; }
        //    public string title { get; set; }
        //    public string type { get; set; }
        //}

        //public class Port1
        //{
        //    public int _default { get; set; }
        //    public string description { get; set; }
        //    public bool requiresRestart { get; set; }
        //    public string title { get; set; }
        //    public string type { get; set; }
        //}

        //public class Ssl_Cert
        //{
        //    public string description { get; set; }
        //    public bool requiresRestart { get; set; }
        //    public string title { get; set; }
        //    public string[] type { get; set; }
        //}

        //public class Ssl_Key
        //{
        //    public string description { get; set; }
        //    public bool requiresRestart { get; set; }
        //    public string title { get; set; }
        //    public string[] type { get; set; }
        //}

        //public class Url
        //{
        //    public string description { get; set; }
        //    public bool requiresRestart { get; set; }
        //    public string title { get; set; }
        //    public string[] type { get; set; }
        //}

        //public class Groups1
        //{
        //    public Patternproperties1 patternProperties { get; set; }
        //    public Propertynames1 propertyNames { get; set; }
        //    public string type { get; set; }
        //}

        //public class Patternproperties1
        //{
        //    public _1 _ { get; set; }
        //}

        //public class _1
        //{
        //    public string _ref { get; set; }
        //}

        //public class Propertynames1
        //{
        //    public string pattern { get; set; }
        //}

        //public class Homeassistant2
        //{
        //    public bool _default { get; set; }
        //    public string description { get; set; }
        //    public Oneof4[] oneOf { get; set; }
        //    public bool requiresRestart { get; set; }
        //    public string title { get; set; }
        //}

        //public class Oneof4
        //{
        //    public string title { get; set; }
        //    public string type { get; set; }
        //    public Properties10 properties { get; set; }
        //}

        //public class Properties10
        //{
        //    public Discovery_Topic discovery_topic { get; set; }
        //    public Legacy_Entity_Attributes legacy_entity_attributes { get; set; }
        //    public Legacy_Triggers legacy_triggers { get; set; }
        //    public Status_Topic status_topic { get; set; }
        //}

        //public class Discovery_Topic
        //{
        //    public string description { get; set; }
        //    public string[] examples { get; set; }
        //    public bool requiresRestart { get; set; }
        //    public string title { get; set; }
        //    public string type { get; set; }
        //}

        //public class Legacy_Entity_Attributes
        //{
        //    public bool _default { get; set; }
        //    public string description { get; set; }
        //    public string title { get; set; }
        //    public string type { get; set; }
        //}

        //public class Legacy_Triggers
        //{
        //    public bool _default { get; set; }
        //    public string description { get; set; }
        //    public string title { get; set; }
        //    public string type { get; set; }
        //}

        //public class Status_Topic
        //{
        //    public string description { get; set; }
        //    public string[] examples { get; set; }
        //    public bool requiresRestart { get; set; }
        //    public string title { get; set; }
        //    public string type { get; set; }
        //}

        //public class Map_Options1
        //{
        //    public Properties11 properties { get; set; }
        //    public string title { get; set; }
        //    public string type { get; set; }
        //}

        //public class Properties11
        //{
        //    public Graphviz1 graphviz { get; set; }
        //}

        //public class Graphviz1
        //{
        //    public Properties12 properties { get; set; }
        //    public string type { get; set; }
        //}

        //public class Properties12
        //{
        //    public Colors1 colors { get; set; }
        //}

        //public class Colors1
        //{
        //    public Properties13 properties { get; set; }
        //    public string type { get; set; }
        //}

        //public class Properties13
        //{
        //    public Fill1 fill { get; set; }
        //    public Font1 font { get; set; }
        //    public Line1 line { get; set; }
        //}

        //public class Fill1
        //{
        //    public Properties14 properties { get; set; }
        //    public string type { get; set; }
        //}

        //public class Properties14
        //{
        //    public Coordinator coordinator { get; set; }
        //    public Enddevice enddevice { get; set; }
        //    public Router router { get; set; }
        //}

        //public class Coordinator
        //{
        //    public string type { get; set; }
        //}

        //public class Enddevice
        //{
        //    public string type { get; set; }
        //}

        //public class Router
        //{
        //    public string type { get; set; }
        //}

        //public class Font1
        //{
        //    public Properties15 properties { get; set; }
        //    public string type { get; set; }
        //}

        //public class Properties15
        //{
        //    public Coordinator1 coordinator { get; set; }
        //    public Enddevice1 enddevice { get; set; }
        //    public Router1 router { get; set; }
        //}

        //public class Coordinator1
        //{
        //    public string type { get; set; }
        //}

        //public class Enddevice1
        //{
        //    public string type { get; set; }
        //}

        //public class Router1
        //{
        //    public string type { get; set; }
        //}

        //public class Line1
        //{
        //    public Properties16 properties { get; set; }
        //    public string type { get; set; }
        //}

        //public class Properties16
        //{
        //    public Active1 active { get; set; }
        //    public Inactive inactive { get; set; }
        //}

        //public class Active1
        //{
        //    public string type { get; set; }
        //}

        //public class Inactive
        //{
        //    public string type { get; set; }
        //}

        //public class Mqtt1
        //{
        //    public Properties17 properties { get; set; }
        //    public string[] required { get; set; }
        //    public string title { get; set; }
        //    public string type { get; set; }
        //}

        //public class Properties17
        //{
        //    public Base_Topic base_topic { get; set; }
        //    public Ca ca { get; set; }
        //    public Cert cert { get; set; }
        //    public Client_Id client_id { get; set; }
        //    public Force_Disable_Retain force_disable_retain { get; set; }
        //    public Include_Device_Information include_device_information { get; set; }
        //    public Keepalive keepalive { get; set; }
        //    public Key key { get; set; }
        //    public Password password { get; set; }
        //    public Reject_Unauthorized reject_unauthorized { get; set; }
        //    public Server server { get; set; }
        //    public User user { get; set; }
        //    public Version version { get; set; }
        //}

        //public class Base_Topic
        //{
        //    public string _default { get; set; }
        //    public string description { get; set; }
        //    public string[] examples { get; set; }
        //    public bool requiresRestart { get; set; }
        //    public string title { get; set; }
        //    public string type { get; set; }
        //}

        //public class Ca
        //{
        //    public string description { get; set; }
        //    public string[] examples { get; set; }
        //    public bool requiresRestart { get; set; }
        //    public string title { get; set; }
        //    public string type { get; set; }
        //}

        //public class Cert
        //{
        //    public string description { get; set; }
        //    public string[] examples { get; set; }
        //    public bool requiresRestart { get; set; }
        //    public string title { get; set; }
        //    public string type { get; set; }
        //}

        //public class Client_Id
        //{
        //    public string description { get; set; }
        //    public string[] examples { get; set; }
        //    public bool requiresRestart { get; set; }
        //    public string title { get; set; }
        //    public string type { get; set; }
        //}

        //public class Force_Disable_Retain
        //{
        //    public bool _default { get; set; }
        //    public string description { get; set; }
        //    public bool requiresRestart { get; set; }
        //    public string title { get; set; }
        //    public string type { get; set; }
        //}

        //public class Include_Device_Information
        //{
        //    public bool _default { get; set; }
        //    public string description { get; set; }
        //    public string title { get; set; }
        //    public string type { get; set; }
        //}

        //public class Keepalive
        //{
        //    public int _default { get; set; }
        //    public string description { get; set; }
        //    public bool requiresRestart { get; set; }
        //    public string title { get; set; }
        //    public string type { get; set; }
        //}

        //public class Key
        //{
        //    public string description { get; set; }
        //    public string[] examples { get; set; }
        //    public bool requiresRestart { get; set; }
        //    public string title { get; set; }
        //    public string type { get; set; }
        //}

        //public class Password
        //{
        //    public string description { get; set; }
        //    public string[] examples { get; set; }
        //    public bool requiresRestart { get; set; }
        //    public string title { get; set; }
        //    public string type { get; set; }
        //}

        //public class Reject_Unauthorized
        //{
        //    public bool _default { get; set; }
        //    public string description { get; set; }
        //    public bool requiresRestart { get; set; }
        //    public string title { get; set; }
        //    public string type { get; set; }
        //}

        //public class Server
        //{
        //    public string description { get; set; }
        //    public string[] examples { get; set; }
        //    public bool requiresRestart { get; set; }
        //    public string title { get; set; }
        //    public string type { get; set; }
        //}

        //public class User
        //{
        //    public string description { get; set; }
        //    public string[] examples { get; set; }
        //    public bool requiresRestart { get; set; }
        //    public string title { get; set; }
        //    public string type { get; set; }
        //}

        //public class Version
        //{
        //    public int _default { get; set; }
        //    public string description { get; set; }
        //    public int[] examples { get; set; }
        //    public bool requiresRestart { get; set; }
        //    public string title { get; set; }
        //    public string[] type { get; set; }
        //}

        //public class Ota1
        //{
        //    public Properties18 properties { get; set; }
        //    public string title { get; set; }
        //    public string type { get; set; }
        //}

        //public class Properties18
        //{
        //    public Disable_Automatic_Update_Check disable_automatic_update_check { get; set; }
        //    public Ikea_Ota_Use_Test_Url ikea_ota_use_test_url { get; set; }
        //    public Update_Check_Interval update_check_interval { get; set; }
        //    public Zigbee_Ota_Override_Index_Location zigbee_ota_override_index_location { get; set; }
        //}

        //public class Disable_Automatic_Update_Check
        //{
        //    public bool _default { get; set; }
        //    public string description { get; set; }
        //    public string title { get; set; }
        //    public string type { get; set; }
        //}

        //public class Ikea_Ota_Use_Test_Url
        //{
        //    public bool _default { get; set; }
        //    public string description { get; set; }
        //    public bool requiresRestart { get; set; }
        //    public string title { get; set; }
        //    public string type { get; set; }
        //}

        //public class Update_Check_Interval
        //{
        //    public int _default { get; set; }
        //    public string description { get; set; }
        //    public string title { get; set; }
        //    public string type { get; set; }
        //}

        //public class Zigbee_Ota_Override_Index_Location
        //{
        //    public string description { get; set; }
        //    public string[] examples { get; set; }
        //    public bool requiresRestart { get; set; }
        //    public string title { get; set; }
        //    public string type { get; set; }
        //}

        //public class Passlist
        //{
        //    public string description { get; set; }
        //    public Items12 items { get; set; }
        //    public bool requiresRestart { get; set; }
        //    public string title { get; set; }
        //    public string type { get; set; }
        //}

        //public class Items12
        //{
        //    public string type { get; set; }
        //}

        //public class Permit_Join
        //{
        //    public bool _default { get; set; }
        //    public string description { get; set; }
        //    public string title { get; set; }
        //    public string type { get; set; }
        //}

        //public class Serial1
        //{
        //    public Properties19 properties { get; set; }
        //    public string title { get; set; }
        //    public string type { get; set; }
        //}

        //public class Properties19
        //{
        //    public Adapter adapter { get; set; }
        //    public Baudrate baudrate { get; set; }
        //    public Disable_Led disable_led { get; set; }
        //    public Port2 port { get; set; }
        //    public Rtscts rtscts { get; set; }
        //}

        //public class Adapter
        //{
        //    public string _default { get; set; }
        //    public string description { get; set; }
        //    public string[] _enum { get; set; }
        //    public bool requiresRestart { get; set; }
        //    public string title { get; set; }
        //    public string[] type { get; set; }
        //}

        //public class Baudrate
        //{
        //    public string description { get; set; }
        //    public int[] examples { get; set; }
        //    public bool requiresRestart { get; set; }
        //    public string title { get; set; }
        //    public string type { get; set; }
        //}

        //public class Disable_Led
        //{
        //    public bool _default { get; set; }
        //    public string description { get; set; }
        //    public bool requiresRestart { get; set; }
        //    public string title { get; set; }
        //    public string type { get; set; }
        //}

        //public class Port2
        //{
        //    public string description { get; set; }
        //    public string[] examples { get; set; }
        //    public bool requiresRestart { get; set; }
        //    public string title { get; set; }
        //    public string[] type { get; set; }
        //}

        //public class Rtscts
        //{
        //    public string description { get; set; }
        //    public bool requiresRestart { get; set; }
        //    public string title { get; set; }
        //    public string type { get; set; }
        //}

        //public class Whitelist
        //{
        //    public Items13 items { get; set; }
        //    public bool readOnly { get; set; }
        //    public bool requiresRestart { get; set; }
        //    public string title { get; set; }
        //    public string type { get; set; }
        //}

        //public class Items13
        //{
        //    public string type { get; set; }
        //}

        public class Coordinator
        {
            public string ieee_address { get; set; }
            public Meta meta { get; set; }
            public string type { get; set; }
        }

        public class Meta
        {
            public int maintrel { get; set; }
            public int majorrel { get; set; }
            public int minorrel { get; set; }
            public int product { get; set; }
            public int revision { get; set; }
            public int transportrev { get; set; }
        }

        public class Network
        {
            public int channel { get; set; }
            public string extended_pan_id { get; set; }
            public int pan_id { get; set; }
        }


    }
}
