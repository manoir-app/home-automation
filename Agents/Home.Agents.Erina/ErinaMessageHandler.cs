using Home.Common;
using Home.Common.Messages;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Home.Agents.Erina
{
    public static class ErinaMessageHandler
    {

        public static void Start()
        {

            Thread t = new Thread(() => NatsMessageThread.Run(new string[] { "erina.>" },
            ErinaMessageHandler.HandleMessage));
            t.Name = "NatsThread";
            t.Start();
        }


        public static void Stop()
        {
            NatsMessageThread.Stop();
        }


        public static MessageResponse HandleMessage(MessageOrigin origin, string topic, string messageBody)
        {
            if (messageBody == null)
                return MessageResponse.GenericFail;

            messageBody = messageBody.Trim();
            Console.WriteLine("------------------------");
            Console.WriteLine("Message NATS Recu:");
            Console.WriteLine(messageBody);
            Console.WriteLine("------------------------");

            return MessageResponse.OK;
        }

    }
}
