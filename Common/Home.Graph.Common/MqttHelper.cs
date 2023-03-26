using Home.Common;
using Home.Common.Model;
using MongoDB.Bson;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Globalization;
using System.Threading.Tasks;

namespace Home.Graph.Common
{
    public static class MqttHelper
    {
        static IManagedMqttClient _client = null;

        public static void Start(string name)
        {
            if (_client != null)
                return;

            Console.WriteLine("MQTT * Start ");

            string server = null; // LocalDebugHelper.GetLocalServiceHost();
            //if (server == null)
            //    server = Environment.GetEnvironmentVariable("MOSQUITTO_SERVICE_HOST");
            if (server == null)
                server = HomeServerHelper.GetLocalIP();
            if (server == null)
                return;

            var options = new ManagedMqttClientOptionsBuilder()
                .WithAutoReconnectDelay(TimeSpan.FromSeconds(5))
                .WithClientOptions(new MqttClientOptionsBuilder()
                    .WithClientId(name + "-" + Environment.MachineName)
                    .WithTcpServer((opts) =>
                    {
                        opts.AddressFamily = System.Net.Sockets.AddressFamily.InterNetwork;
                        opts.Server = server;
                        opts.Port = 1883;
                    })
                    .WithKeepAlivePeriod(TimeSpan.FromMinutes(10))
                    .Build())
                .Build();

            _client = new MqttFactory().CreateManagedMqttClient();
            _client.ApplicationMessageReceivedAsync += _client_ApplicationMessageReceivedAsync;
            _client.ConnectingFailedAsync += _client_ConnectingFailedAsync;
            _client.ConnectedAsync += _client_ConnectedAsync;
            _client.ApplicationMessageProcessedAsync += _client_ApplicationMessageProcessedAsync;
            _client.ApplicationMessageSkippedAsync += _client_ApplicationMessageSkippedAsync;
            _client.DisconnectedAsync += _client_DisconnectedAsync;
            _client.SynchronizingSubscriptionsFailedAsync += _client_SynchronizingSubscriptionsFailedAsync;
            _client.StartAsync(options).Wait();
        }

        private static Task _client_SynchronizingSubscriptionsFailedAsync(ManagedProcessFailedEventArgs arg)
        {
            Console.WriteLine("MQTT * Err de sync sub : " + arg.Exception);
            return Task.CompletedTask;
        }

        private static Task _client_DisconnectedAsync(MQTTnet.Client.MqttClientDisconnectedEventArgs arg)
        {
            if (arg.Exception == null)
                Console.WriteLine("MQTT * Déconnecté : " + arg.ReasonString);
            else
                Console.WriteLine("MQTT * Déconnecté : " + arg.ReasonString + "/" + arg.Exception);
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
            else
                Console.WriteLine("MQTT * OK - " + JsonConvert.SerializeObject(arg.ApplicationMessage.ApplicationMessage));

            return Task.CompletedTask;
        }

        private static Task _client_ConnectedAsync(MQTTnet.Client.MqttClientConnectedEventArgs arg)
        {
            Console.WriteLine("MQTT * Connecté : " + arg.ConnectResult.ReasonString);
            return Task.CompletedTask;
        }

        private static Task _client_ConnectingFailedAsync(ConnectingFailedEventArgs arg)
        {
            Console.WriteLine("MQTT * Connection failed : " + arg.Exception.ToString());
            return Task.CompletedTask;
        }



        public static Dictionary<string, List<Action<string, string>>> _handlers = new Dictionary<string, List<Action<string, string>>>();


        private static Task _client_ApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs arg)
        {
            var msg = arg.ApplicationMessage;
            var t = msg.Topic;
            if (t == null) return Task.CompletedTask;

            List<Action<string, string>> handlers = null;
            if (!_handlers.TryGetValue(t, out handlers))
            {
                foreach (var k in _handlers.Keys)
                {
                    string[] parts = k.Split(new char[] { '#', '+' }, StringSplitOptions.RemoveEmptyEntries);
                    string curPath = t;
                    bool isMatch = true;
                    // v1 : on ne s'embete pas avec # ou + 
                    // on match si toutes les parts sont là, dans l'ordre
                    foreach(var p in parts)
                    {
                        int curIndex = curPath.IndexOf(p);
                        if(curIndex < 0)
                        {
                            isMatch = false;
                            break;
                        }

                        curPath = curPath.Substring(curIndex + p.Length);
                    }

                    if(isMatch)
                        handlers = _handlers[k];
                }
            }

            if(handlers!=null)
            {
                foreach (var handler in handlers)
                {
                    t = msg.ConvertPayloadToString();
                    if (t != null)
                        handler.Invoke(msg.Topic, t);
                }
            }


            return Task.CompletedTask;
        }



        public static void AddChangeHandler(string topic, Action<string, string> handler)
        {
            List<Action<string, string>> handlers;
            if (!_handlers.TryGetValue(topic, out handlers))
                _handlers[topic] = new List<Action<string, string>>();
            _handlers[topic].Add(handler);
            Console.WriteLine("MQTTHelper - subscription to " + topic);
            _client.SubscribeAsync(topic).Wait();
        }

        public static void RemoveChangeHandler(string topic, Action<string, string> handler)
        {
            List<Action<string, string>> handlers;
            if (_handlers.TryGetValue(topic, out handlers))
                _handlers[topic].Remove(handler);

            Console.WriteLine("MQTTHelper - ending subscription to " + topic);
            _client.UnsubscribeAsync(topic).Wait();
        }

        public static void PublishUserLogin(User user, string where)
        {
            try
            {
                AddRefreshUserMessages(user);


                _client.EnqueueAsync(new MqttApplicationMessageBuilder()
                    .WithTopic($"mesh/users/{user.Id}/activity/login/{where}")
                    .WithPayload(DateTimeOffset.Now.ToString("u"))
                    .WithRetainFlag().Build()).Wait();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private static void AddRefreshUserMessages(User user)
        {
            _client.EnqueueAsync(new MqttApplicationMessageBuilder()
                .WithTopic($"mesh/users/{user.Id}/name")
                .WithPayload(user.CommonName)
                .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                .WithRetainFlag().Build()).Wait();
            _client.EnqueueAsync(new MqttApplicationMessageBuilder()
                .WithTopic($"mesh/users/{user.Id}/role")
                .WithPayload(user.IsMain ? "main" : "user")
                .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                .WithRetainFlag().Build()).Wait();
        }

        public static void PublishUserPresence(User user, Location loc)
        {
            if (user.IsGuest)
                return;

            try
            {

                AddRefreshUserMessages(user);

                if (user.Presence != null)
                {

                    if (loc == null)
                    {
                        _client.EnqueueAsync(new MqttApplicationMessageBuilder()
                            .WithTopic($"mesh/users/{user.Id}/presence/currentLocation/id")
                            .WithPayload("")
                            .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                            .WithRetainFlag().Build()).Wait();
                        _client.EnqueueAsync(new MqttApplicationMessageBuilder()
                            .WithTopic($"mesh/users/{user.Id}/presence/currentLocation/name")
                            .WithPayload("")
                            .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                            .WithRetainFlag().Build()).Wait();
                    }
                    else
                    {
                        _client.EnqueueAsync(new MqttApplicationMessageBuilder()
                            .WithTopic($"mesh/users/{user.Id}/presence/currentLocation/id")
                            .WithPayload(loc.Id)
                            .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                            .WithRetainFlag().Build()).Wait();
                        _client.EnqueueAsync(new MqttApplicationMessageBuilder()
                            .WithTopic($"mesh/users/{user.Id}/presence/currentLocation/name")
                            .WithPayload(loc.Name)
                            .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                            .WithRetainFlag().Build()).Wait();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public static void PublishTriggerActivated(string triggerId, DateTimeOffset triggerExecutionDate, string description)
        {
            try
            {

                _client.EnqueueAsync(new MqttApplicationMessageBuilder()
                    .WithTopic($"mesh/triggers/{triggerId}/lastRun")
                    .WithPayload(triggerExecutionDate.ToString("O"))
                    .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                    .WithRetainFlag().Build()).Wait();

                _client.EnqueueAsync(new MqttApplicationMessageBuilder()
                    .WithTopic($"mesh/triggers/{triggerId}/desc")
                    .WithPayload(description)
                    .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                    .WithRetainFlag().Build()).Wait();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public static void PublishJson(string topicName, string json)
        {
            try
            {

                _client.EnqueueAsync(new MqttApplicationMessageBuilder()
                    .WithTopic(topicName)
                    .WithContentType("text/json")
                    .WithPayload(json)
                    .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                    .Build()).Wait();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private class DeviceStatus
        {
            public string Ipv4 { get; set; }
            public string IpV6 { get; set; }
            public bool Reachable { get; set; }
        }

        private static Dictionary<string, DeviceStatus> _devices = new Dictionary<string, DeviceStatus>();

        public static void PublishDeviceStatus(string deviceName, bool isReachable, string mainIpv4, string mainIpv6)
        {
            DeviceStatus stat = null;
            if (!_devices.TryGetValue(deviceName, out stat))
                stat = null;

            if (stat != null && stat.Reachable == isReachable && stat.Ipv4 == mainIpv4 && stat.IpV6 == mainIpv6)
                return;

            if (stat != null)
                _devices.Remove(deviceName);

            stat = new DeviceStatus()
            {
                Ipv4 = mainIpv4,
                IpV6 = mainIpv6,
                Reachable = isReachable
            };

            _devices.Add(deviceName, stat);

            MqttApplicationMessageBuilder msg1 = new MqttApplicationMessageBuilder()
                .WithTopic("mesh/network/appliances/" + deviceName + "/reachable")
                .WithPayload(isReachable.ToString())
                .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                .WithRetainFlag();
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

        public static void PublishAgentStatus(string agent, string status, DateTimeOffset lastPing)
        {
            _client.EnqueueAsync(new MqttApplicationMessageBuilder()
                .WithTopic($"mesh/agents/{agent}/status")
                .WithPayload(status)
                .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                .WithRetainFlag().Build()).Wait();

            _client.EnqueueAsync(new MqttApplicationMessageBuilder()
                .WithTopic($"mesh/agents/{agent}/lastPing")
                .WithPayload(lastPing.ToString("u"))
                .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                .WithRetainFlag().Build()).Wait();

        }

        public static void PublishSceneGroupStatus(string sceneGroupId, string sceneGroupName, DateTimeOffset lastChange, string sceneId, string sceneName, bool isActive)
        {
            _client.EnqueueAsync(new MqttApplicationMessageBuilder()
                .WithTopic($"mesh/scenes/{sceneGroupId}/name")
                .WithPayload(sceneGroupName)
                .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                .WithRetainFlag().Build()).Wait();

            _client.EnqueueAsync(new MqttApplicationMessageBuilder()
                .WithTopic($"mesh/scenes/{sceneGroupId}/lastChange")
                .WithPayload(lastChange.ToString("u"))
                .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce).Build()).Wait();

            _client.EnqueueAsync(new MqttApplicationMessageBuilder()
                .WithTopic($"mesh/scenes/{sceneGroupId}/{sceneId}/name")
                .WithPayload(sceneName)
                .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                .WithRetainFlag().Build()).Wait();

            _client.EnqueueAsync(new MqttApplicationMessageBuilder()
                .WithTopic($"mesh/scenes/{sceneGroupId}/{sceneId}/active")
                .WithPayload(isActive.ToString())
                .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                .WithRetainFlag().Build()).Wait();
        }


        public static void PublishRoom(string zoneId, LocationRoom room)
        {
            _client.EnqueueAsync(new MqttApplicationMessageBuilder()
                .WithTopic($"mesh/zones/{EscapeName(zoneId)}/rooms/{EscapeName(room.Id)}/name")
                .WithPayload(room.Name)
                .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                .WithRetainFlag().Build()).Wait();

            if (room.Properties != null)
            {
                if (room.Properties.Temperature != null)
                    _client.EnqueueAsync(new MqttApplicationMessageBuilder()
                        .WithTopic($"mesh/zones/{EscapeName(zoneId)}/rooms/{EscapeName(room.Id)}/temperature")
                        .WithPayload(room.Properties.Temperature.GetValueOrDefault().ToString("0.00"))
                        .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                        .WithRetainFlag().Build()).Wait();
                if (room.Properties.Humidity != null)
                    _client.EnqueueAsync(new MqttApplicationMessageBuilder()
                    .WithTopic($"mesh/zones/{EscapeName(zoneId)}/rooms/{EscapeName(room.Id)}/humidity")
                    .WithPayload(room.Properties.Humidity.GetValueOrDefault().ToString("0.00"))
                    .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                    .WithRetainFlag().Build()).Wait();
                if (room.Properties.Occupancy != null)
                    _client.EnqueueAsync(new MqttApplicationMessageBuilder()
                    .WithTopic($"mesh/zones/{EscapeName(zoneId)}/rooms/{EscapeName(room.Id)}/occupancy")
                    .WithPayload(room.Properties.Occupancy.Value.ToString())
                    .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                    .WithRetainFlag().Build()).Wait();

                if (room.Properties.MoreProperties != null)
                {
                    foreach (var t in room.Properties.MoreProperties.Keys)
                    {
                        _client.EnqueueAsync(new MqttApplicationMessageBuilder()
                            .WithTopic($"mesh/zones/{EscapeName(zoneId)}/rooms/{EscapeName(room.Id)}/{EscapeName(t)}")
                            .WithPayload(room.Properties.MoreProperties[t])
                            .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                            .WithRetainFlag().Build()).Wait();
                    }
                }

            }
        }

        public static void PublishEntity(Entity entity)
        {
            _client.EnqueueAsync(new MqttApplicationMessageBuilder()
                .WithTopic($"mesh/entities/{EscapeName(entity.Id)}/name")
                .WithPayload(entity.Name)
                .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                .WithRetainFlag().Build()).Wait();
            _client.EnqueueAsync(new MqttApplicationMessageBuilder()
                .WithTopic($"mesh/entities/{EscapeName(entity.Id)}/kind")
                .WithPayload(entity.EntityKind)
                .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                .WithRetainFlag().Build()).Wait();
            _client.EnqueueAsync(new MqttApplicationMessageBuilder()
                .WithTopic($"mesh/entities/{EscapeName(entity.Id)}/currentImage")
                .WithPayload(entity.CurrentImageUrl == null ? entity.DefaultImageUrl : entity.CurrentImageUrl)
                .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                .WithRetainFlag().Build()).Wait();


            foreach (var t in entity.Datas.Keys)
            {
                AddEntityData(t, entity.Datas[t], EscapeName(entity.Id));
            }
        }

        private static void AddEntityData(string key, EntityData t, string path)
        {
            if (!t.IsComplex())
            {
                switch (t.SimpleType.ToLowerInvariant())
                {
                    case "system.decimal":
                        _client.EnqueueAsync(new MqttApplicationMessageBuilder()
                        .WithTopic($"mesh/entities/{path}/{EscapeName(key)}")
                        .WithPayload(t.DecimalSimpleValue.GetValueOrDefault().ToString("0.00", CultureInfo.InvariantCulture))
                        .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                        .WithRetainFlag().Build()).Wait();
                        break;
                    case "system.int64":
                        _client.EnqueueAsync(new MqttApplicationMessageBuilder()
                        .WithTopic($"mesh/entities/{path}/{EscapeName(key)}")
                        .WithPayload(t.IntSimpleValue.GetValueOrDefault().ToString("0", CultureInfo.InvariantCulture))
                        .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                        .WithRetainFlag().Build()).Wait();
                        break;
                    case "system.datetimeoffset":
                        _client.EnqueueAsync(new MqttApplicationMessageBuilder()
                        .WithTopic($"mesh/entities/{path}/{EscapeName(key)}")
                        .WithPayload(t.DateSimpleValue.GetValueOrDefault().ToUniversalTime().ToString("yyyyMMdd-HHmmssZ", CultureInfo.InvariantCulture))
                        .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                        .WithRetainFlag().Build()).Wait();
                        break;
                    case "system.string":
                        _client.EnqueueAsync(new MqttApplicationMessageBuilder()
                        .WithTopic($"mesh/entities/{path}/{EscapeName(key)}")
                        .WithPayload(t.SimpleValue)
                        .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                        .WithRetainFlag().Build()).Wait();
                        break;

                }
            }
            else
            {
                path = path + "/" + EscapeName(key);
                foreach (var z in t.ComplexValue.Keys)
                {
                    AddEntityData(z, t.ComplexValue[z], path);
                }
            }
        }

        private static string EscapeName(string name)
        {
            if (name == null)
                return "null";
            return name.Replace("/", "-");
        }

        public static void PublishMeshProperty(string property, string value)
        {
            MqttApplicationMessageBuilder msg1 = new MqttApplicationMessageBuilder()
                .WithTopic($"mesh/properties/{property}")
                .WithPayload(value)
                .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                .WithRetainFlag();
            _client.EnqueueAsync(msg1.Build()).Wait();
        }

        public static void Stop()
        {
            _client.StopAsync().Wait();
            _client.Dispose();
            _client = null;
        }


    }
}
