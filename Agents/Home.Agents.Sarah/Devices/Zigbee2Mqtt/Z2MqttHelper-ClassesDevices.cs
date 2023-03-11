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

        public class DeviceInfo
        {
            public DeviceDefinition definition { get; set; }
            public bool disabled { get; set; }
            //public Endpoints endpoints { get; set; }
            public string friendly_name { get; set; }
            public string ieee_address { get; set; }
            public bool interview_completed { get; set; }
            public bool interviewing { get; set; }
            public int network_address { get; set; }
            public bool supported { get; set; }
            public string type { get; set; }
            public string date_code { get; set; }
            public string manufacturer { get; set; }
            public string model_id { get; set; }
            public string power_source { get; set; }
            public string software_build_id { get; set; }
        }

        public class DeviceDefinition
        {
            public string description { get; set; }
            public DeviceExpose[] exposes { get; set; }
            public string model { get; set; }
            public Option[] options { get; set; }
            public bool supports_ota { get; set; }
            public string vendor { get; set; }
        }

        public class DeviceExpose
        {
            public int access { get; set; }
            public string description { get; set; }
            public string name { get; set; }
            public string property { get; set; }
            public string type { get; set; }
            public string unit { get; set; }
            public int value_max { get; set; }
            public int value_min { get; set; }
            public DeviceFeature[] features { get; set; }
            public string[] values { get; set; }
            public bool value_off { get; set; }
            public bool value_on { get; set; }
            public int value_step { get; set; }
        }

        public class DeviceFeature
        {
            public int access { get; set; }
            public string description { get; set; }
            public string name { get; set; }
            public string property { get; set; }
            public string type { get; set; }
            public string value_off { get; set; }
            public string value_on { get; set; }
            public string value_toggle { get; set; }
            public int? value_max { get; set; }
            public int? value_min { get; set; }
            public Preset[] presets { get; set; }
            public string unit { get; set; }
            public Feature2[] features { get; set; }
            public float? value_step { get; set; }
            public string[] values { get; set; }
            public DeviceSubFeature item_type { get; set; }
        }

        public class DeviceSubFeature
        {
            public DeviceFeature[] features { get; set; }
            public string name { get; set; }
            public string type { get; set; }
        }


        public class Preset
        {
            public string description { get; set; }
            public string name { get; set; }
            public int value { get; set; }
        }

        public class Feature2
        {
            public int access { get; set; }
            public string name { get; set; }
            public string property { get; set; }
            public string type { get; set; }
        }

        public class Option
        {
            public int access { get; set; }
            public string description { get; set; }
            public string name { get; set; }
            public string property { get; set; }
            public string type { get; set; }
            public int value_max { get; set; }
            public int value_min { get; set; }
            public bool value_off { get; set; }
            public bool value_on { get; set; }
        }

        //public class Endpoints
        //{
        //    public _1 _1 { get; set; }
        //    public _10 _10 { get; set; }
        //    public _11 _11 { get; set; }
        //    public _110 _110 { get; set; }
        //    public _12 _12 { get; set; }
        //    public _13 _13 { get; set; }
        //    public _2 _2 { get; set; }
        //    public _242 _242 { get; set; }
        //    public _3 _3 { get; set; }
        //    public _4 _4 { get; set; }
        //    public _47 _47 { get; set; }
        //    public _5 _5 { get; set; }
        //    public _6 _6 { get; set; }
        //    public _8 _8 { get; set; }
        //}

        //public class _1
        //{
        //    public object[] bindings { get; set; }
        //    public Clusters clusters { get; set; }
        //    public object[] configured_reportings { get; set; }
        //    public object[] scenes { get; set; }
        //}

        //public class Clusters
        //{
        //    public string[] input { get; set; }
        //    public string[] output { get; set; }
        //}

        //public class _10
        //{
        //    public object[] bindings { get; set; }
        //    public Clusters1 clusters { get; set; }
        //    public object[] configured_reportings { get; set; }
        //    public object[] scenes { get; set; }
        //}

        //public class Clusters1
        //{
        //    public object[] input { get; set; }
        //    public object[] output { get; set; }
        //}

        //public class _11
        //{
        //    public Binding[] bindings { get; set; }
        //    public Clusters2 clusters { get; set; }
        //    public object[] configured_reportings { get; set; }
        //    public object[] scenes { get; set; }
        //}

        //public class Clusters2
        //{
        //    public string[] input { get; set; }
        //    public string[] output { get; set; }
        //}

        //public class Binding
        //{
        //    public string cluster { get; set; }
        //    public Target target { get; set; }
        //}

        //public class Target
        //{
        //    public int endpoint { get; set; }
        //    public string ieee_address { get; set; }
        //    public string type { get; set; }
        //}

        //public class _110
        //{
        //    public object[] bindings { get; set; }
        //    public Clusters3 clusters { get; set; }
        //    public object[] configured_reportings { get; set; }
        //    public object[] scenes { get; set; }
        //}

        //public class Clusters3
        //{
        //    public object[] input { get; set; }
        //    public object[] output { get; set; }
        //}

        //public class _12
        //{
        //    public object[] bindings { get; set; }
        //    public Clusters4 clusters { get; set; }
        //    public object[] configured_reportings { get; set; }
        //    public object[] scenes { get; set; }
        //}

        //public class Clusters4
        //{
        //    public object[] input { get; set; }
        //    public object[] output { get; set; }
        //}

        //public class _13
        //{
        //    public object[] bindings { get; set; }
        //    public Clusters5 clusters { get; set; }
        //    public object[] configured_reportings { get; set; }
        //    public object[] scenes { get; set; }
        //}

        //public class Clusters5
        //{
        //    public string[] input { get; set; }
        //    public object[] output { get; set; }
        //}

        //public class _2
        //{
        //    public object[] bindings { get; set; }
        //    public Clusters6 clusters { get; set; }
        //    public object[] configured_reportings { get; set; }
        //    public object[] scenes { get; set; }
        //}

        //public class Clusters6
        //{
        //    public string[] input { get; set; }
        //    public string[] output { get; set; }
        //}

        //public class _242
        //{
        //    public object[] bindings { get; set; }
        //    public Clusters7 clusters { get; set; }
        //    public object[] configured_reportings { get; set; }
        //    public object[] scenes { get; set; }
        //}

        //public class Clusters7
        //{
        //    public object[] input { get; set; }
        //    public string[] output { get; set; }
        //}

        //public class _3
        //{
        //    public object[] bindings { get; set; }
        //    public Clusters8 clusters { get; set; }
        //    public object[] configured_reportings { get; set; }
        //    public object[] scenes { get; set; }
        //}

        //public class Clusters8
        //{
        //    public object[] input { get; set; }
        //    public object[] output { get; set; }
        //}

        //public class _4
        //{
        //    public object[] bindings { get; set; }
        //    public Clusters9 clusters { get; set; }
        //    public object[] configured_reportings { get; set; }
        //    public object[] scenes { get; set; }
        //}

        //public class Clusters9
        //{
        //    public object[] input { get; set; }
        //    public object[] output { get; set; }
        //}

        //public class _47
        //{
        //    public object[] bindings { get; set; }
        //    public Clusters10 clusters { get; set; }
        //    public object[] configured_reportings { get; set; }
        //    public object[] scenes { get; set; }
        //}

        //public class Clusters10
        //{
        //    public object[] input { get; set; }
        //    public object[] output { get; set; }
        //}

        //public class _5
        //{
        //    public object[] bindings { get; set; }
        //    public Clusters11 clusters { get; set; }
        //    public object[] configured_reportings { get; set; }
        //    public object[] scenes { get; set; }
        //}

        //public class Clusters11
        //{
        //    public object[] input { get; set; }
        //    public object[] output { get; set; }
        //}

        //public class _6
        //{
        //    public object[] bindings { get; set; }
        //    public Clusters12 clusters { get; set; }
        //    public object[] configured_reportings { get; set; }
        //    public object[] scenes { get; set; }
        //}

        //public class Clusters12
        //{
        //    public object[] input { get; set; }
        //    public object[] output { get; set; }
        //}

        //public class _8
        //{
        //    public object[] bindings { get; set; }
        //    public Clusters13 clusters { get; set; }
        //    public object[] configured_reportings { get; set; }
        //    public object[] scenes { get; set; }
        //}

        //public class Clusters13
        //{
        //    public object[] input { get; set; }
        //    public object[] output { get; set; }
        //}


    }
}
