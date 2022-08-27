using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Home.Agents.Sarah.Devices.Divoom
{
    partial class DivoomDeviceHelper
    {
        public enum DisplayKind
        {
            // 0:Faces;1:Cloud Channdle;2:Visualizer;3:Custom
            Faces = 0,
            Cloud = 1,
            Vizualizer = 2,
            Custom = 3
        }

        private class CommandData
        {
            public string Command { get; set; }
        }

        private class SelectChannelCommand : CommandData
        {
            public SelectChannelCommand(DisplayKind kind)
            {
                Command = "Channel/SetIndex";
                SelectIndex = (int)kind;
            }

            public int SelectIndex { get; set; }
        }

        private class SetCustomPageIndex : CommandData
        {
            public SetCustomPageIndex(int pageId)
            {
                Command = "Channel/SetCustomPageIndex";
                CustomPageIndex = (int)pageId;
            }

            public int CustomPageIndex { get; set; }
        }


        public static void SwitchTo(string ip, DisplayKind kind, int? customId = null)
        {
            if (kind == DisplayKind.Custom && customId == null)
                customId = 0;

            using (var cli = new WebClient())
            {
                var cmd = new SelectChannelCommand(kind);
                string url = $"http://{ip}:80/post";
                cli.Headers.Set(HttpRequestHeader.ContentType, "application/json");
                cli.UploadData(url, "POST", Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(cmd)));

                if (kind == DisplayKind.Custom)
                {
                    cli.Headers.Set(HttpRequestHeader.ContentType, "application/json");
                    var cmd2 = new SetCustomPageIndex(customId.GetValueOrDefault(0));
                    cli.UploadData(url, "POST", Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(cmd2)));
                }
            }
        }


        public class GetIndexResponse
        {
            public string error_code { get; set; }
            public int SelectIndex { get; set; }
        }


        private static T SendCommand<T>(string ip, CommandData cmd)
        {
            using (var cli = new WebClient())
            {
                string url = $"http://{ip}:80/post";
                cli.Headers.Set(HttpRequestHeader.ContentType, "application/json");
                var tmp = cli.UploadString(url, "POST", JsonConvert.SerializeObject(cmd));
                return JsonConvert.DeserializeObject<T>(tmp);
            }
        }
    }
}
