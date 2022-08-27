using Home.Common;
using Home.Common.Model;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;

namespace Home.Agents.Clara
{
    public static class NewsChecker
    {

        public static void Start()
        {
            var t = new Thread(() => NewsChecker.Run());
            t.Name = "Get News";
            t.Start();
        }

        public static void Stop()
        {
            _stop = true;
        }


        public static bool _stop = false;
        public static DateTimeOffset? _offlineDate = null;

        private static void Run()
        {
            while (!_stop)
            {
                try
                {
                    for (int i = 0; i < 1200; i++)
                    {
                        if (_stop)
                            return;
                        Thread.Sleep(500);
                    }

                    var ag = AgentHelper.GetAgent("clara");
                    if (ag == null || string.IsNullOrEmpty(ag.ConfigurationData))
                        continue;

                    var cfg = JsonConvert.DeserializeObject<ClaraConfigurationData>(ag.ConfigurationData);
                    if (cfg.NewsSources == null || cfg.NewsSources.Count == 0)
                        continue;

                    var users = (from z in cfg.NewsSources
                                 where z.UserId != null
                                 select z.UserId.ToLowerInvariant()).Distinct().ToList();

                    foreach (var usr in users)
                    {
                        List<InformationItemsBucket> buckets = null;
                        using (var cli = new MainApiAgentWebClient("clara"))
                        {
                            string url = $"/v1.0/pim/informations/{usr}/buckets";
                            buckets = cli.DownloadData<List<InformationItemsBucket>>(url);
                        }

                        foreach (var r in cfg.NewsSources)
                        {
                            if (!usr.Equals(r.UserId, StringComparison.InvariantCultureIgnoreCase))
                                continue;

                            List<InformationItem> items = null;

                            if (r.Source == null)
                                continue;
                            switch (r.Source.ToLowerInvariant())
                            {
                                case "youtube":
                                    items = NewsItems.YoutubeHelper.GetNewItems(r.SourceUri, r.LastChecked);
                                    break;
                                case "rss":
                                    items = NewsItems.YoutubeHelper.GetNewItems(r.SourceUri, r.LastChecked);
                                    break;
                            }

                            if (items != null && items.Count > 0)
                            {
                                foreach (var it in items)
                                {
                                    if (!string.IsNullOrEmpty(r.BuckedId))
                                        it.BucketId = r.BuckedId;
                                    else
                                        it.BucketId = Classify(it, buckets);

                                    it.Source = r.Source;
                                    it.Status = InformationItemStatus.New;
                                }

                                UploadItems(usr, items);
                                var dt = (from z in items select z.Date).Max();
                                r.LastChecked = dt;
                            }
                        }

                        AgentHelper.UpdateConfig("clara", cfg);
                        AgentHelper.UpdateStatusWithInfo("clara", "Get Newsitems OK");
                        Console.WriteLine("Get Newsitems OK");
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine("Get Newsitems failed : " + ex.Message);
                }
            }

        }

        private static string Classify(InformationItem it, List<InformationItemsBucket> buckets)
        {
            return buckets.FirstOrDefault()?.Id;
        }

        private static void UploadItems(string user, List<InformationItem> items)
        {

            for (int i = 0; i < 3; i++)
            {
                try
                {
                    using (var cli = new MainApiAgentWebClient("clara"))
                    {
                        bool t = cli.UploadData<bool, List<InformationItem>>($"v1.0/pim/informations/{user}/items", "POST", items);
                        Console.Write(JsonConvert.SerializeObject(t));
                        break;
                    }
                }
                catch (WebException ex)
                {
                    Thread.Sleep(1000);
                }
            }
        }
    }
}