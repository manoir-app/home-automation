using Home.Common;
using Home.Common.Model;
using MQTTnet;
using MQTTnet.Client.Options;
using MQTTnet.Client.Receiving;
using MQTTnet.Extensions.ManagedClient;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace Home.Graph.Common
{
    public static class MqttHelper
    {
        static IManagedMqttClient _client = null;

        public static void Start()
        {
            if (_client != null)
                return;

            string server = LocalDebugHelper.GetLocalServiceHost();
            if (server == null)
                server = HomeServerHelper.GetLocalIP();
            if (server == null)
                return;

            var options = new ManagedMqttClientOptionsBuilder()
                .WithAutoReconnectDelay(TimeSpan.FromSeconds(5))
                .WithClientOptions(new MqttClientOptionsBuilder()
                    .WithClientId("HomeGraph")
                    .WithTcpServer((opts) =>
                    {
                        opts.AddressFamily = System.Net.Sockets.AddressFamily.InterNetwork;
                        opts.Server = server;
                        opts.Port = 1883;
                    })
                    .WithCleanSession(true)
                    .Build())
                .Build();

            _client = new MqttFactory().CreateManagedMqttClient();
            _client.StartAsync(options).Wait();
            _client.ApplicationMessageReceivedHandler = MessageHandler._instance;
        }

        private class MessageHandler : IMqttApplicationMessageReceivedHandler
        {
            private MessageHandler()
            {

            }

            public static MessageHandler _instance = new MessageHandler();

            public Dictionary<string, List<Action<string>>> _handlers = new Dictionary<string, List<Action<string>>>();

            public async Task HandleApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs eventArgs)
            {
                var msg = eventArgs.ApplicationMessage;
                var t = msg.Topic;
                if (t == null) return;

                List<Action<string>> handlers;
                if (_handlers.TryGetValue(t, out handlers))
                {
                    foreach (var handler in handlers)
                    {
                        t = msg.ConvertPayloadToString();
                        if (t != null)
                        {
                            Console.WriteLine("MQTTHelper - raising event on subscription to " + msg.Topic + " : " + t);
                            handler.Invoke(t);
                        }
                    }
                }

                await Task.CompletedTask;
            }
        }


        public static void AddChangeHandler(string topic, Action<string> handler)
        {
            List<Action<string>> handlers;
            if(!MessageHandler._instance._handlers.TryGetValue(topic, out handlers))
                MessageHandler._instance._handlers[topic] = new List<Action<string>>();
            MessageHandler._instance._handlers[topic].Add(handler);
            Console.WriteLine("MQTTHelper - subscription to " + topic);
            _client.SubscribeAsync(topic);
        }

        public static void RemoveChangeHandler(string topic, Action<string> handler)
        {
            List<Action<string>> handlers;
            if (MessageHandler._instance._handlers.TryGetValue(topic, out handlers))
                MessageHandler._instance._handlers[topic].Remove(handler);

            Console.WriteLine("MQTTHelper - ending subscription to " + topic);
            _client.UnsubscribeAsync(topic);
        }

        public static void PublishUserLogin(User user, string where)
        {
            try
            {
                List<MqttApplicationMessage> msgs = new List<MqttApplicationMessage>();

                AddRefreshUserMessages(user, msgs);

                msgs.Add(new MqttApplicationMessageBuilder()
                    .WithTopic($"mesh/users/{user.Id}/activity/login/{where}")
                    .WithPayload(DateTimeOffset.Now.ToString("u"))
                    .WithAtLeastOnceQoS()
                    .WithRetainFlag().Build());

                _client.PublishAsync(msgs).Wait();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private static void AddRefreshUserMessages(User user, List<MqttApplicationMessage> msgs)
        {
            msgs.Add(new MqttApplicationMessageBuilder()
                .WithTopic($"mesh/users/{user.Id}/name")
                .WithPayload(user.CommonName)
                .WithAtLeastOnceQoS()
                .WithRetainFlag().Build());
            msgs.Add(new MqttApplicationMessageBuilder()
                .WithTopic($"mesh/users/{user.Id}/role")
                .WithPayload(user.IsMain ? "main" : "user")
                .WithAtLeastOnceQoS()
                .WithRetainFlag().Build());
        }

        public static void PublishUserPresence(User user, Location loc)
        {
            if (user.IsGuest)
                return;

            try
            {
                List<MqttApplicationMessage> msgs = new List<MqttApplicationMessage>();

                AddRefreshUserMessages(user, msgs);

                if (user.Presence != null)
                {

                    if (loc == null)
                    {
                        msgs.Add(new MqttApplicationMessageBuilder()
                            .WithTopic($"mesh/users/{user.Id}/presence/currentLocation/id")
                            .WithPayload("")
                            .WithAtLeastOnceQoS()
                            .WithRetainFlag().Build());
                        msgs.Add(new MqttApplicationMessageBuilder()
                            .WithTopic($"mesh/users/{user.Id}/presence/currentLocation/name")
                            .WithPayload("")
                            .WithAtLeastOnceQoS()
                            .WithRetainFlag().Build());
                    }
                    else
                    {
                        msgs.Add(new MqttApplicationMessageBuilder()
                            .WithTopic($"mesh/users/{user.Id}/presence/currentLocation/id")
                            .WithPayload(loc.Id)
                            .WithAtLeastOnceQoS()
                            .WithRetainFlag().Build());
                        msgs.Add(new MqttApplicationMessageBuilder()
                            .WithTopic($"mesh/users/{user.Id}/presence/currentLocation/name")
                            .WithPayload(loc.Name)
                            .WithAtLeastOnceQoS()
                            .WithRetainFlag().Build());
                    }
                }

                _client.PublishAsync(msgs).Wait();

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
                List<MqttApplicationMessage> msgs = new List<MqttApplicationMessage>();

                msgs.Add(new MqttApplicationMessageBuilder()
                    .WithTopic($"mesh/triggers/{triggerId}/lastRun")
                    .WithPayload(triggerExecutionDate.ToString("O"))
                    .WithAtLeastOnceQoS()
                    .WithRetainFlag().Build());

                msgs.Add(new MqttApplicationMessageBuilder()
                    .WithTopic($"mesh/triggers/{triggerId}/desc")
                    .WithPayload(description)
                    .WithAtLeastOnceQoS()
                    .WithRetainFlag().Build());


                _client.PublishAsync(msgs).Wait();
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

        public static void PublishAgentStatus(string agent, string status, DateTimeOffset lastPing)
        {
            List<MqttApplicationMessage> msgs = new List<MqttApplicationMessage>();
            msgs.Add(new MqttApplicationMessageBuilder()
                .WithTopic($"mesh/agents/{agent}/status")
                .WithPayload(status)
                .WithAtLeastOnceQoS()
                .WithRetainFlag().Build());

            msgs.Add(new MqttApplicationMessageBuilder()
                .WithTopic($"mesh/agents/{agent}/lastPing")
                .WithPayload(lastPing.ToString("u"))
                .WithAtLeastOnceQoS()
                .WithRetainFlag().Build());

            _client.PublishAsync(msgs).Wait();
        }

        public static void PublishSceneGroupStatus(string sceneGroupId, string sceneGroupName, DateTimeOffset lastChange, string sceneId, string sceneName, bool isActive)
        {
            List<MqttApplicationMessage> msgs = new List<MqttApplicationMessage>();
            msgs.Add(new MqttApplicationMessageBuilder()
                .WithTopic($"mesh/scenes/{sceneGroupId}/name")
                .WithPayload(sceneGroupName)
                .WithAtLeastOnceQoS()
                .WithRetainFlag().Build());

            msgs.Add(new MqttApplicationMessageBuilder()
                .WithTopic($"mesh/scenes/{sceneGroupId}/lastChange")
                .WithPayload(lastChange.ToString("u"))
                .WithAtLeastOnceQoS().Build());

            msgs.Add(new MqttApplicationMessageBuilder()
                .WithTopic($"mesh/scenes/{sceneGroupId}/{sceneId}/name")
                .WithPayload(sceneName)
                .WithAtLeastOnceQoS()
                .WithRetainFlag().Build());

            msgs.Add(new MqttApplicationMessageBuilder()
                .WithTopic($"mesh/scenes/{sceneGroupId}/{sceneId}/active")
                .WithPayload(isActive.ToString())
                .WithAtLeastOnceQoS()
                .WithRetainFlag().Build());

            _client.PublishAsync(msgs).Wait();
        }


        public static void PublishRoom(LocationRoom room)
        {
            List<MqttApplicationMessage> msgs = new List<MqttApplicationMessage>();
            msgs.Add(new MqttApplicationMessageBuilder()
                .WithTopic($"mesh/rooms/{EscapeName(room.Id)}/name")
                .WithPayload(room.Name)
                .WithAtLeastOnceQoS()
                .WithRetainFlag().Build());

            if (room.Properties != null)
            {
                if (room.Properties.Temperature != null)
                    msgs.Add(new MqttApplicationMessageBuilder()
                        .WithTopic($"mesh/rooms/{EscapeName(room.Id)}/temperature")
                        .WithPayload(room.Properties.Temperature.GetValueOrDefault().ToString("0.00"))
                        .WithAtLeastOnceQoS()
                        .WithRetainFlag().Build());
                if (room.Properties.Humidity != null)
                    msgs.Add(new MqttApplicationMessageBuilder()
                    .WithTopic($"mesh/rooms/{EscapeName(room.Id)}/humidity")
                    .WithPayload(room.Properties.Humidity.GetValueOrDefault().ToString("0.00"))
                    .WithAtLeastOnceQoS()
                    .WithRetainFlag().Build());

                if (room.Properties.MoreProperties != null)
                {
                    foreach (var t in room.Properties.MoreProperties.Keys)
                    {
                        msgs.Add(new MqttApplicationMessageBuilder()
                            .WithTopic($"mesh/rooms/{EscapeName(room.Id)}/{EscapeName(t)}")
                            .WithPayload(room.Properties.MoreProperties[t])
                            .WithAtLeastOnceQoS()
                            .WithRetainFlag().Build());
                    }
                }

            }



            _client.PublishAsync(msgs).Wait();
        }

        public static void PublishEntity(Entity entity)
        {
            List<MqttApplicationMessage> msgs = new List<MqttApplicationMessage>();
            msgs.Add(new MqttApplicationMessageBuilder()
                .WithTopic($"mesh/entities/{EscapeName(entity.Id)}/name")
                .WithPayload(entity.Name)
                .WithAtLeastOnceQoS()
                .WithRetainFlag().Build());
            msgs.Add(new MqttApplicationMessageBuilder()
                .WithTopic($"mesh/entities/{EscapeName(entity.Id)}/kind")
                .WithPayload(entity.EntityKind)
                .WithAtLeastOnceQoS()
                .WithRetainFlag().Build());
            msgs.Add(new MqttApplicationMessageBuilder()
                .WithTopic($"mesh/entities/{EscapeName(entity.Id)}/currentImage")
                .WithPayload(entity.CurrentImageUrl==null?entity.DefaultImageUrl:entity.CurrentImageUrl)
                .WithAtLeastOnceQoS()
                .WithRetainFlag().Build());


            foreach (var t in entity.Datas.Keys)
            {
                AddEntityData(msgs, t, entity.Datas[t], EscapeName(entity.Id));
            }

            _client.PublishAsync(msgs).Wait();
        }

        private static void AddEntityData(List<MqttApplicationMessage> msgs, string key, EntityData t, string path)
        {
            if (!t.IsComplex())
            {
                switch (t.SimpleType.ToLowerInvariant())
                {
                    case "system.decimal":
                        msgs.Add(new MqttApplicationMessageBuilder()
                        .WithTopic($"mesh/entities/{path}/{EscapeName(key)}")
                        .WithPayload(t.DecimalSimpleValue.GetValueOrDefault().ToString("0.00", CultureInfo.InvariantCulture))
                        .WithAtLeastOnceQoS()
                        .WithRetainFlag().Build());
                        break;
                    case "system.int64":
                        msgs.Add(new MqttApplicationMessageBuilder()
                        .WithTopic($"mesh/entities/{path}/{EscapeName(key)}")
                        .WithPayload(t.IntSimpleValue.GetValueOrDefault().ToString("0", CultureInfo.InvariantCulture))
                        .WithAtLeastOnceQoS()
                        .WithRetainFlag().Build());
                        break;
                    case "system.datetimeoffset":
                        msgs.Add(new MqttApplicationMessageBuilder()
                        .WithTopic($"mesh/entities/{path}/{EscapeName(key)}")
                        .WithPayload(t.DateSimpleValue.GetValueOrDefault().ToUniversalTime().ToString("yyyyMMdd-HHmmssZ", CultureInfo.InvariantCulture))
                        .WithAtLeastOnceQoS()
                        .WithRetainFlag().Build());
                        break;
                    case "system.string":
                        msgs.Add(new MqttApplicationMessageBuilder()
                        .WithTopic($"mesh/entities/{path}/{EscapeName(key)}")
                        .WithPayload(t.SimpleValue)
                        .WithAtLeastOnceQoS()
                        .WithRetainFlag().Build());
                        break;

                }
            }
            else
            {
                path = path + "/" + EscapeName(key);
                foreach (var z in t.ComplexValue.Keys)
                {
                    AddEntityData(msgs, z, t.ComplexValue[z], path);
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
                .WithAtLeastOnceQoS()
                .WithRetainFlag();
            _client.PublishAsync(msg1.Build()).Wait();
        }

        public static void Stop()
        {
            _client.StopAsync().Wait();
            _client.Dispose();
            _client = null;
        }


    }
}
