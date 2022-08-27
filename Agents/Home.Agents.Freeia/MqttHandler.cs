using MQTTnet;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Options;
using MQTTnet.Extensions.ManagedClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
                    .WithClientId("Gaia")
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
            _client.ConnectingFailedHandler = new Temp();
            _client.UseConnectedHandler(new Temp());
        }

        public class Temp : IApplicationMessageProcessedHandler, IMqttClientConnectedHandler, IMqttClientDisconnectedHandler, IApplicationMessageSkippedHandler, IConnectingFailedHandler
        {
            public Task HandleApplicationMessageProcessedAsync(ApplicationMessageProcessedEventArgs eventArgs)
            {
                if (eventArgs.HasFailed)
                {
                    Console.WriteLine("MQTT * Erreur - " + JsonConvert.SerializeObject(eventArgs.ApplicationMessage.ApplicationMessage) + eventArgs.Exception?.ToString());
                }
                else
                    Console.WriteLine("MQTT * OK - " + JsonConvert.SerializeObject(eventArgs.ApplicationMessage.ApplicationMessage));

                return Task.CompletedTask;
            }

            public Task HandleApplicationMessageSkippedAsync(ApplicationMessageSkippedEventArgs eventArgs)
            {
                Console.WriteLine("MQTT * Skipped");
                return Task.CompletedTask;
            }

            public Task HandleConnectedAsync(MqttClientConnectedEventArgs eventArgs)
            {
                Console.WriteLine("MQTT * Connecté");

                return Task.CompletedTask;
            }

            public Task HandleConnectingFailedAsync(ManagedProcessFailedEventArgs eventArgs)
            {
                Console.WriteLine("MQTT * Connection failed : " + eventArgs.Exception.ToString());
                return Task.CompletedTask;
            }

            public Task HandleDisconnectedAsync(MqttClientDisconnectedEventArgs eventArgs)
            {
                Console.WriteLine("MQTT * Déconnecté");
                return Task.CompletedTask;
            }
        }


        public static void PublishNetworkStatus(string content, bool isUp, long bandwith_up, long bandwith_down, long rate_up, long rate_down, string[] ssids)
        {
            List<MqttApplicationMessage> msgs = new List<MqttApplicationMessage>();
            msgs.Add(new MqttApplicationMessageBuilder()
                .WithTopic("mesh/network/freebox/status")
                .WithPayload(content)
                .WithAtLeastOnceQoS()
                .WithRetainFlag().Build());

            msgs.Add(new MqttApplicationMessageBuilder()
                .WithTopic("mesh/network/freebox/up")
                .WithPayload(isUp.ToString())
                .WithAtLeastOnceQoS()
                .WithRetainFlag().Build());

            msgs.Add(new MqttApplicationMessageBuilder()
                .WithTopic("mesh/network/freebox/bandwidth/used/up")
                .WithPayload(rate_up.ToString("0"))
                .WithAtLeastOnceQoS()
                .Build());

            msgs.Add(new MqttApplicationMessageBuilder()
                .WithTopic("mesh/network/freebox/bandwidth/used/down")
                .WithPayload(rate_down.ToString("0"))
                .WithAtLeastOnceQoS().Build());

            msgs.Add(new MqttApplicationMessageBuilder()
                .WithTopic("mesh/network/freebox/bandwidth/up")
                .WithPayload(bandwith_up.ToString("0"))
                .WithAtLeastOnceQoS().Build());

            msgs.Add(new MqttApplicationMessageBuilder()
                .WithTopic("mesh/network/freebox/bandwidth/down")
                .WithPayload(bandwith_down.ToString("0"))
                .WithAtLeastOnceQoS().Build());

            if (ssids != null && ssids.Length > 0)
            {
                msgs.Add(new MqttApplicationMessageBuilder()
                .WithTopic("mesh/network/wifi/mainSsid")
                .WithPayload(ssids.FirstOrDefault())
                .WithAtLeastOnceQoS()
                .WithRetainFlag().Build());
            }

            Console.WriteLine("Publishing networkstatus : " + content);

            _client.PublishAsync(msgs).Wait();
        }

        public static void PublishTest(string pth, string content)
        {
            MqttApplicationMessageBuilder msg1 = new MqttApplicationMessageBuilder()
                .WithTopic("tests/" + pth)
                .WithPayload(content);

            _client.PublishAsync(msg1.Build()).Wait();
        }

        public static void PublishDeviceStatus(string deviceName, bool isReachable, string mainIpv4, string mainIpv6, string name)
        {
            MqttApplicationMessageBuilder msg1 = new MqttApplicationMessageBuilder()
                .WithTopic("mesh/network/appliances/" + deviceName + "/reachable")
                .WithPayload(isReachable.ToString())
                .WithAtLeastOnceQoS()
                .WithRetainFlag();
            List<MqttApplicationMessage> msgs = new List<MqttApplicationMessage>();
            msgs.Add(msg1.Build());

            if (!string.IsNullOrEmpty(mainIpv4))
            {
                msgs.Add(new MqttApplicationMessageBuilder()
                .WithTopic("mesh/network/appliances/" + deviceName + "/ipv4")
                .WithPayload(mainIpv4.ToString())
                .WithAtLeastOnceQoS()
                .WithRetainFlag()
                .Build());
            }

            if (!string.IsNullOrEmpty(name))
            {
                msgs.Add(new MqttApplicationMessageBuilder()
                .WithTopic("mesh/network/appliances/" + deviceName + "/name")
                .WithPayload(name.ToString())
                .WithAtLeastOnceQoS()
                .WithRetainFlag()
                .Build());
            }

            if (!string.IsNullOrEmpty(mainIpv6))
            {
                msgs.Add(new MqttApplicationMessageBuilder()
                .WithTopic("mesh/network/appliances/" + deviceName + "/ipv6")
                .WithPayload(mainIpv6.ToString())
                .WithAtLeastOnceQoS()
                .WithRetainFlag()
                .Build());
            }

            _client.PublishAsync(msgs).Wait();

        }


        public static void PublishExternalSsid(string ssid, int signal)
        {
            MqttApplicationMessageBuilder msg1 = new MqttApplicationMessageBuilder()
                .WithTopic("mesh/network/wifi/otherSsid/" + ssid + "/ssid")
                .WithPayload(ssid.ToString())
                .WithAtLeastOnceQoS();
            List<MqttApplicationMessage> msgs = new List<MqttApplicationMessage>();
            msgs.Add(msg1.Build());

            msgs.Add(new MqttApplicationMessageBuilder()
            .WithTopic("mesh/network/wifi/otherSsid/" + ssid + "/signal")
            .WithPayload(signal.ToString())
            .WithAtLeastOnceQoS()
            .Build());
            _client.PublishAsync(msgs).Wait();

        }

        public static void Stop()
        {
            _client.StopAsync().Wait();
        }

    }
}
