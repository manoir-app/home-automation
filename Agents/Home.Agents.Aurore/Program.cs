﻿using Home.Agents.Aurore.Greetings;
using Home.Agents.Aurore.Integrations.Rhasspy;
using Home.Common;
using Home.Graph.Common;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Home.Agents.Aurore
{
    class Program
    {
        internal static bool _stop = false;

        static void Main(string[] args)
        {
            AgentHelper.WriteStartupMessage("Aurore", typeof(Program).Assembly);

          
#if DEBUG

            RhasspyService.FullRefresh();

            return;
#endif

            AgentHelper.SetupReporting("aurore");
            AgentHelper.SetupLocaleFromServer("aurore");

            AgentHelper.ReportStart("aurore", "user-interaction");

            MqttHelper.Start("agents-aurore");
            AuroreMessageHandler.Start();
            AuroreNewsItemService.Start();
            CommonAppGreetingsUpdater.Start();
            while (!_stop)
            {
                Thread.Sleep(500);
                AgentHelper.Ping("aurore");

            }
            CommonAppGreetingsUpdater.Stop();
            AuroreNewsItemService.Stop();
            AuroreMessageHandler.Stop();
            MqttHelper.Stop();

        }

        private static void CopyCommonFiles(string root, string folder)
        {
            if(!root.EndsWith("/"))
                root = root + "/";

            foreach(var sub in Directory.GetDirectories(folder))
                CopyCommonFiles(root, sub);

            foreach(var t in Directory.GetFiles(folder))
            {
                string dest = "/home-automation/files/cache/" + t.Substring(root.Length);
                var dir = Path.GetDirectoryName(dest);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                File.Copy(t, dest, true);
            }
        }



        // nuget GoogleCast
        // using GoogleCast;
        // using GoogleCast.Channels;
        // using GoogleCast.Models.Media;


        //static async Task Test()
        //{
        //    var receiver = (await new DeviceLocator().FindReceiversAsync()).First();

        //    var sender = new Sender();
        //    // Connect to the Chromecast
        //    await sender.ConnectAsync(receiver);
        //    // Launch the default media receiver application


        //    var receivever = sender.GetChannel<IReceiverChannel>();
        //    receivever.StatusChanged += Receivever_StatusChanged;

        //    var mediaChannel = sender.GetChannel<IMediaChannel>();
        //    await sender.LaunchAsync(mediaChannel);
        //    // Load and play Big Buck Bunny video

        //    var media1 = new MediaInformation()
        //    {
        //        ContentId = "https://commondatastorage.googleapis.com/gtv-videos-bucket/CastVideos/mp4/DesigningForGoogleCast.mp4",
        //        Duration = 10,
        //        StreamType = StreamType.None
        //    };
        //    var media2 = new MediaInformation()
        //    {
        //        ContentId = "https://commondatastorage.googleapis.com/gtv-videos-bucket/CastVideos/mp4/DesigningForGoogleCast.mp4",
        //        Duration = 10,
        //        StreamType = StreamType.None
        //    };

        //    var s = await mediaChannel.QueueLoadAsync(RepeatMode.RepeatOff, new MediaInformation[] { media1, media2 });
        //    s = await mediaChannel.SeekAsync(12);
        //}

        //private static void Receivever_StatusChanged(object sender, EventArgs e)
        //{
        //    var status = ((IReceiverChannel)sender).Status;
        //    Console.Write(status?.Applications?.FirstOrDefault()?.StatusText);
        //}
    }
}
