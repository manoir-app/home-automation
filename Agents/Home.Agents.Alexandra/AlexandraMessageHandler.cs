using Home.Common;
using Home.Common.Messages;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Home.Agents.Alexandra
{
    public static class AlexandraMessageHandler
    {
        public static void Start()
        {

            Thread t = new Thread(() => NatsMessageThread.Run(new string[] { "alexandra.>"},
            AlexandraMessageHandler.HandleMessage));
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
            Console.WriteLine("Message :");
            Console.WriteLine(messageBody);
            Console.WriteLine("------------------------");
            
            if(topic.Equals("alexandra.stop", StringComparison.InvariantCultureIgnoreCase))
            {
                Program._stop = true;
                return MessageResponse.OK;
            }
            else
            {
                throw new NotImplementedException();
            }
        }


    }
}
