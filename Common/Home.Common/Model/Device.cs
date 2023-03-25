using Home.Common.HomeAutomation;
using System;
using System.Collections.Generic;

namespace Home.Common.Model
{
    public class Device
    {
        public Device()
        {
            DeviceRoles = new List<string>();
            DeviceAddresses = new List<string>();
            Datas = new List<DeviceData>();
            SupportPrivacyMode = true;
            Images = new Dictionary<string, string>();
            SecondaryDatas = new List<DeviceData>();
        }

        public Device(DeviceBase fromAutomationDevice, string managingAgent = "sarah") : this()
        {
            DeviceInternalName = fromAutomationDevice.DeviceName;
            DeviceKind = Device.DeviceKindHomeAutomation;

            this.DeviceRoles.AddRange(fromAutomationDevice.GetRoles());
            this.SecondaryDatas = fromAutomationDevice.SecondaryProperties;
        }


        public const string DeviceKindMainServer = "home-graph";

        public const string DeviceKindHomeAutomation = "homeautomation";
        public const string DeviceKindDisplay = "display";
        public const string DeviceKindSecurity = "security";
        

        public const string DeviceKindNetwork = "network";
        public const string DeviceKindMobileDevice = "mobiledevice";

        public const string HomeAutomationMainRoleBridge = "main:bridge";
        public const string HomeAutomationMainRoleLight = "main:light";
        public const string HomeAutomationMainRoleShutterSwitch = "main:shutters";
        public const string HomeAutomationMainRoleSensors = "main:sensors";

        public const string HomeAutomationRoleSwitch = "switch";
        public const string HomeAutomationRoleColorBound = "color-bound";
        public const string HomeAutomationRoleDimmer = "dimmer";

        public const string HomeAutomationRoleActionnable = "actionnable";

        public const string DisplayRoleImageDisplay = "image-display";

        public string Id { get; set; }
        public string MeshId { get; set; }

        public string DeviceInternalName { get; set; }

        public string DeviceGivenName { get; set; }

        public string DeviceAgentId { get; set; }
        public string DevicePlatform { get; set; }

        public string DeviceKind { get; set; }
        public List<string> DeviceRoles { get; set; }
        public List<string> DeviceAddresses { get; set; }

        /// <summary>
        /// si non <c>null</c> l'id de l'intégration ayant
        /// créé ce device
        /// </summary>
        public string IntegrationId { get; set; }
        public string IntegrationInstanceId { get; set; }

        public string MainStatusInfo { get; set; }
        public List<DeviceData> Datas { get; set; }
        public List<DeviceData> SecondaryDatas { get; set; }

        public string AssignatedUserId { get; set; }

        public string ConfigurationData { get; set; }

        public AutomationMeshPrivacyMode? CurrentPrivacyMode { get; set; }

        public bool SupportPrivacyMode { get; set; }
        public bool IsIgnored { get; set; }
        public string SameAsDeviceId { get; set; }

        public DeviceUsageLevel UsageLevel { get; set; }

        public Dictionary<string, string> Images { get; set; }
    }

    public enum DeviceUsageLevel
    {
        Normal = 0,
        Secondary = 1,
        SystemOnly = 2
    }

    public enum DeviceActionnableActionType
    {
        None,
        Trigger
    }

    public class DeviceActionnable
    {
        public const string ActionnableTypeSwitch = "on/off";
        public const string ActionnableTypePushButton = "push";

        public bool IsMainData { get; set; }
        public string Name { get; set; }
        public string StandardActionnableType { get; set; }

        public DeviceActionnableActionType ActionType { get; set; }
        public string ActionParameter { get; set; }

        public static string ToUrl(DeviceActionnableActionType type, string parameter)
        {
            var srv = "home.anzin.carbenay.manoir.app";// HomeServerHelper.GetLocalIP();
            switch (type)
            {
                case DeviceActionnableActionType.None:
                    return "";
                case DeviceActionnableActionType.Trigger:
                    return $"https://{srv}/v1.0/system/mesh/local/triggers/{parameter}/raise";
            }
            return "";
        }

        public void FillActionFromUrl(string url)
        {
            var uri = new Uri(url);
            url = uri.AbsolutePath.ToLowerInvariant();
            if (url.StartsWith("/v1.0/system/mesh/local/triggers/") && url.EndsWith("/raise"))
            {
                ActionType = DeviceActionnableActionType.Trigger;
                ActionParameter = url.Substring("/v1.0/system/mesh/local/triggers/".Length);
                ActionParameter = ActionParameter.Substring(0, ActionParameter.IndexOf("/"));
            }
        }
    }


    public class DeviceData
    {
        public const string DataTypeSwitch = "on/off";
        public const string DataTypeShutter = "up/down";
        public const string DataTypeGradient = "gradient";
        public const string DataTypeColor = "color";

        public const string DataTypeSensorTemperature = "temperature";
        public const string DataTypeSensorHumidity = "humidity";
        public const string DataTypeSensorPressure = "pressure";


        // les propriétés secondaires
        public const string DataTypeAlimentationType = "alimentation";
        public const string DataTypeBatteryPercentage = "battery_percentage";
        public const string DataTypeInternalTemperature = "device_temp";
        public const string DataTypeLinkSignalStrength = "link_quality";


        public bool IsMainData { get; set; }
        public string Name { get; set; }
        public string StandardDataType { get; set; }
        public string Value { get; set; }
        public string ValueUnit { get; set; }
        public string MinValue { get; set; }
        public string MaxValue { get; set; }
        public string[] ValidValues { get; set; }

    }
}
