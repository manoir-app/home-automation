using Home.Common;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;

namespace Home.Agents.Erza
{
    public partial class NetworkChecker
    {
        public static DateTimeOffset? _offlineDate = null;

        private static void RunOfflineCheck()
        {
            // on maintient le graph en fonction
            CheckPublicGraph();

            if (!CheckGoogle())
            {
                if (!CheckGouv())
                {
                    if (!CheckByIp())
                    {
                        SetOffline();
                        return;
                    }
                }
            }

            if (_offlineDate.HasValue)
            {
                SetOnline();
            }

        }

        private static void SetOnline()
        {
            _offlineDate = null;
        }

        private static void SetOffline()
        {
            _offlineDate = DateTimeOffset.Now;

            var url = Environment.GetEnvironmentVariable("MAIN_API_SERVICE_HOST");

        }

        private static bool CheckByIp()
        {
            try
            {
                using (WebClient cli = new WebClient())
                {
                    string s = cli.DownloadString("https://1.1.1.1/");
                    ParseForOfflineMessages(s);
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        private static bool CheckGouv()
        {
            try
            {
                using (WebClient cli = new WebClient())
                {
                    string s = cli.DownloadString("https://www.gouvernement.fr/");
                    ParseForOfflineMessages(s);
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        private static bool CheckPublicGraph()
        {

            string publicUrl = HomeServerHelper.GetPublicGraphUrl();
            if(publicUrl==null)
                return true;


            if (!publicUrl.EndsWith("/"))
                publicUrl += "/";

            try
            {
                using (WebClient cli = new WebClient())
                {
                    cli.Headers.Add(HttpRequestHeader.Accept, "application/json");
                    string s = cli.DownloadString($"{publicUrl}v1.0/system/mesh/local/graph/check");
                    if (string.IsNullOrEmpty(s))
                        return false;
                    if (!("dev.carbenay.info:home-automation:proxy".Equals(s)))
                        return false;
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }



        private static bool CheckGoogle()
        {
            try
            {
                using (WebClient cli = new WebClient())
                {
                    string s = cli.DownloadString("https://www.google.com/");
                    ParseForOfflineMessages(s);
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        private static void ParseForOfflineMessages(string s)
        {

        }
    }
}
