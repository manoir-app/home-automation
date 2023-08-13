using Home.Common;
using Home.Common.Model;
using MQTTnet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Home.Graph.Common
{
    public static class NetworkConnectionHelper
    {
        private static DateTime _lastRefresh = DateTime.MinValue;
        private static string _agentName = null;
        private static AutomationMesh _mesh = null;

        public static void Init(string agentName)
        {
            _agentName = agentName;
        }

        private static bool IsMainConnection(string connectionName)
        {
            if(Math.Abs((DateTime.Now - _lastRefresh).TotalMinutes)>10)
            {
                var tmp = AgentHelper.GetLocalMesh(_agentName);
                if (tmp != null)
                    _mesh = tmp;
            }

            if (_mesh == null || _mesh.InternetConnections==null)
                return true;

            var t = (from z in _mesh.InternetConnections where z.IsMain select z).FirstOrDefault();
            if (t == null)
                return true;
            if (t.Id.Equals(connectionName, StringComparison.InvariantCultureIgnoreCase))
                return true;

            return false;
        }

        public static void PublishNetworkStatus(string connectionName, string content, bool isUp, long bandwith_up, long bandwith_down, long rate_up, long rate_down, string[] ssids)
        {
            bool isMain = IsMainConnection(connectionName);

            List<MqttApplicationMessage> msgs = new List<MqttApplicationMessage>();
            MqttHelper.Client.EnqueueAsync(new MqttApplicationMessageBuilder()
                .WithTopic($"manoir/network/{connectionName}/status")
                .WithPayload(content)
                .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                .WithRetainFlag().Build()).Wait();
            if (isMain)
            {
                MqttHelper.Client.EnqueueAsync(new MqttApplicationMessageBuilder()
                    .WithTopic($"manoir/network/internet-router/status")
                    .WithPayload(content)
                    .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                    .WithRetainFlag().Build()).Wait();
                MqttHelper.Client.EnqueueAsync(new MqttApplicationMessageBuilder()
                    .WithTopic($"manoir/network/internet-router/connectionId")
                    .WithPayload(connectionName)
                    .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                    .WithRetainFlag().Build()).Wait();
            }

            MqttHelper.Client.EnqueueAsync(new MqttApplicationMessageBuilder()
                .WithTopic($"manoir/network/{connectionName}/up")
                .WithPayload(isUp.ToString())
                .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                .WithRetainFlag().Build()).Wait();

            MqttHelper.Client.EnqueueAsync(new MqttApplicationMessageBuilder()
                .WithTopic($"manoir/network/{connectionName}/bandwidth/used/up")
                .WithPayload(rate_up.ToString("0"))
                .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                .Build()).Wait();

            MqttHelper.Client.EnqueueAsync(new MqttApplicationMessageBuilder()
                .WithTopic($"manoir/network/{connectionName}/bandwidth/used/down")
                .WithPayload(rate_down.ToString("0"))
                .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce).Build()).Wait();

            MqttHelper.Client.EnqueueAsync(new MqttApplicationMessageBuilder()
                .WithTopic($"manoir/network/{connectionName}/bandwidth/up")
                .WithPayload(bandwith_up.ToString("0"))
                .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce).Build()).Wait();

            MqttHelper.Client.EnqueueAsync(new MqttApplicationMessageBuilder()
                .WithTopic($"manoir/network/{connectionName}/bandwidth/down")
                .WithPayload(bandwith_down.ToString("0"))
                .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce).Build()).Wait();

            if (ssids != null && ssids.Length > 0)
            {
                MqttHelper.Client.EnqueueAsync(new MqttApplicationMessageBuilder()
                .WithTopic($"manoir/network/wifi/mainSsid")
                .WithPayload(ssids.FirstOrDefault())
                .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                .WithRetainFlag().Build()).Wait();
            }

            Console.WriteLine("Publishing networkstatus : " + content);

        }

        public static void PublishDeviceStatus(string deviceName, bool isReachable, string mainIpv4, string mainIpv6, string name)
        {
            MqttApplicationMessageBuilder msg1 = new MqttApplicationMessageBuilder()
                .WithTopic($"manoir/network/appliances/" + deviceName + "/reachable")
                .WithPayload(isReachable.ToString())
                .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                .WithRetainFlag();
            List<MqttApplicationMessage> msgs = new List<MqttApplicationMessage>();
            MqttHelper.Client.EnqueueAsync(msg1.Build()).Wait();

            if (!string.IsNullOrEmpty(mainIpv4))
            {
                MqttHelper.Client.EnqueueAsync(new MqttApplicationMessageBuilder()
                .WithTopic($"manoir/network/appliances/" + deviceName + "/ipv4")
                .WithPayload(mainIpv4.ToString())
                .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                .WithRetainFlag()
                .Build()).Wait();
            }

            if (!string.IsNullOrEmpty(name))
            {
                MqttHelper.Client.EnqueueAsync(new MqttApplicationMessageBuilder()
                .WithTopic($"manoir/network/appliances/" + deviceName + "/name")
                .WithPayload(name.ToString())
                .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                .WithRetainFlag()
                .Build()).Wait();
            }

            if (!string.IsNullOrEmpty(mainIpv6))
            {
                MqttHelper.Client.EnqueueAsync(new MqttApplicationMessageBuilder()
                .WithTopic($"manoir/network/appliances/" + deviceName + "/ipv6")
                .WithPayload(mainIpv6.ToString())
                .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                .WithRetainFlag()
                .Build()).Wait();
            }

        }


        public static void PublishExternalSsid(string ssid, int signal)
        {
            MqttApplicationMessageBuilder msg1 = new MqttApplicationMessageBuilder()
                .WithTopic($"manoir/network/wifi/otherSsid/" + ssid + "/ssid")
                .WithPayload(ssid.ToString())
                .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce);
            List<MqttApplicationMessage> msgs = new List<MqttApplicationMessage>();
            MqttHelper.Client.EnqueueAsync(msg1.Build());

            MqttHelper.Client.EnqueueAsync(new MqttApplicationMessageBuilder()
            .WithTopic($"manoir/network/wifi/otherSsid/" + ssid + "/signal")
            .WithPayload(signal.ToString())
            .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
            .Build()).Wait();

        }

    }
}
