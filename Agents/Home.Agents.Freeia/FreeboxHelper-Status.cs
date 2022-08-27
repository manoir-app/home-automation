using Home.Common;
using Home.Common.Messages;
using Home.Common.Model;
using Home.Graph.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Home.Agents.Freeia
{
    partial class FreeboxHelper
    {
        static List<string> _externalSsids = new List<string>();

        public static void Reboot()
        {

            var tk = GetToken();

            using (WebClient cli = new WebClient())
            {
                GetBaseUrl(cli);

                string sessTk = Login(tk, cli);

                cli.Headers.Add("X-Fbx-App-Auth", sessTk);
                string t = cli.UploadString("v4/system/reboot/", "POST", "");

                //cli.Headers.Add("X-Fbx-App-Auth", sessTk);
                //t = cli.UploadString("v4/login/logout/", "POST", "");
            }
        }
        public class GetConnectionStatusResponse
        {
            public bool success { get; set; }
            public GetConnectionStatusResponseResult result { get; set; }
        }

        public class GetConnectionStatusResponseResult
        {
            public string type { get; set; }
            public long rate_down { get; set; }
            public long bytes_up { get; set; }
            public int[] ipv4_port_range { get; set; }
            public long rate_up { get; set; }
            public long bandwidth_up { get; set; }
            public string ipv6 { get; set; }
            public long bandwidth_down { get; set; }
            public string media { get; set; }
            public string state { get; set; }
            public long bytes_down { get; set; }
            public string ipv4 { get; set; }
        }

        private class LastNetworkStatus
        {
            public bool Up { get; set; }
            public string Content { get; set; }
            public DateTimeOffset LastSent { get; set; } = DateTimeOffset.Now;
        }
        private static LastNetworkStatus _lastNetwork = null;

        public static void TestConnectionStatus()
        {
            var tk = GetToken();

            using (WebClient cli = new WebClient())
            {
                GetBaseUrl(cli);

                string sessTk = Login(tk, cli);

                try
                {
                    cli.Headers.Add("X-Fbx-App-Auth", sessTk);
                    string t = cli.DownloadString("v4/connection");

                    var cn = JsonConvert.DeserializeObject<GetConnectionStatusResponse>(t);
                    BssConfigData wifiConfig = null;
                    try
                    {
                        t = cli.DownloadString("v4/wifi/bss/");
                        var bss = JsonConvert.DeserializeObject<BssConfigResponse>(t);
                        wifiConfig = bss?.result.FirstOrDefault().config;
                    }
                    catch
                    {
                        wifiConfig = null;
                    }

                    if (cn != null)
                    {
                        InternetConnectionStatusRefresh r = new InternetConnectionStatusRefresh()
                        {
                            ConnectionId = "freebox",
                            ConnectionType = cn.result.type,
                            DownloadBandwith = cn.result.bandwidth_down,
                            UsedDownloadBandwith = cn.result.rate_down,
                            UploadBandwith = cn.result.bandwidth_up,
                            UsedUploadBandwith = cn.result.rate_up,
                        };

                        if (wifiConfig != null && !wifiConfig.hide_ssid)
                        {
                            r.Ssids = new string[] { wifiConfig.ssid };
                        }

                        bool isUp = false;
                        switch (cn.result.state)
                        {
                            case "down":
                                r.Status = ConnectionStatus.Down;
                                break;
                            case "going_down":
                                r.Status = ConnectionStatus.Failing;
                                break;
                            case "going_up":
                                r.Status = ConnectionStatus.Restarting;
                                break;
                            case "up":
                                r.Status = ConnectionStatus.Up;
                                isUp = true;
                                break;
                        }

                        if (!(_lastNetwork != null && _lastNetwork.Up == isUp
                                && _lastNetwork.Content == cn.result.state
                                 && Math.Abs((DateTimeOffset.Now - _lastNetwork.LastSent).TotalSeconds) < 10))
                        {

                            _lastNetwork = new LastNetworkStatus()
                            {
                                Content = cn.result.state,
                                Up = isUp
                            };
                            MqttHandler.PublishNetworkStatus(cn.result.state, isUp, cn.result.bandwidth_up, cn.result.bandwidth_down, cn.result.rate_up, cn.result.rate_down, r.Ssids);
                        }



                        for (int i = 0; i < 3; i++)
                        {
                            try
                            {
                                using (var mainapi = new MainApiAgentWebClient("freeia"))
                                {
                                    mainapi.UploadData<InternetConnection, InternetConnectionStatusRefresh>("v1.0/system/mesh/local/internet-connections", "POST", r);
                                    break;
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.Error.WriteLine(ex);
                            }
                        }

                        var externassids = new Dictionary<string, ApNeighborsResult>();
                        // puis on obtient les ssids "voisins"

                        t = cli.DownloadString("v4/wifi/ap/");
                        var aps = JsonConvert.DeserializeObject<ApResponse>(t);
                        if (aps.success)
                        {
                            foreach (var ap in aps.result)
                            {
                                t = cli.DownloadString($"v4/wifi/ap/{0}/neighbors");
                                var neis = JsonConvert.DeserializeObject<ApNeighborsResponse>(t);
                                if(neis!=null && neis.success && neis.result!=null)
                                {
                                    foreach(var nei in neis.result)
                                    {
                                        if (!externassids.ContainsKey(nei.ssid))
                                            externassids.Add(nei.ssid, nei);
                                    }
                                }
                            }
                        }

                        foreach(var key in externassids.Keys)
                        {
                            var nei = externassids[key];
                            MqttHandler.PublishExternalSsid(nei.ssid, nei.signal);

                            if (!_externalSsids.Contains(key))
                            {
                                MessagingHelper.PushToLocalAgent(new NetworkSsidDetectedMessage()
                                {
                                    SsidName = key
                                });
                                _externalSsids.Add(key);
                            }

                        }

                    }
                }
                finally
                {
                    try
                    {
                        var z = cli.UploadString("v4/login/logout/", "POST", "");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Impossible de fermer la session : " + ex.Message);
                    }
                }

            }
        }



        public class BssConfigResponse
        {
            public bool success { get; set; }
            public BssConfigResult[] result { get; set; }
        }

        public class BssConfigResult
        {
            public BssConfigData config { get; set; }
            public Bss_Params bss_params { get; set; }
            public Bss_Params shared_bss_params { get; set; }
            public string id { get; set; }
            public bool use_shared_params { get; set; }
            public int phy_id { get; set; }
            public BssStatus status { get; set; }
        }

        public class BssConfigData
        {
            public bool enabled { get; set; }
            public bool wps_enabled { get; set; }
            public string encryption { get; set; }
            public bool hide_ssid { get; set; }
            public int eapol_version { get; set; }
            public string key { get; set; }
            public string wps_uuid { get; set; }
            public bool use_default_config { get; set; }
            public string ssid { get; set; }
        }

        public class Bss_Params
        {
            public bool enabled { get; set; }
            public string wps_uuid { get; set; }
            public string ssid { get; set; }
            public string encryption { get; set; }
            public bool wps_enabled { get; set; }
            public bool hide_ssid { get; set; }
            public int eapol_version { get; set; }
            public string key { get; set; }
        }



        public class BssStatus
        {
            public string state { get; set; }
            public int sta_count { get; set; }
            public int authorized_sta_count { get; set; }
            public bool is_main_bss { get; set; }
        }

        public class ApResponse
        {
            public bool success { get; set; }
            public ApResultData[] result { get; set; }
        }

        public class ApResultData
        {
            public string name { get; set; }
            public int id { get; set; }
        }


        public class ApNeighborsResponse
        {
            public bool success { get; set; }
            public ApNeighborsResult[] result { get; set; }
        }

        public class ApNeighborsResult
        {
            public string channel_width { get; set; }
            public string ssid { get; set; }
            public int channel { get; set; }
            public string band { get; set; }
            public string bssid { get; set; }
            public int secondary_channel { get; set; }
            public int signal { get; set; }
        }


    }
}
