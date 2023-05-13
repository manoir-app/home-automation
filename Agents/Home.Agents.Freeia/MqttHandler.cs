using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Threading.Tasks;

namespace Home.Agents.Freeia
{
    static class MqttHandler
    {
        static IManagedMqttClient _client = null;

        public static void Start()
        {
            string server = Environment.GetEnvironmentVariable("LAN_SERVER_IP");
#if DEBUG
            if (server == null)
                server = "192.168.2.184";
#endif
            if (server == null)
                return;

            Console.WriteLine("MQTT * settings : server = " + server);


            var options = new ManagedMqttClientOptionsBuilder()
                .WithAutoReconnectDelay(TimeSpan.FromSeconds(5))
                .WithClientOptions(new MqttClientOptionsBuilder()
                    .WithClientId("Freeia")
                    .WithTcpServer((options) =>
                    {
                        options.AddressFamily = System.Net.Sockets.AddressFamily.InterNetwork;
                        options.Server = server;
                        options.Port = 1883;
                    })
                    .WithCleanSession(true)
                    .Build())
                .Build();

            _client = new MqttFactory().CreateManagedMqttClient();
            _client.StartAsync(options).Wait();
            _client.ConnectingFailedAsync += _client_ConnectingFailedAsync;
            _client.ConnectedAsync += _client_ConnectedAsync;
            _client.ApplicationMessageProcessedAsync += _client_ApplicationMessageProcessedAsync;
            _client.ApplicationMessageSkippedAsync += _client_ApplicationMessageSkippedAsync;
            _client.DisconnectedAsync += _client_DisconnectedAsync;
        }

        private static Task _client_DisconnectedAsync(MQTTnet.Client.MqttClientDisconnectedEventArgs arg)
        {
            Console.WriteLine("MQTT * Déconnecté");
            return Task.CompletedTask;
        }

        private static Task _client_ApplicationMessageSkippedAsync(ApplicationMessageSkippedEventArgs arg)
        {
            Console.WriteLine("MQTT * Skipped");
            return Task.CompletedTask;
        }

        private static Task _client_ApplicationMessageProcessedAsync(ApplicationMessageProcessedEventArgs arg)
        {
            if (arg.Exception != null)
            {
                Console.WriteLine("MQTT * Erreur - " + JsonConvert.SerializeObject(arg.ApplicationMessage.ApplicationMessage) + arg.Exception?.ToString());
            }

            return Task.CompletedTask;
        }

        private static Task _client_ConnectedAsync(MQTTnet.Client.MqttClientConnectedEventArgs arg)
        {
            Console.WriteLine("MQTT * Connecté : " + arg.ConnectResult);
            return Task.CompletedTask;
        }

        private static Task _client_ConnectingFailedAsync(ConnectingFailedEventArgs arg)
        {
            throw new NotImplementedException(); Console.WriteLine("MQTT * Connection failed : " + arg.Exception.ToString());
            return Task.CompletedTask;
        }
        public static void PublishNetworkStatus(string content, bool isUp, long bandwith_up, long bandwith_down, long rate_up, long rate_down, string[] ssids)
        {
            List<MqttApplicationMessage> msgs = new List<MqttApplicationMessage>();
            _client.EnqueueAsync(new MqttApplicationMessageBuilder()
                .WithTopic("mesh/network/freebox/status")
                .WithPayload(content)
                .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                .WithRetainFlag().Build()).Wait();

            _client.EnqueueAsync(new MqttApplicationMessageBuilder()
                .WithTopic("mesh/network/freebox/up")
                .WithPayload(isUp.ToString())
                .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                .WithRetainFlag().Build()).Wait();

            _client.EnqueueAsync(new MqttApplicationMessageBuilder()
                .WithTopic("mesh/network/freebox/bandwidth/used/up")
                .WithPayload(rate_up.ToString("0"))
                .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                .Build()).Wait();

            _client.EnqueueAsync(new MqttApplicationMessageBuilder()
                .WithTopic("mesh/network/freebox/bandwidth/used/down")
                .WithPayload(rate_down.ToString("0"))
                .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce).Build()).Wait();

            _client.EnqueueAsync(new MqttApplicationMessageBuilder()
                .WithTopic("mesh/network/freebox/bandwidth/up")
                .WithPayload(bandwith_up.ToString("0"))
                .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce).Build()).Wait();

            _client.EnqueueAsync(new MqttApplicationMessageBuilder()
                .WithTopic("mesh/network/freebox/bandwidth/down")
                .WithPayload(bandwith_down.ToString("0"))
                .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce).Build()).Wait();

            if (ssids != null && ssids.Length > 0)
            {
                _client.EnqueueAsync(new MqttApplicationMessageBuilder()
                .WithTopic("mesh/network/wifi/mainSsid")
                .WithPayload(ssids.FirstOrDefault())
                .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                .WithRetainFlag().Build()).Wait();
            }

            Console.WriteLine("Publishing networkstatus : " + content);

        }

        public static void PublishTest(string pth, string content)
        {
            MqttApplicationMessageBuilder msg1 = new MqttApplicationMessageBuilder()
                .WithTopic("tests/" + pth)
                .WithPayload(content);

            _client.EnqueueAsync(msg1.Build()).Wait();
        }

        public static void PublishDeviceStatus(string deviceName, bool isReachable, string mainIpv4, string mainIpv6, string name)
        {
            MqttApplicationMessageBuilder msg1 = new MqttApplicationMessageBuilder()
                .WithTopic("mesh/network/appliances/" + deviceName + "/reachable")
                .WithPayload(isReachable.ToString())
                .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                .WithRetainFlag();
            List<MqttApplicationMessage> msgs = new List<MqttApplicationMessage>();
            _client.EnqueueAsync(msg1.Build()).Wait();

            if (!string.IsNullOrEmpty(mainIpv4))
            {
                _client.EnqueueAsync(new MqttApplicationMessageBuilder()
                .WithTopic("mesh/network/appliances/" + deviceName + "/ipv4")
                .WithPayload(mainIpv4.ToString())
                .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                .WithRetainFlag()
                .Build()).Wait();
            }

            if (!string.IsNullOrEmpty(name))
            {
                _client.EnqueueAsync(new MqttApplicationMessageBuilder()
                .WithTopic("mesh/network/appliances/" + deviceName + "/name")
                .WithPayload(name.ToString())
                .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                .WithRetainFlag()
                .Build()).Wait();
            }

            if (!string.IsNullOrEmpty(mainIpv6))
            {
                _client.EnqueueAsync(new MqttApplicationMessageBuilder()
                .WithTopic("mesh/network/appliances/" + deviceName + "/ipv6")
                .WithPayload(mainIpv6.ToString())
                .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                .WithRetainFlag()
                .Build()).Wait();
            }

        }


        public static void PublishExternalSsid(string ssid, int signal)
        {
            MqttApplicationMessageBuilder msg1 = new MqttApplicationMessageBuilder()
                .WithTopic("mesh/network/wifi/otherSsid/" + ssid + "/ssid")
                .WithPayload(ssid.ToString())
                .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce);
            List<MqttApplicationMessage> msgs = new List<MqttApplicationMessage>();
            _client.EnqueueAsync(msg1.Build());

            _client.EnqueueAsync(new MqttApplicationMessageBuilder()
            .WithTopic("mesh/network/wifi/otherSsid/" + ssid + "/signal")
            .WithPayload(signal.ToString())
            .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
            .Build()).Wait();

        }

        public static void Stop()
        {
            _client.StopAsync().Wait();
        }

    }
}
