using Home.Common.Model;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Home.Common
{
    public static class HomeServerHelper
    {
        internal static AutomationMesh _cache;

        public static string GetLocalIP()
        {

            var server = Environment.GetEnvironmentVariable("LAN_SERVER_IP");
            if (string.IsNullOrEmpty(server))
                server = "192.168.2.184";

            IPAddress ip;
            if (!IPAddress.TryParse(server, out ip))
            {
                try
                {
                    var dns = Dns.GetHostAddresses(server);
                    foreach (var r in dns)
                    {
                        if (r.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                            return r.ToString();
                    }
                }
                catch
                {

                }

                return server;
            }
            else
                return server;
        }

        public static string GetPublicGraphUrl(string subPath)
        {
            var t = GetPublicGraphUrl();
            if (string.IsNullOrEmpty(subPath))
                return t;
            if (!t.EndsWith("/"))
                t = t + "/";
            if (subPath.StartsWith("/"))
                subPath = subPath.Substring(1);
            return t + subPath;
        }

        public static string GetPublicGraphUrl()
        {
            var server = Environment.GetEnvironmentVariable("LAN_SERVER_IP");

            if (server == null)
                return "https://public.anzin.carbenay.manoir.app/";

            if (server.Contains(".184"))
                return "https://public.dev.carbenay.manoir.app/";

            return "https://public.anzin.carbenay.manoir.app/";

        }


        public static string GetLocalGraphUrl(string subPath)
        {
            var t = GetLocalGraphUrl();
            if (string.IsNullOrEmpty(subPath))
                return t;
            if (!t.EndsWith("/"))
                t = t + "/";
            if (subPath.StartsWith("/"))
                subPath = subPath.Substring(1);
            return t + subPath;
        }

        public static string GetLocalGraphUrl()
        {
            var server = Environment.GetEnvironmentVariable("LAN_SERVER_IP");

            if (server == null)
                return "https://home.anzin.carbenay.manoir.app/";

            if (server.Contains(".184"))
                return "https://home.dev.carbenay.manoir.app/";

            return "https://home.anzin.carbenay.manoir.app/";

        }
    }
}
