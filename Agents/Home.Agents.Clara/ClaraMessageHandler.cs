using Home.Common;
using Home.Common.Messages;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Home.Agents.Clara
{
    public static class ClaraMessageHandler
    {

        public static void Start()
        {

            Thread t = new Thread(() => NatsMessageThread.Run(new string[] { "clara.>",
                DownloadItemProgressMessage.CancelledTopicName,
                DownloadItemProgressMessage.StartedTopicName,
                DownloadItemProgressMessage.PausedTopicName,
                NewExternalTokenMessage.TopicName
            },
            ClaraMessageHandler.HandleMessage));
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

            DownloadItemProgressMessage itemProgress;

            switch (topic.ToLowerInvariant())
            {
                case "clara.stop":
                    Program._stop = true;
                    return MessageResponse.OK;
                case DownloadItemProgressMessage.CancelledTopicName:
                    itemProgress = JsonConvert.DeserializeObject<DownloadItemProgressMessage>(messageBody);
                    if (DownloadSourceChecker.HandleCancel(itemProgress))
                        return MessageResponse.OK;
                    return MessageResponse.GenericFail;
                case DownloadItemProgressMessage.StartedTopicName:
                    itemProgress = JsonConvert.DeserializeObject<DownloadItemProgressMessage>(messageBody);
                    if (DownloadSourceChecker.HandleStartDownload(itemProgress))
                        return MessageResponse.OK;
                    return MessageResponse.GenericFail;
                case NewExternalTokenMessage.TopicName:
                    return MessageResponse.OK;
            }

            return MessageResponse.OK;
        }

    }
}
