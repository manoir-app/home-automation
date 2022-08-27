using Home.Common;
using Home.Common.Model;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Xml;
using System.Text.RegularExpressions;

namespace Home.Agents.Clara
{
    public static partial class DownloadSourceChecker
    {

        public static void Start()
        {
            var t = new Thread(() => DownloadSourceChecker.Run());
            t.Name = "Get downloads";
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
                    for (int i = 0; i < 1200; i++)
                    {
                        if (_stop)
                            return;
                        Thread.Sleep(500);
                    }

                    ClassifyNewFromServer();

                    DoTorrents();

                    Console.WriteLine("Get Torrents OK");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Get torrent failed : " + ex.Message);
                }
            }

        }
        private static void DoTorrents()
        {
            var ag = AgentHelper.GetAgent("clara");
            if (ag == null || string.IsNullOrEmpty(ag.ConfigurationData))
                return;

            var cfg = JsonConvert.DeserializeObject<ClaraConfigurationData>(ag.ConfigurationData);
            if (cfg.TorrentSources == null || cfg.TorrentSources.Count == 0)
                return;

            foreach (var r in cfg.TorrentSources)
            {
                var items = GetDownloads(r.Url);
                if (items.Count > 0)
                {
                    foreach (var it in items)
                        ClassifyNewItem(it, r, items);
                }
                UploadItems(items);
            }

            AgentHelper.UpdateConfig("clara", cfg);
        }

        private static Regex regExEpisode = new Regex(@"(?<episode>S\d{1,3}E\d{1,3})", RegexOptions.IgnoreCase);



        public static void ClassifyNewItem(DownloadItem it, RssTorrentSource r, List<DownloadItem> allItems)
        {
            if (it == null)
                return;

            if (r != null)
                it.IsPrivate = r.IsPrivate;

            string title = it.Title;
            Match mEpisode = regExEpisode.Match(it.Title);
            if (mEpisode != null && mEpisode.Success)
            {
                it.Tags.Add("serie");
                title = title.Replace(mEpisode.Value, "");

            }
            string[] parts = title.Split(new char[] { ' ', '.', ',', '-', '_', '[', '(', ')', ']' },
                StringSplitOptions.RemoveEmptyEntries);

            try
            {
                UpdateMetaData(it, parts);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erreur récupération des méta : " + ex.ToString());
            }

            if (mEpisode != null && mEpisode.Success)
                it.VideoMetadata.EpisodeId = mEpisode.Value;

            if (r != null && r.Tags != null && r.Tags.Length > 0)
                it.Tags.AddRange(r.Tags);
            it.Tags = it.Tags.Distinct().ToList();

            if (IsAlreadyAvailable(it))
            {
                it.Status = DownloadItemStatus.DontDownload;
                return;
            }
            else
            {
                // reste à calculer la pertinence
                ScoreDownload(it);
            }
        }
        private static bool ScoreDownload(DownloadItem it)
        {
            // pour l'instant, on score un peu super beaucoup "en dur" :)

            if (it.VideoMetadata == null)
                return false;

            decimal should = 1M;

            if (it.VideoMetadata.GenreCodes != null)
            {
                foreach (var g in it.VideoMetadata.GenreCodes)
                {
                    switch (g.ToLowerInvariant())
                    {
                        case "crime":
                        case "thriller":
                            should *= 1.05M;
                            break;
                        case "comédie":
                        case "comedie":
                        case "aventure":
                        case "familial":
                        case "animation":
                            should *= 1.15M;
                            break;
                        case "fantastique":
                        case "science-fiction":
                        case "science-fiction & fantastique":
                        case "mystère":
                            should *= 1.35M;
                            break;
                    }
                }
            }

            if (it.Language != null && !it.Language.Equals("fr-fr", StringComparison.InvariantCulture))
                should *= .25M;

            if(!string.IsNullOrEmpty(it.LinkedDownloadId))
                should *= .05M;

            var pMC = (from z in it.Pertinences
                       where z.UserId == "mcarbenay"
                       select z).FirstOrDefault();
            if (pMC == null)
            {
                it.Pertinences.Add(new DownloadPertinence()
                {
                    UserId = "mcarbenay",
                    Pertinence = should
                });
                return true;
            }
            else if(pMC.Pertinence!=should)
            {
                pMC.Pertinence = should;
                return true;
            }

            return false;
        }

        private static void UpdateMetaData(DownloadItem it, string[] parts)
        {
            var meta = VideoAndDownloadsHelper.GetMeta(parts, it.IsPrivate.GetValueOrDefault());

            if (meta == null)
            {
                meta = new VideoMetadata()
                {
                    LocalTitle = it.Title
                };
            }
            it.VideoMetadata = meta;

            ParseLineForMoreData(it, parts, meta);
        }

        public static void ParseForMoreData(DownloadItem it, VideoMetadata meta)
        {
            string[] parts = it.Title.Split(new char[] { ' ', '.', ',', '-', '_', '[', '(', ')', ']' },
                    StringSplitOptions.RemoveEmptyEntries);
            ParseLineForMoreData(it, parts, meta);
        }

        private static void ParseLineForMoreData(DownloadItem it, string[] parts, VideoMetadata meta)
        {
            foreach (var p in parts)
            {
                // ici il faudra mettre la partie ML, en attendant
                // un bon vieux switch :)
                int i = 0;
                string plow = p.ToLowerInvariant();
                if (int.TryParse(p, out i))
                {
                    if (i > 1900 && i < 2200)
                        continue;
                }
                else if (plow.EndsWith('p') && int.TryParse(p.Substring(0, p.Length - 1), out i))
                {
                    // résolution type 720p
                    meta.Resolution = p;
                    continue;
                }
                else if (plow.EndsWith('x') && int.TryParse(p.Substring(1), out i))
                {
                    // x264
                    continue;
                }
                else if (plow.EndsWith('h') && int.TryParse(p.Substring(1), out i))
                {
                    // h264, h265, etc.
                    continue;
                }
                else
                {
                    switch (plow)
                    {
                        case "fullhd":
                            meta.Resolution = "1080p";
                            break;
                        case "4k":
                            meta.Resolution = "4k";
                            break;
                        case "xvid":
                        case "hdlight":
                        case "dvdrip":
                        case "hdtv":
                        case "bdrip":
                        case "webrip":
                            break;
                        case "bluray":
                        case "repack":
                        case "subforced":
                            break;
                        case "fr":
                        case "french":
                        case "truefrench":
                            it.Language = "fr-FR";
                            meta.Language = "fr-FR";
                            break;
                        case "english":
                            it.Language = "en-US";
                            meta.Language = "en-US";
                            break;
                        case "german":
                            it.Language = "de-DE";
                            meta.Language = "de-DE";
                            break;
                        case "vostfr":
                        case "vo":
                            it.Language = "*-*";
                            meta.Language = "*-*";
                            break;
                        case "saison":
                            break;
                    }
                }
            }
        }

        private static void UploadItems(List<DownloadItem> items)
        {

            for (int i = 0; i < 3; i++)
            {
                try
                {
                    using (var cli = new MainApiAgentWebClient("clara"))
                    {
                        bool t = cli.UploadData<bool, List<DownloadItem>>($"v1.0/downloads/", "POST", items);
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


        public static List<DownloadItem> GetDownloads(string channelUrl)
        {
            List<DownloadItem> ret = new List<DownloadItem>();
            string content;

            try
            {
                using (var cli = new WebClient())
                {
                    content = cli.DownloadString(channelUrl);
                }
            }
            catch
            {
                content = "<rss version='2.0'><channel /></rss>";
            }

            var doc = new XmlDocument();
            doc.LoadXml(content);

            ParseV2_0(channelUrl, ret, doc);

            return ret;
        }

        private static Regex rMagnet = new Regex("[\\'\\\"](?<link>magnet\\:.*nce)[\\'\\\"]");

        private static void ParseV2_0(string url, List<DownloadItem> ret, XmlDocument doc)
        {
            var mgr = new XmlNamespaceManager(doc.NameTable);
            mgr.AddNamespace("atom", "http://www.w3.org/2005/Atom");
            mgr.AddNamespace("media", "http://search.yahoo.com/mrss/");

            foreach (XmlElement elm in doc.SelectNodes("/rss/channel/item", mgr))
            {
                DownloadItem ite = new DownloadItem();

                var elmTmp = elm.SelectSingleNode("title") as XmlElement;
                ite.Title = elmTmp?.InnerText;
                elmTmp = elm.SelectSingleNode("link") as XmlElement;
                string pageUrl = elmTmp?.InnerText;
                ite.SourceAgent = "clara";
                ite.Source = url;
                ite.Status = DownloadItemStatus.New;
                if (!string.IsNullOrEmpty(pageUrl))
                {
                    using (var cli = new WebClient())
                    {
                        var html = cli.DownloadString(pageUrl);
                        var matches = rMagnet.Matches(html);
                        if (matches != null && matches.Count > 0)
                        {
                            var match = matches[0];
                            var gp = match.Groups["link"];
                            if (gp != null)
                            {
                                ite.SourceUrl = gp.Value;
                                ret.Add(ite);
                            }
                        }
                    }
                }

            }
        }

    }
}