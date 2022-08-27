using Home.Common;
using Home.Common.Messages;
using Home.Common.Model;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Web;

namespace Home.Agents.Freeia
{
    partial class FreeboxHelper
    {
        internal static MessageResponse EnumerateDevices()
        {
            var ret = new NetworkDeviceEnumerateMessageResponse()
            {

            };
            var tk = GetToken();

            using (WebClient cli = new WebClient())
            {
                GetBaseUrl(cli);
                string sessTk = Login(tk, cli);
                try
                {
                    string t;
                    BrowseNetworkResponse cn = GetNetworkDevices(cli, sessTk);
                    if (cn != null)
                    {
                        if (!cn.success)
                            return MessageResponse.GenericFail;
                        ret.Response = "ok";
                        ret.Agent = "freeia";
                        ret.Network = "local";
                        foreach (var itm in cn.result)
                        {
                            var mac = itm.l2ident.type.Equals("mac_address") ? itm.l2ident.id : null;
                            var dev = new NetworkDeviceData()
                            {
                                Id = itm.id,
                                MacAddress = mac,
                                Name = itm.primary_name,
                                Vendor = itm.vendor_name,
                                Model = itm.model
                            };
                            if (itm.l3connectivities != null)
                            {
                                dev.IpV4 = (from z in itm.l3connectivities
                                            where z.af.Equals("ipv4")
                                                && z.reachable
                                            select z.addr).FirstOrDefault();
                            }
                            dev.IpV6 = null;

                            if (itm.active && itm.reachable)
                            {
                                ret.ActiveDevices.Add(dev);
                            }
                            else
                            {
                                ret.InactiveDevices.Add(dev);
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

            return ret;
        }

        private static BrowseNetworkResponse GetNetworkDevices(WebClient cli, string sessTk)
        {
            BrowseNetworkResponse cn;
            cli.Headers.Add("X-Fbx-App-Auth", sessTk);
            string t = cli.DownloadString("v4/lan/browser/pub/");
            cn = JsonConvert.DeserializeObject<BrowseNetworkResponse>(t);
            return cn;
        }

        public class BrowseNetworkResponse
        {
            public bool success { get; set; }
            public BrowseNetworkResponseItem[] result { get; set; }
        }

        public class BrowseNetworkResponseItem
        {
            public L2ident l2ident { get; set; }
            public bool active { get; set; }
            public bool persistent { get; set; }
            public BrowseNetworkResponseItemName[] names { get; set; }
            public string vendor_name { get; set; }
            public string host_type { get; set; }
            public string _interface { get; set; }
            public string id { get; set; }
            public int last_time_reachable { get; set; }
            public bool primary_name_manual { get; set; }
            public string default_name { get; set; }
            public L3connectivities[] l3connectivities { get; set; }
            public bool reachable { get; set; }
            public int last_activity { get; set; }
            public Access_Point access_point { get; set; }
            public string primary_name { get; set; }
            public string model { get; set; }
        }

        public class L2ident
        {
            public string id { get; set; }
            public string type { get; set; }
        }

        public class Access_Point
        {
            public string mac { get; set; }
            public string type { get; set; }
            public string connectivity_type { get; set; }
            public string uid { get; set; }
            public Wifi_Information wifi_information { get; set; }
            public long rx_rate { get; set; }
            public long tx_rate { get; set; }
        }

        public class Wifi_Information
        {
            public string band { get; set; }
            public string ssid { get; set; }
            public int signal { get; set; }
        }

        public class BrowseNetworkResponseItemName
        {
            public string name { get; set; }
            public string source { get; set; }
        }

        public class L3connectivities
        {
            public string addr { get; set; }
            public bool active { get; set; }
            public bool reachable { get; set; }
            public int last_activity { get; set; }
            public string af { get; set; }
            public int last_time_reachable { get; set; }
        }


    }
}
