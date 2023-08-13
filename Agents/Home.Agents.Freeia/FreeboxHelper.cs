using Home.Common;
using Home.Common.Messages;
using Home.Common.Model;
using Home.Graph.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;

namespace Home.Agents.Freeia
{
    public static partial class FreeboxHelper
    {
        private static string root1 = @"MIIFmjCCA4KgAwIBAgIJAKLyz15lYOrYMA0GCSqGSIb3DQEBCwUAMFoxCzAJBgNV
BAYTAkZSMQ8wDQYDVQQIDAZGcmFuY2UxDjAMBgNVBAcMBVBhcmlzMRAwDgYDVQQK
DAdGcmVlYm94MRgwFgYDVQQDDA9GcmVlYm94IFJvb3QgQ0EwHhcNMTUwNzMwMTUw
OTIwWhcNMzUwNzI1MTUwOTIwWjBaMQswCQYDVQQGEwJGUjEPMA0GA1UECAwGRnJh
bmNlMQ4wDAYDVQQHDAVQYXJpczEQMA4GA1UECgwHRnJlZWJveDEYMBYGA1UEAwwP
RnJlZWJveCBSb290IENBMIICIjANBgkqhkiG9w0BAQEFAAOCAg8AMIICCgKCAgEA
xqYIvq8538SH6BJ99jDlOPoyDBrlwKEp879oYplicTC2/p0X66R/ft0en1uSQadC
sL/JTyfgyJAgI1Dq2Y5EYVT/7G6GBtVH6Bxa713mM+I/v0JlTGFalgMqamMuIRDQ
tdyvqEIs8DcfGB/1l2A8UhKOFbHQsMcigxOe9ZodMhtVNn0mUyG+9Zgu1e/YMhsS
iG4Kqap6TGtk80yruS1mMWVSgLOq9F5BGD4rlNlWLo0C3R10mFCpqvsFU+g4kYoA
dTxaIpi1pgng3CGLE0FXgwstJz8RBaZObYEslEYKDzmer5zrU1pVHiwkjsgwbnuy
WtM1Xry3Jxc7N/i1rxFmN/4l/Tcb1F7x4yVZmrzbQVptKSmyTEvPvpzqzdxVWuYi
qIFSe/njl8dX9v5hjbMo4CeLuXIRE4nSq2A7GBm4j9Zb6/l2WIBpnCKtwUVlroKw
NBgB6zHg5WI9nWGuy3ozpP4zyxqXhaTgrQcDDIG/SQS1GOXKGdkCcSa+VkJ0jTf5
od7PxBn9/TuN0yYdgQK3YDjD9F9+CLp8QZK1bnPdVGywPfL1iztngF9J6JohTyL/
VMvpWfS/X6R4Y3p8/eSio4BNuPvm9r0xp6IMpW92V8SYL0N6TQQxzZYgkLV7TbQI
Hw6v64yMbbF0YS9VjS0sFpZcFERVQiodRu7nYNC1jy8CAwEAAaNjMGEwHQYDVR0O
BBYEFD2erMkECujilR0BuER09FdsYIebMB8GA1UdIwQYMBaAFD2erMkECujilR0B
uER09FdsYIebMA8GA1UdEwEB/wQFMAMBAf8wDgYDVR0PAQH/BAQDAgGGMA0GCSqG
SIb3DQEBCwUAA4ICAQAZ2Nx8mWIWckNY8X2t/ymmCbcKxGw8Hn3BfTDcUWQ7GLRf
MGzTqxGSLBQ5tENaclbtTpNrqPv2k6LY0VjfrKoTSS8JfXkm6+FUtyXpsGK8MrLL
hZ/YdADTfbbWOjjD0VaPUoglvo2N4n7rOuRxVYIij11fL/wl3OUZ7GHLgL3qXSz0
+RGW+1oZo8HQ7pb6RwLfv42Gf+2gyNBckM7VVh9R19UkLCsHFqhFBbUmqwJgNA2/
3twgV6Y26qlyHXXODUfV3arLCwFoNB+IIrde1E/JoOry9oKvF8DZTo/Qm6o2KsdZ
dxs/YcIUsCvKX8WCKtH6la/kFCUcXIb8f1u+Y4pjj3PBmKI/1+Rs9GqB0kt1otyx
Q6bqxqBSgsrkuhCfRxwjbfBgmXjIZ/a4muY5uMI0gbl9zbMFEJHDojhH6TUB5qd0
JJlI61gldaT5Ci1aLbvVcJtdeGhElf7pOE9JrXINpP3NOJJaUSueAvxyj/WWoo0v
4KO7njox8F6jCHALNDLdTsX0FTGmUZ/s/QfJry3VNwyjCyWDy1ra4KWoqt6U7SzM
d5jENIZChM8TnDXJzqc+mu00cI3icn9bV9flYCXLTIsprB21wVSMh0XeBGylKxeB
S27oDfFq04XSox7JM9HdTt2hLK96x1T7FpFrBTnALzb7vHv9MhXqAT90fPR/8A==";


        private static string root2 = @"MIICWTCCAd+gAwIBAgIJAMaRcLnIgyukMAoGCCqGSM49BAMCMGExCzAJBgNVBAYT
AkZSMQ8wDQYDVQQIDAZGcmFuY2UxDjAMBgNVBAcMBVBhcmlzMRMwEQYDVQQKDApG
cmVlYm94IFNBMRwwGgYDVQQDDBNGcmVlYm94IEVDQyBSb290IENBMB4XDTE1MDkw
MTE4MDIwN1oXDTM1MDgyNzE4MDIwN1owYTELMAkGA1UEBhMCRlIxDzANBgNVBAgM
BkZyYW5jZTEOMAwGA1UEBwwFUGFyaXMxEzARBgNVBAoMCkZyZWVib3ggU0ExHDAa
BgNVBAMME0ZyZWVib3ggRUNDIFJvb3QgQ0EwdjAQBgcqhkjOPQIBBgUrgQQAIgNi
AASCjD6ZKn5ko6cU5Vxh8GA1KqRi6p2GQzndxHtuUmwY8RvBbhZ0GIL7bQ4f08ae
JOv0ycWjEW0fyOnAw6AYdsN6y1eNvH2DVfoXQyGoCSvXQNAUxla+sJuLGICRYiZz
mnijYzBhMB0GA1UdDgQWBBTIB3c2GlbV6EIh2ErEMJvFxMz/QTAfBgNVHSMEGDAW
gBTIB3c2GlbV6EIh2ErEMJvFxMz/QTAPBgNVHRMBAf8EBTADAQH/MA4GA1UdDwEB
/wQEAwIBhjAKBggqhkjOPQQDAgNoADBlAjA8tzEMRVX8vrFuOGDhvZr7OSJjbBr8
gl2I70LeVNGEXZsAThUkqj5Rg9bV8xw3aSMCMQCDjB5CgsLH8EdZmiksdBRRKM2r
vxo6c0dSSNrr7dDN+m2/dRvgoIpGL2GauOGqDFY=";

        private static X509Certificate2 rootCert1 = null;
        private static X509Certificate2 rootCert2 = null;

        public static void Start()
        {
            NetworkConnectionHelper.Init("freeia");

            var t = new Thread(() => FreeboxHelper.Run());
            t.Name = "Connection status checker";
            t.Start();
        }

        public static void Stop()
        {
            _stop = true;
        }


        public static bool _stop = false;

        private static void Run()
        {
            while (!_stop)
            {
                try
                {
                    TestConnectionStatus();
                    CheckDownloadsStatus();
                    RefreshAppliancesStatus();
                    Console.Write(".");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }

                for (int i = 0; i < 60 && !_stop; i++)
                {
                    Thread.Sleep(500);
                }
            }

        }


        private class DeviceStatus
        {
            public string Ipv4 { get; set; }
            public string IpV6 { get; set; }
            public string Name { get; set; }
            public bool Reachable { get; set; }
            public DateTimeOffset LastSent { get; set; } = DateTimeOffset.Now;
        }

        private static Dictionary<string, DeviceStatus> _devices = new Dictionary<string, DeviceStatus>();


        public static void RefreshAppliancesStatus()
        {
            var deviceResponse = EnumerateDevices() as NetworkDeviceEnumerateMessageResponse;
            if (deviceResponse != null) // null = plantage
            {
                DeviceStatus stat = null;

                var allDevs = (deviceResponse as NetworkDeviceEnumerateMessageResponse);
                foreach (var dev in allDevs.ActiveDevices)
                {
                    if (!_devices.TryGetValue(dev.Id, out stat))
                        stat = null;
                    if (stat != null && stat.Reachable == true
                        && stat.Ipv4 == dev.IpV4 && stat.IpV6 == dev.IpV6
                        && stat.Name == dev.Name
                        && Math.Abs((DateTimeOffset.Now - stat.LastSent).TotalMinutes) < 60)
                        continue;

                    if (stat != null)
                    {
                        if (stat.Reachable == false)
                            RaiseDeviceTriggerIfNeeded(dev, true);
                        _devices.Remove(dev.Id);
                    }
                    stat = new DeviceStatus()
                    {
                        Ipv4 = dev.IpV4,
                        IpV6 = dev.IpV6,
                        Reachable = true,
                        Name = dev.Name
                    };

                    _devices.Add(dev.Id, stat);


                    NetworkConnectionHelper.PublishDeviceStatus(dev.Id, true, dev.IpV4, dev.IpV6, dev.Name);
                }
                foreach (var dev in allDevs.InactiveDevices)
                {
                    if (!_devices.TryGetValue(dev.Id, out stat))
                        stat = null;
                    if (stat != null && stat.Reachable == false
                        && stat.Ipv4 == dev.IpV4 && stat.IpV6 == dev.IpV6
                        && stat.Name == dev.Name
                        && Math.Abs((DateTimeOffset.Now - stat.LastSent).TotalMinutes) < 60)
                        continue;

                    if (stat != null)
                    {
                        if (stat.Reachable == true)
                            RaiseDeviceTriggerIfNeeded(dev, false);
                        _devices.Remove(dev.Id);
                    }
                    stat = new DeviceStatus()
                    {
                        Ipv4 = dev.IpV4,
                        IpV6 = dev.IpV6,
                        Reachable = false,
                        Name = dev.Name
                    };

                    _devices.Add(dev.Id, stat);

                    NetworkConnectionHelper.PublishDeviceStatus(dev.Id, false, dev.IpV4, dev.IpV6, dev.Name);
                }
            }
        }

        private static List<Trigger> _triggers = null;

        public static void Reload()
        {
            using (var cli = new MainApiAgentWebClient("freeia"))
            {
                var lst = cli.DownloadData<List<Trigger>>("v1.0/system/mesh/local/triggers");

                _triggers = (from z in lst
                             where z.Kind == TriggerKind.NetworkDeviceConnectionChanged
                             select z).ToList();
                foreach (var t in _triggers)
                {
                    Console.WriteLine($"Trigger {t.Id} : {JsonConvert.SerializeObject(t)}");
                }
            }
        }

        private static void RaiseDeviceTriggerIfNeeded(NetworkDeviceData dev, bool isReachable)
        {
            if (_triggers == null)
                Reload();

            foreach (var t in _triggers)
            {
                if (t.NetworkDeviceName.Equals(dev.Id)
                    || t.NetworkDeviceName.Equals(dev.Name))
                {
                    if (t.NetworkDeviceTriggerKind.HasValue)
                    {
                        if (t.NetworkDeviceTriggerKind == NetworkDeviceTriggerKind.Connection && !isReachable)
                            continue;
                        if (t.NetworkDeviceTriggerKind == NetworkDeviceTriggerKind.Disconnection && isReachable)
                            continue;
                    }

                    for (int i = 0; i < 3; i++)
                    {
                        using (var cli = new MainApiAgentWebClient("sarah"))
                        {
                            var done = cli.UploadData<bool, NetworkDeviceData>($"v1.0/system/mesh/local/triggers/{t.Id}/raise", "POST", dev);
                            if (done)
                            {
                                Console.WriteLine("Trigger : " + t.Id + " déclenché");
                                break;
                            }
                        }
                    }
                }
            }
        }

        static FreeboxHelper()
        {
            rootCert1 = new X509Certificate2(Convert.FromBase64String(root1));
            rootCert2 = new X509Certificate2(Convert.FromBase64String(root2));

            ServicePointManager.ServerCertificateValidationCallback +=
            (sender, cert, chain, error) =>
            {
                return true;
            };
        }


        private static ExternalToken _token;

        public static ExternalToken GetToken()
        {
            if (_token == null)
            {
                for (int i = 0; i < 3; i++)
                {
                    try
                    {
                        using (var cli = new MainApiAgentWebClient("freeia"))
                        {
                            var exts = cli.DownloadData<List<ExternalToken>>("/v1.0/security/tokens/system/freebox");
                            if (exts != null && exts.Count >= 1)
                                _token = exts[0];
                            else
                            {

                            }
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                }

            }

            return _token;
        }

        public class ApiVersion
        {
            public string box_model_name { get; set; }
            public string api_base_url { get; set; }
            public int https_port { get; set; }
            public string device_name { get; set; }
            public bool https_available { get; set; }
            public string box_model { get; set; }
            public string api_domain { get; set; }
            public string uid { get; set; }
            public string api_version { get; set; }
            public string device_type { get; set; }
        }


        public class LoginAppRequest
        {
            public string app_id { get; set; }
            public string app_name { get; set; }
            public string app_version { get; set; }
            public string device_name { get; set; }
        }


        public class LoginAppResponse
        {
            public bool success { get; set; }
            public LoginAppResponseResult result { get; set; }
        }

        public class LoginAppResponseResult
        {
            public string app_token { get; set; }
            public int track_id { get; set; }
        }


        public class LoginAppEndResponse
        {
            public bool success { get; set; }
            public LoginAppEndResponseResult result { get; set; }
        }

        public class LoginAppEndResponseResult
        {
            public string status { get; set; }
            public string challenge { get; set; }
        }


        public static void StartAuth()
        {
            ApiVersion vers = null;
            try
            {
                using (WebClient cli = new WebClient())
                    vers = GetApiVersion(cli);
            }
            catch (WebException)
            {
                // pas de freebox, probablement
                return;
            }



            try
            {
                using (WebClient cli = new WebClient())
                {
                    cli.BaseAddress = $"http://mafreebox.freebox.fr";
                    var json = JsonConvert.SerializeObject(new LoginAppRequest()
                    {
                        app_id = "home.agents.freeia.carbenay.dev",
                        app_name = "Home Automation : Freeia",
                        app_version = "1.0",
                        device_name = "Freeia"
                    });

                    var t = cli.UploadString("api/v4/login/authorize/", "POST", json);
                    var id = JsonConvert.DeserializeObject<LoginAppResponse>(t);

                    int retry = 0;
                    while (retry < 5)
                    {
                        try
                        {
                            t = cli.DownloadString("/api/v4/login/authorize/" + id.result.track_id);
                            var resp = JsonConvert.DeserializeObject<LoginAppEndResponse>(t);
                            if (resp != null)
                            {
                                switch (resp.result.status.ToLowerInvariant())
                                {
                                    case "granted":
                                        SaveToken(id);
                                        return;
                                }
                            }

                            Thread.Sleep(1000);
                        }
                        catch
                        {
                            Thread.Sleep(1000);
                            retry++;
                        }
                    }

                }
            }
            catch (WebException)
            {
                // pas de freebox, probablement
                return;
            }

        }

        private static void SaveToken(LoginAppResponse id)
        {
            using (var cli = new MainApiAgentWebClient("freeia"))
            {
                var js = JsonConvert.SerializeObject(new ExternalToken()
                {
                    ExpiresAt = null,
                    Token = id.result.app_token,
                });
                cli.UploadString("/v1.0/security/tokens/system/freebox", "PUT", js);
            }
        }



        public class GetChallengeResponse
        {
            public bool success { get; set; }
            public GetChallengeResponseResult result { get; set; }
        }

        public class GetChallengeResponseResult
        {
            public string status { get; set; }
            public string challenge { get; set; }
        }


        public class OpenSessionRequest
        {
            public string app_id { get; set; }
            public string password { get; set; }
        }


        public class OpenSessionResponse
        {
            public bool success { get; set; }
            public OpenSessionResponseResult result { get; set; }
        }

        public class OpenSessionResponseResult
        {
            public string session_token { get; set; }
            public string challenge { get; set; }
            public OpenSessionResponsePermissions permissions { get; set; }
        }

        public class OpenSessionResponsePermissions
        {
            public bool downloader { get; set; }
        }


        private static ApiVersion GetApiVersion(WebClient cli)
        {
            var t = cli.DownloadString("http://mafreebox.freebox.fr/api_version");
            return JsonConvert.DeserializeObject<ApiVersion>(t);
        }

        private static string GetBaseUrl(WebClient cli)
        {
            ApiVersion vers = null;
            try
            {
                vers = GetApiVersion(cli);
            }
            catch (WebException)
            {
                // pas de freebox, probablement
                cli.BaseAddress = "http://mafreebox.freebox.fr/api/";
                return cli.BaseAddress;
            }

            if (vers.https_available)
            {
                cli.BaseAddress = $"https://{vers.api_domain}:{vers.https_port}{vers.api_base_url}";
                return cli.BaseAddress;
            }

            cli.BaseAddress = "http://mafreebox.freebox.fr/api/";
            return cli.BaseAddress;
        }





        private static string Login(ExternalToken tk, WebClient cli)
        {
            GetBaseUrl(cli);

            string t = cli.DownloadString("v4/login/");
            var chal = JsonConvert.DeserializeObject<GetChallengeResponse>(t);

            var byts = Encoding.UTF8.GetBytes(tk.Token);
            var sha1 = new HMACSHA1(byts);
            byts = Encoding.UTF8.GetBytes(chal.result.challenge);

            byts = sha1.ComputeHash(byts);
            StringBuilder blr = new StringBuilder();
            foreach (var c in byts)
                blr.Append(c.ToString("x2"));

            var json = JsonConvert.SerializeObject(new OpenSessionRequest()
            {
                app_id = "home.agents.freeia.carbenay.dev",
                password = blr.ToString()
            });


            t = cli.UploadString("v4/login/session/", "POST", json);
            var sess = JsonConvert.DeserializeObject<OpenSessionResponse>(t);

            return sess.result.session_token;
        }
    }
}
