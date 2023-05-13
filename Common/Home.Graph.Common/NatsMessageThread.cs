using Home.Common.Messages;
using Home.Graph.Common;
using NATS.Client;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading;

namespace Home.Common
{

    [Serializable]
    public class NatsNoResponseException : Exception
    {
        public NatsNoResponseException() { }
        public NatsNoResponseException(string message) : base(message) { }
        public NatsNoResponseException(string message, Exception inner) : base(message, inner) { }
        protected NatsNoResponseException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    public static class NatsMessageThread
    {
        private static bool _shouldStop = false;

        public static void Stop()
        {
            _shouldStop = true;
        }

        public static void Push(BaseMessage message)
        {
            string topic = message.Topic;
            Push(topic, message);
        }

        public static void Push(string topic, BaseMessage message)
        {
            string messageContent = JsonConvert.SerializeObject(message);
            Push(topic, messageContent);
        }

        public static void Push(string topic, string messageContent)
        {
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    using (var c = GetConnection())
                    {
                        c.Publish(topic, Encoding.UTF8.GetBytes(messageContent));
                        break;
                    }
                }
                catch (Exception)
                {
                    if (i == 2)
                        throw;
                }
            }
        }

        public static string Request(string topic, BaseMessage message)
        {
            string messageContent = JsonConvert.SerializeObject(message);
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    using (var c = GetConnection())
                    {
                        var msg = c.Request(topic, Encoding.UTF8.GetBytes(messageContent), 1500);
                        var messagebody = Encoding.UTF8.GetString(msg.Data, 0, msg.Data.Length);
                        //Console.WriteLine($"Send request for {topic} OK : {messagebody} ");
                        return messagebody;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Send request for {topic} failed ({ex.GetType().Name}): {ex.Message} ");
                    Thread.Sleep(500);
                    if (i == 2)
                        throw;
                }
            }

            return JsonConvert.SerializeObject(MessageResponse.GenericFail);
        }

        public static T Request<T>(string topic, BaseMessage message)
        {
            return Request<T>(topic, message, 15000);
        }
        public static T Request<T>(string topic, BaseMessage message, int dureeMaxEnMs)
        {
            string messageContent = JsonConvert.SerializeObject(message);
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    using (var c = GetConnection())
                    {
                        var msg = c.Request(topic, Encoding.UTF8.GetBytes(messageContent), dureeMaxEnMs);
                        var messagebody = Encoding.UTF8.GetString(msg.Data, 0, msg.Data.Length);
                        //Console.WriteLine($"Send request for {topic} OK : {messagebody} ");
                        return JsonConvert.DeserializeObject<T>(messagebody);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Send request for {topic} failed ({ex.GetType().Name}) : {ex.Message}");
                    Thread.Sleep(500);
                    if (i == 2)
                        throw;
                }
            }

            throw new InvalidOperationException();
        }

        public static void Run(string[] topics, MessageHandler hndl)
        {
            var s = GetServers();
            foreach (var srv in s)
                Console.WriteLine(srv);

            while (!_shouldStop)
            {
                try
                {

                    using (var c = GetConnection())
                    {
                        foreach (var topic in topics)
                        {
                            c.SubscribeAsync(topic, (sender, args) =>
                            {
                                var messagebody = Encoding.UTF8.GetString(args.Message.Data, 0, args.Message.Data.Length);
                                try
                                {
                                    Console.WriteLine("------------------------");
                                    Console.Write("Message Recu:");
                                    Console.WriteLine(args.Message.Subject);
                                    Console.WriteLine("------------------------");

                                    var hdl = hndl.Invoke(MessageOrigin.Local, args.Message.Subject, messagebody);
                                    if (hdl != null)
                                    {
                                        hdl.Topic = args.Message.Subject;
                                        
                                        try
                                        {
                                            Console.WriteLine("------------------------");
                                            Console.Write("Message Recu:");
                                            Console.Write(args.Message.Subject);
                                            Console.Write(" => ");
                                            Console.WriteLine(hdl.Response);
                                            Console.WriteLine("------------------------");
                                            args.Message.Respond(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(hdl)));
                                        }
                                        catch (Exception ex)
                                        {
                                            Console.WriteLine(ex.ToString());
                                        }
                                    }

                                }
                                catch(NatsNoResponseException)
                                {
                                    Console.WriteLine("------------------------");
                                    Console.Write("Message Recu:");
                                    Console.Write(args.Message.Subject);
                                    Console.Write(" => NO RESPONSE");
                                    Console.WriteLine("------------------------");
                                }
                                catch (NotImplementedException)
                                {
                                    Console.WriteLine("------------------------");
                                    Console.Write("Message Recu:");
                                    Console.Write(args.Message.Subject);
                                    Console.Write(" => NOT IMPLEMENTED");
                                    Console.WriteLine("------------------------");
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
                catch(NotImplementedException)
                {

                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex.ToString());
                }
            }

        }

        public static IConnection GetConnection()
        {
            var cf = new ConnectionFactory();
            string[] servers = GetServers();

            Options opts = ConnectionFactory.GetDefaultOptions();
            opts.MaxReconnect = 2;
            opts.ReconnectWait = 1000;
            opts.Servers = servers;
            IConnection c = cf.CreateConnection(opts);
            return c;
        }

        public static string[] GetServers()
        {
            var srv = "localhost";
            var port = 4222;

            var tmp = Environment.GetEnvironmentVariable("NATS_SERVICE_HOST");
            if (!string.IsNullOrEmpty(tmp))
                srv = tmp;

            //tmp = LocalDebugHelper.GetLocalServiceHost();
            //if (!string.IsNullOrEmpty(tmp))
            //    srv = tmp;


            tmp = Environment.GetEnvironmentVariable("NATS_SERVICE_PORT");
            if (string.IsNullOrEmpty(tmp))
                tmp = Environment.GetEnvironmentVariable("NATS_PORT_4222_TCP_PROTO");
            if (!string.IsNullOrEmpty(tmp))
            {
                if (!int.TryParse(tmp, out port))
                    port = 4222;
            }
            string[] servers = new string[] { $"nats://{srv}:{port}" };
            return servers;
        }


    }
}
