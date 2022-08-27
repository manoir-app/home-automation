using Home.Common;
using Home.Common.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Web;

namespace Home.Agents.Freeia
{
    partial class FreeboxHelper
    {
        private static List<DownloadItem> _currentDownload = null;
        private static DateTime _lastRefreshCurrentDownload = DateTime.MinValue;
        internal static void CheckDownloadsStatus()
        {
            List<DownloadItem> tmp = null;

            for (int i = 0; i < 3; i++)
            {
                try
                {
                    using (var apicli = new MainApiAgentWebClient("freeia"))
                    {
                        var items = apicli.DownloadData<List<DownloadItem>>("v1.0/downloads/queued");
                        tmp = items;
                    }
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }

            foreach (var item in tmp)
            {
                int id = -1;
                if (item.DownloadPrivateId == null)
                {
                    if (_startedDownloads.TryGetValue(item.SourceUrl, out id))
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            try
                            {
                                using (var apicli = new MainApiAgentWebClient("freeia"))
                                {
                                    Console.WriteLine("Updating queued item that was in progress #" + id + " - DONE !");
                                    apicli.DownloadString("v1.0/downloads/" + item.Id + "/markasinprogress?privateid=freebox:downloadmgr:" + id);
                                }
                                break;
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex);
                            }
                        }
                    }
                }
            }

            if (_lastRefreshCurrentDownload < DateTime.Now.AddMinutes(-2))
                _currentDownload = null;

            tmp = _currentDownload;
            if (tmp == null)
            {
                for (int i = 0; i < 3; i++)
                {
                    try
                    {
                        using (var apicli = new MainApiAgentWebClient("freeia"))
                        {
                            var items = apicli.DownloadData<List<DownloadItem>>("v1.0/downloads/inprogress");
                            _currentDownload = items;
                            tmp = items;
                            _lastRefreshCurrentDownload = DateTime.Now;
                        }
                        break;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                }
            }
            bool hasCompleted = false;
            if (tmp != null && tmp.Count > 0)
            {
                var tk = GetToken();

                using (WebClient cli = new WebClient())
                {
                    GetBaseUrl(cli);
                    string sessTk = Login(tk, cli);

                    foreach (var item in tmp)
                    {
                        if (item.DownloadPrivateId != null && item.DownloadPrivateId.StartsWith("freebox:downloadmgr:"))
                        {
                            if (string.IsNullOrEmpty(cli.Headers["X-Fbx-App-Auth"]))
                                cli.Headers.Add("X-Fbx-App-Auth", sessTk);

                            string itemNum = item.DownloadPrivateId.Substring(20);
                            string tmpst = null;
                            try
                            {
                                tmpst = cli.DownloadString("v4/downloads/" + itemNum);
                            }
                            catch(Exception e)
                            {
                                Console.WriteLine("Error while getting v4/downloads/" + itemNum + " : " + e);
                                tmpst = null;
                            }
                            if (tmpst == null)
                                continue;
                            var status = JsonConvert.DeserializeObject<DownloadStatusResponse>(tmpst);
                            if (status.success)
                            {
                                switch (status.result.status.ToLowerInvariant())
                                {
                                    case "seeding":
                                    case "done":
                                        Console.WriteLine("Checking status of download #" + itemNum + " - DONE !");
                                        // on récupère les fichiers
                                        List<DownloadItemResult> results = GetDownloadFiles(itemNum, cli, sessTk);

                                        for (int i = 0; i < 3; i++)
                                        {
                                            try
                                            {
                                                using (var apicli = new MainApiAgentWebClient("freeia"))
                                                {
                                                    apicli.UploadData<bool, List<DownloadItemResult>>("v1.0/downloads/" + item.Id + "/markascompleted", "POST", results);
                                                }
                                                hasCompleted = true;

                                                if (string.IsNullOrEmpty(cli.Headers["X-Fbx-App-Auth"]))
                                                    cli.Headers.Add("X-Fbx-App-Auth", sessTk);

                                                AgentHelper.UpdateStatusWithInfo("freeia", "Download of " + item.Title + " : done");

                                                cli.UploadData("v4/downloads/" + itemNum, "DELETE", new byte[0]);
                                                break;
                                            }
                                            catch (Exception ex)
                                            {
                                                Console.WriteLine(ex);
                                            }
                                        }

                                        break;
                                }
                            }
                        }
                    }
                }

            }

            if (hasCompleted)
                _currentDownload = null;




        }

        private static List<DownloadItemResult> GetDownloadFiles(string itemNum, WebClient cli, string sessTk)
        {
            if (string.IsNullOrEmpty(cli.Headers["X-Fbx-App-Auth"]))
                cli.Headers.Add("X-Fbx-App-Auth", sessTk);

            var json = cli.DownloadString($"v4/downloads/{itemNum}/files");

            Console.WriteLine($"Files for download {itemNum} : {json}");

            if (json == null)
                return new List<DownloadItemResult>();
            var ret = new List<DownloadItemResult>();
            var res = JsonConvert.DeserializeObject<GetDownloadFilesResponse>(json);
            if(res.success)
            {
                foreach(var fil in res.result)
                {
                    var it = new DownloadItemResult()
                    {
                        Filename = fil.name
                    };
                    it.Links.Add(new DownloadItemResultFileLink()
                    {
                        Kind = DownloadItemResultFileLinkKind.Ftp,
                        Uri = "ftp://mafreebox.freebox.fr" + fil.path
                    });

                    it.Links.Add(new DownloadItemResultFileLink()
                    {
                        Kind = DownloadItemResultFileLinkKind.Smb,
                        Uri = "\\\\mafreebox.freebox.fr" + fil.path.Replace("/", "\\")
                    });

                    ret.Add(it);
                }
            }

            return ret;
        }



        private class GetDownloadFilesResponse
        {
            public bool success { get; set; }
            public GetDownloadFilesResponseItem[] result { get; set; }
        }

        private class GetDownloadFilesResponseItem
        {
            public string path { get; set; }
            public string id { get; set; }
            public string task_id { get; set; }
            public string filepath { get; set; }
            public string mimetype { get; set; }
            public string name { get; set; }
            public long rx { get; set; }
            public string status { get; set; }
            public string priority { get; set; }
            public string error { get; set; }
            public long size { get; set; }
        }



        private static Dictionary<string, int> _startedDownloads = new Dictionary<string, int>();


        internal static DownloadAddResponseItem StartDownload(string sourceUrl)
        {
            int id = -1;
            if (_startedDownloads.TryGetValue(sourceUrl, out id))
            {
                return new DownloadAddResponseItem()
                {
                    id = id
                };
            }

            var tk = GetToken();

            using (WebClient cli = new WebClient())
            {
                GetBaseUrl(cli);

                string sessTk = Login(tk, cli);

                cli.Headers.Add("X-Fbx-App-Auth", sessTk);
                cli.Headers.Add(HttpRequestHeader.ContentType, "application/x-www-form-urlencoded");
                string t = cli.UploadString("v4/downloads/add", "POST", "download_url=" + HttpUtility.UrlEncode(sourceUrl));
                Console.WriteLine("Ajout téléchargment : résultat = " + t);
                var r = JsonConvert.DeserializeObject<DownloadAddResponse>(t);
                if (r != null && r.success)
                {
                    AgentHelper.UpdateStatusWithInfo("freeia", "Added task to download : id = " + r.result.id);
                    id = r.result.id;
                    _startedDownloads.Add(sourceUrl, id);
                    _currentDownload = null;
                    return r.result;
                }
            }

            return null;
        }


        public class DownloadAddResponse
        {
            public DownloadAddResponseItem result { get; set; }
            public bool success { get; set; }
        }

        public class DownloadAddResponseItem
        {
            public int id { get; set; }
        }



        public class DownloadStatusResponse
        {
            public bool success { get; set; }
            public DownloadStatusResponseItem result { get; set; }
        }

        public class DownloadStatusResponseItem
        {
            public long rx_bytes { get; set; }
            public long tx_bytes { get; set; }
            public string download_dir { get; set; }
            public string archive_password { get; set; }
            public long eta { get; set; }
            public string status { get; set; }
            public string io_priority { get; set; }
            public long size { get; set; }
            public string type { get; set; }
            public string error { get; set; }
            public long queue_pos { get; set; }
            public long id { get; set; }
            public long created_ts { get; set; }
            public long tx_rate { get; set; }
            public string name { get; set; }
            public long rx_pct { get; set; }
            public long rx_rate { get; set; }
            public long tx_pct { get; set; }
        }


    }
}
