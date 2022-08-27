using Home.Common;
using Home.Common.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Home.Agents.Clara.NewsItems
{
    static class YoutubeHelper
    {

        public class SearchVideosResponse
        {
            public string kind { get; set; }
            public string etag { get; set; }
            public string nextPageToken { get; set; }
            public string regionCode { get; set; }
            public Pageinfo pageInfo { get; set; }
            public Item[] items { get; set; }
        }

        public class Pageinfo
        {
            public int totalResults { get; set; }
            public int resultsPerPage { get; set; }
        }

        public class Item
        {
            public string kind { get; set; }
            public string etag { get; set; }
            public Id id { get; set; }
            public Snippet snippet { get; set; }
        }

        public class Id
        {
            public string kind { get; set; }
            public string videoId { get; set; }
        }

        public class Snippet
        {
            public DateTime publishedAt { get; set; }
            public string channelId { get; set; }
            public string title { get; set; }
            public string description { get; set; }
            public Thumbnails thumbnails { get; set; }
            public string channelTitle { get; set; }
            public string liveBroadcastContent { get; set; }
            public DateTime publishTime { get; set; }
        }

        public class Thumbnails
        {
            public Default _default { get; set; }
            public Medium medium { get; set; }
            public High high { get; set; }
        }

        public class Default
        {
            public string url { get; set; }
            public int width { get; set; }
            public int height { get; set; }
        }

        public class Medium
        {
            public string url { get; set; }
            public int width { get; set; }
            public int height { get; set; }
        }

        public class High
        {
            public string url { get; set; }
            public int width { get; set; }
            public int height { get; set; }
        }




        public static List<InformationItem> GetNewItems(string channelId, DateTimeOffset since)
        {
            try
            {
                List<InformationItem> ret = new List<InformationItem>();

                string url = "https://youtube.googleapis.com/youtube/v3/search?part=snippet&channelId=";
                url += channelId;
                url += "&publishedAfter=";
                url += (since.ToUniversalTime().ToString("yyyy-MM-dd\\THH:mm:ss") +"Z");
                url += "&order=date&key=";
                url += ConfigurationSettingsHelper.GetYoutubeApiKey();

                using (var cli = new WebClient())
                {
                    cli.Headers.Add(HttpRequestHeader.Accept, "application/json");
                    var tmp = cli.DownloadString(url);
                    var vids = JsonConvert.DeserializeObject<SearchVideosResponse>(tmp);
                    foreach(var vi in vids.items)
                    {
                        InformationItem item = new InformationItem()
                        {
                            Author = vi.snippet.channelTitle,
                            SourceUrl = "https://www.youtube.com/watch?v=" + vi.id.videoId,
                            Title = vi.snippet.title,
                            Content = vi.snippet.description,
                            ContentFormat = InformationItemContentFormat.Html,
                            Date = vi.snippet.publishedAt,
                            Kind = InformationItemKind.NewsItem,
                            Source = "youtube",
                            Status = InformationItemStatus.New
                        };

                        ret.Add(item);
                    }
                }

                return ret;

            }
            catch (Exception ex)
            {
                AgentHelper.UpdateStatusWithInfo("clara", $"Failed in Youtube Get items for channel {channelId} :{ex.Message}");
                return null;
            }
        }
    }
}
