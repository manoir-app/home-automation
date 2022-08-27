using Home.Common;
using Home.Common.Messages;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Home.Agents.Freeia
{
    public static class FreeiaMessageHandler
    {
        public static void Start()
        {

            Thread t = new Thread(() => NatsMessageThread.Run(new string[] { "freeia.>",
                "system.network.devices.enumerate",
                "homeautomation.devices.freeia.>",
                "system.triggers.change",
                DownloadItemMessage.TopicName},
            FreeiaMessageHandler.HandleMessage));
            t.Name = "NatsThread";
            t.Start();
        }


        public static void Stop()
        {
            ServiceBusMessageThread.Stop();
            NatsMessageThread.Stop();
        }


        public static MessageResponse HandleMessage(MessageOrigin origin, string topic, string messageBody)
        {
            if (messageBody == null)
                return MessageResponse.GenericFail;

            messageBody = messageBody.Trim();
            Console.WriteLine("------------------------");
            Console.WriteLine("Message Recu:");
            Console.WriteLine(messageBody);
            Console.WriteLine("------------------------");

            switch (topic.ToLowerInvariant())
            {
                case "freeia.stop":
                    Program._stop = true;
                    return MessageResponse.OK;
                case "freeia.login":
                    FreeboxHelper.StartAuth();
                    return MessageResponse.OK;
                case "freeia.reboot":
                    FreeboxHelper.Reboot();
                    return MessageResponse.OK;
                case "system.network.devices.enumerate":
                    return FreeboxHelper.EnumerateDevices();
                case "system.triggers.change":
                    FreeboxHelper.Reload();
                    return MessageResponse.OK;
                case DownloadItemMessage.TopicName:
                    var dataDownload = JsonConvert.DeserializeObject<DownloadItemMessage>(messageBody);
                    var retDownload = new DownloadItemMessageResponse()
                    {
                        Topic = DownloadItemMessage.TopicName,
                        Response = "ok",
                        SourceUrl = dataDownload.SourceUrl,
                        WasQueued = false
                    };
                    if (dataDownload != null)
                    {
                        var dl = FreeboxHelper.StartDownload(dataDownload.SourceUrl);
                        if (dl != null)
                        {
                            retDownload.WasQueued = true;
                            retDownload.Identifier = "freebox:downloadmgr:" + dl.id;
                        }
                    }
                    return retDownload;
            }

            return MessageResponse.OK;
        }
    }
}
