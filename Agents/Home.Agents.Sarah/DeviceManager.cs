using Home.Common;
using Home.Common.HomeAutomation;
using Home.Common.Messages;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Home.Agents.Sarah
{
    public static partial class DeviceManager
    {
        public static List<DeviceBase> AllDevices = new List<DeviceBase>();
        public static List<string> AllTopics = new List<string>();

        public static void Start()
        {
            var topics = new List<string>();
            // les actions qui sont reconnues comme passant par SARAH
            topics.Add("homeautomation.devices.sarah.>");

            // les discovery de devices que l'on gère
            topics.Add("homeautomation.discovery.fake.>");
            topics.Add("homeautomation.discovery.fake");
            topics.Add("homeautomation.discovery.plex.>");
            topics.Add("homeautomation.discovery.plex");
            topics.Add("homeautomation.discovery.storage.>");
            topics.Add("homeautomation.discovery.storage");
            topics.Add("homeautomation.discovery.zipato.>");
            topics.Add("homeautomation.discovery.zipato");
            topics.Add("homeautomation.discovery.shelly.>");
            topics.Add("homeautomation.discovery.shelly");
            topics.Add("homeautomation.discovery.hue.>");
            topics.Add("homeautomation.discovery.hue");
            topics.Add("homeautomation.discovery.divoom.>");
            topics.Add("homeautomation.discovery.divoom");

            // les commandes de devices que l'on gère
            topics.Add("homeautomation.fake.>");
            topics.Add("homeautomation.fake");
            topics.Add("homeautomation.plex.>");
            topics.Add("homeautomation.plex");
            topics.Add("homeautomation.storage.>");
            topics.Add("homeautomation.storage");
            topics.Add("homeautomation.zipato.>");
            topics.Add("homeautomation.zipato");
            topics.Add("homeautomation.shelly.>");
            topics.Add("homeautomation.shelly");
            topics.Add("homeautomation.hue.>");
            topics.Add("homeautomation.hue");
            topics.Add("homeautomation.divoom.>");
            topics.Add("homeautomation.divoom");
            topics.Add("display.divoom.>");
            topics.Add("display.divoom");

            AllTopics = topics;

            StartListening(topics.ToArray());
        }

        private static void StartListening(string[] topics)
        {
            _shouldStop = false;
            Thread t = new Thread(() => Run(topics,
            DeviceManager.HandleMessage));
            t.Name = "NatsDeviceThread";
            t.Start();
        }

        private static bool _shouldStop;

        public static void Run(string[] topics, MessageHandler hndl)
        {
            var s = NatsMessageThread.GetServers();
            foreach (var srv in s)
                Console.WriteLine(srv + " for device messages");
            Console.WriteLine("Registering device message handler for : ");
            if (topics == null || topics.Length == 0)
                Console.WriteLine("# NO TOPICS#");
            else
            {
                foreach (var topic in topics)
                {
                    Console.WriteLine($"   {topic}");
                }
            }

            while (!_shouldStop)
            {
                try
                {

                    using (var c = NatsMessageThread.GetConnection())
                    {
                        foreach (var topic in topics)
                        {
                            c.SubscribeAsync(topic, (sender, args) =>
                            {
                                var messagebody = Encoding.UTF8.GetString(args.Message.Data, 0, args.Message.Data.Length);
                                try
                                {
                                    var hdl = hndl.Invoke(MessageOrigin.Local, args.Message.Subject, messagebody);
                                    if (hdl != null)
                                    {
                                        hdl.Topic = args.Message.Subject;
                                        try
                                        {
                                            args.Message.Respond(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(hdl)));
                                        }
                                        catch (Exception ex)
                                        {
                                            Console.WriteLine(ex.ToString());
                                        }
                                    }

                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine(ex.ToString());
                                }

                            });
                        }
                        while (!_shouldStop)
                        {
                            Thread.Sleep(500);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex.ToString());
                }
            }

        }

        public static void Stop()
        {
            _shouldStop = true;
        }

        public static MessageResponse HandleMessage(MessageOrigin origin, string topic, string messageBody)
        {
            Console.WriteLine("---- DEVICE MESSAGE ------");
            Console.WriteLine(topic);
            Console.WriteLine();
            Console.WriteLine(messageBody);
            Console.WriteLine("--------------------------");

            if(topic==null)
                return MessageResponse.GenericFail;

            switch (topic.ToLowerInvariant())
            {
                case "clara.hue.login":
                case "clara.homeautomation.hue.login":
                case "homeautomation.hue.login":
                    Devices.Hue.HueHelper.Login();
                    return MessageResponse.OK;
            }

            if (topic.StartsWith("homeautomation.discovery.shelly", StringComparison.InvariantCultureIgnoreCase))
                return Devices.Shelly.ShellyDeviceHelper.HandleMessage(origin, topic, messageBody);
            if (topic.StartsWith("homeautomation.shelly", StringComparison.InvariantCultureIgnoreCase))
                return Devices.Shelly.ShellyDeviceHelper.HandleMessage(origin, topic, messageBody);
            if (topic.StartsWith("homeautomation.discovery.hue", StringComparison.InvariantCultureIgnoreCase))
                return Devices.Hue.HueHelper.HandleMessage(origin, topic, messageBody);
            if (topic.StartsWith("homeautomation.hue", StringComparison.InvariantCultureIgnoreCase))
                return Devices.Hue.HueHelper.HandleMessage(origin, topic, messageBody);
            if (topic.StartsWith("homeautomation.divoom", StringComparison.InvariantCultureIgnoreCase))
                return Devices.Divoom.DivoomDeviceHelper.HandleMessage(origin, topic, messageBody);


            return MessageResponse.GenericFail;

        }

        internal static void ParseFromTopic(string topic, out string deviceId, out string role, out string value)
        {
            string[] parts = topic.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            deviceId = "";
            if (parts.Length > 3)
            {
                switch (parts[3])
                {
                    case "*":
                    case "alldevices":
                        deviceId = "*";
                        break;
                    default:
                        deviceId = parts[3];
                        break;
                }
            }
            role = "";
            if (parts.Length >= 3)
            {
                switch (parts[2])
                {
                    case "switch":
                    case "toggleswitch":
                        role = "switch";
                        break;
                }
            }
            value = "";
            if (parts.Length >= 4)
            {
                switch (parts[3])
                {
                    case "on":
                    case "turnon":
                        value = "on";
                        break;
                }
            }
        }

    }
}
