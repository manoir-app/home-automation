using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Home.Agents.Sarah.Devices.ZipatoBox
{
    public static class ZipatoBoxHelper
    {

        private static string _zipaboxId = null;
        private static string _user = null;
        private static string _password = null;
        private static bool _isConnected = false;
        private static string _localServer = null;


        private class BoxStatusResponse
        {
            public string localIp { get; set; }
        }

        private class UserInitResponse
        {
            public bool success { get; set; }
            public string error { get; set; }
            public string jsessionid { get; set; }
            public string nonce { get; set; }
            public string[] errors { get; set; }
        }

        public class Config
        {
            public string BoxId { get; set; }
            public string Email { get; set; }
            public string Password { get; set; }
            public string LocalServer { get; set; }
        }


        public static void Init(Config cfg)
        {
            // until zipato settings are OK
            // _localServer = "192.168.0.5";

            if(cfg==null)
                throw new ApplicationException("La configuration pour la connexion à la zipabox est incorrecte");

            _zipaboxId = cfg.BoxId;
            _user = cfg.Email;
            _password = cfg.Password;
            _localServer = cfg.LocalServer;

            Connect();
        }


        public static void Connect()
        {
            try
            {
                string tmp = DownloadString("user/init");
                var r = JsonConvert.DeserializeObject<UserInitResponse>(tmp);
                if (r.success)
                {
                    cookieSessionId = r.jsessionid;
                    SHA1 mg = new SHA1CryptoServiceProvider();
                    var b = mg.ComputeHash(Encoding.ASCII.GetBytes(_password));
                    b = mg.ComputeHash(Encoding.ASCII.GetBytes(r.nonce + ToHex(b)));
                    string url = string.Format("/user/login?username={0}&token={1}&serial={2}",
                        HttpUtility.UrlEncode(_user),
                        HttpUtility.UrlEncode(ToHex(b)),
                        HttpUtility.UrlEncode(_zipaboxId));

                    tmp = DownloadString(url);
                    r = JsonConvert.DeserializeObject<UserInitResponse>(tmp);
                    if (r.success)
                    {
                        _isConnected = true;
                    }

                    if (_localServer == null)
                    {
                        UpdateWithLocalServer();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                _isConnected = false;
            }
        }

        private static void UpdateWithLocalServer()
        {
            if (!string.IsNullOrEmpty(_localServer))
                return;

            try
            {
                var jscfg = DownloadString("box");
                if (jscfg != null)
                {
                    var cfg = JsonConvert.DeserializeObject<GetBoxResponse>(jscfg);
                    if (cfg != null)
                    {
                        _localServer = cfg.localIp;
                        // vu qu'on bascule sur l'ip locale, on réinit le cookie de session
                        cookieSessionId = null;
                        Connect();
                    }
                }
            }
            catch (Exception ex)
            {
                // tant pis, on se servira de la version web

            }
        }



        private class GetBoxResponse
        {
            public string serial { get; set; }
            public string name { get; set; }
            public string remoteIp { get; set; }
            public string localIp { get; set; }
            public string timezone { get; set; }
            public int gmtOffset { get; set; }
            public bool online { get; set; }
            public string firmwareVersion { get; set; }
            public string latestFirmwareVersion { get; set; }
            public bool firmwareUpgradeAvailable { get; set; }
            public bool firmwareUpgradeRequired { get; set; }
            public bool needSync { get; set; }
            public DateTime saveDate { get; set; }
            public DateTime syncDate { get; set; }
            public object[] tags { get; set; }
            public object[] packageTags { get; set; }
            public GetBoxResponseConfig config { get; set; }
            public bool exclusive { get; set; }
            public bool setupComplete { get; set; }
            public DateTime lastDeviceAdded { get; set; }
        }

        private class GetBoxResponseConfig
        {
            public object className { get; set; }
            public string dateFormat { get; set; }
            public int timeZoneId { get; set; }
            public string staticIp { get; set; }
            public string simPIN { get; set; }
            public int timeZone { get; set; }
            public bool keepOnline { get; set; }
            public string temperatureScale { get; set; }
            public object autoRebootAt { get; set; }
            public string mtu { get; set; }
            public string simUsername { get; set; }
            public string proxy { get; set; }
            public string simPassword { get; set; }
            public string staticDns2 { get; set; }
            public string staticDns1 { get; set; }
            public string staticGateway { get; set; }
            public string timeFormat { get; set; }
            public string name { get; set; }
            public int ledBrightness { get; set; }
            public string currency { get; set; }
            public string staticNetmask { get; set; }
            public string simAPN { get; set; }
            public string clusterBoxSerial { get; set; }
        }


        private static string ToHex(byte[] allbytes)
        {
            StringBuilder blr = new StringBuilder();
            foreach (byte b in allbytes)
            {
                blr.Append(b.ToString("x2"));
            }
            return blr.ToString();
        }

        private static string GetRootUrl()
        {
            if (string.IsNullOrEmpty(_localServer))
                return "https://my.zipato.com:443/zipato-web/v2/";
            else
                return string.Format("http://{0}:8080/zipato-web/", _localServer);
        }

        private static string cookieSessionId = null;

        public static string DownloadString(string relativeUrl)
        {
            string rootUrl = GetRootUrl();

            if (relativeUrl.StartsWith("/"))
                relativeUrl = relativeUrl.Substring(1);
            if (!rootUrl.EndsWith("/"))
                rootUrl = rootUrl + "/";
            string url = string.Format("{0}{1}", rootUrl, relativeUrl);

            var cookieContainer = new CookieContainer();

            HttpClientHandler hndl = new HttpClientHandler() { CookieContainer = cookieContainer };
            if (!string.IsNullOrEmpty(cookieSessionId))
            {
                Uri uri = new Uri(url, UriKind.Absolute);
                uri = new Uri(string.Format("{0}://{1}{2}", uri.Scheme, uri.Host, uri.IsDefaultPort ? "" : (":" + uri.Port.ToString())));
                cookieContainer.Add(uri, new Cookie("JSESSIONID", cookieSessionId));
            }
            hndl.UseCookies = true;
            HttpClient cli = new HttpClient(hndl);
            cli.DefaultRequestHeaders.Add(HttpRequestHeader.UserAgent.ToString(), "a.u.r.o.r.e.");
            HttpRequestMessage msg = new HttpRequestMessage(HttpMethod.Get, url);

            try
            {
                var r = cli.SendAsync(msg).Result;
                if (r.IsSuccessStatusCode)
                    return r.Content.ReadAsStringAsync().Result;
                else
                    throw new ApplicationException(r.ReasonPhrase);
            }
            catch (AggregateException ex)
            {
                if (ex.InnerException != null)
                    throw ex.InnerException;
                else
                    throw;
            }
        }

    }
}
