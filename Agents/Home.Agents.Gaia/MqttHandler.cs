using MQTTnet;
using MQTTnet.Client.Options;
using MQTTnet.Extensions.ManagedClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Home.Agents.Gaia
{
    static class MqttHandler
    {
        static IManagedMqttClient _client = null;

        public static void Start()
        {
            string server = Environment.GetEnvironmentVariable("MOSQUITTO_SERVICE_HOST");
            if (server == null)
                return;

            var options = new ManagedMqttClientOptionsBuilder()
                .WithAutoReconnectDelay(TimeSpan.FromSeconds(5))
                .WithClientOptions(new MqttClientOptionsBuilder()
                    .WithClientId("Gaia")
                    .WithTcpServer(server)
                    .Build())
                .Build();

             _client = new MqttFactory().CreateManagedMqttClient();           
            _client.StartAsync(options).Wait();
        }

        public static void Publish(string topic, object content)
        {
            _client.PublishAsync(new MqttApplicationMessage()
            {
                ContentType = "application/json",
                Topic = "mesh/agents/gaia/" + topic,
                Payload = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(content)),
                PayloadFormatIndicator = MQTTnet.Protocol.MqttPayloadFormatIndicator.CharacterData
            }).Wait();
        }
        public static void Stop()
        {
            _client.StopAsync().Wait();
        }

    }
}
