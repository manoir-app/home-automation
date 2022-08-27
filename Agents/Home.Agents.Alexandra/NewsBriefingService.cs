using Home.Common;
using Home.Common.Messages;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Home.Agents.Alexandra
{
    public class NewsBriefingService
    {
        // temporaire
        private static string _urlFeed = "https://carbenayhomeautomation.blob.core.windows.net/alexa/news/homebrief.json";

        public static void Start()
        {
            var t = new Thread(() => NewsBriefingService.Do());
            t.Name = "Wan Checker";
            t.Start();
        }

        public static void Stop()
        {
            _stop = true;
        }


        public static bool _stop = false;
        public static DateTimeOffset? _lastUpdate = null;

        internal static void Do()
        {
            while (!_stop)
            {
                Thread.Sleep(250);
                if (_lastUpdate == null)
                    GetLastUpdate();
                if(Math.Abs((DateTimeOffset.Now - _lastUpdate.GetValueOrDefault()).TotalMinutes)>5)
                {
                    GenerateAndUpload();
                    _lastUpdate = DateTimeOffset.Now;
                }
            }
        }

        private static string _lastItems = "";
        private static DateTime _lastUpload = DateTime.Today.AddDays(-2);

        private static void GenerateAndUpload()
        {
            List<NewsItem> items = new List<NewsItem>();

            var msgs = NatsMessageThread.Request<HomeStatusItemsMessageResponse>(HomeStatusItemsMessage.GetHomeStatusItems,
                new HomeStatusItemsMessage()
            {
                UserId = null
            });

            if (msgs == null || msgs.Items==null)
                return;

            var tmp = JsonConvert.SerializeObject(msgs.Items);
            if (_lastUpload >= DateTime.Today && _lastItems.Equals(tmp, StringComparison.InvariantCultureIgnoreCase))
                return;
            _lastItems = tmp;

            StringBuilder blr = new StringBuilder();
            var dt = DateTime.Now;
            blr.Append($"Le {dt.ToString("m")}");

            if (msgs.Items.Count > 0)
            {
                foreach (var item in msgs.Items)
                {
                    blr.Append(", ");
                    blr.Append(item.Message);
                }
                blr.Append(".");
            }
            else
            {
                blr.Append(", aucune news !");
            }

            items.Add(new NewsItem()
            {
                uid = Guid.NewGuid().ToString("N"),
                updateDate = DateTimeOffset.Now.ToUniversalTime(),
                mainText = blr.ToString(),
                titleText = msgs.Items.Count == 0 ? "Aucune news" : $"{msgs.Items.Count} infos",
                redirectionUrl = "http://home.anzin.carbenay.me/"
            });

            if(items.Count>0)
                UploadFile(JsonConvert.SerializeObject(items));
        }

        private static string UploadFile(string content)
        {
            string blobConnectionString = ConfigurationSettingsHelper.GetAzureStorageConnectionString();
            Azure.Storage.Blobs.BlobContainerClient containerClient = new Azure.Storage.Blobs.BlobContainerClient(blobConnectionString, "alexa");
            var cnt = containerClient.CreateIfNotExists(Azure.Storage.Blobs.Models.PublicAccessType.Blob);

            Azure.Storage.Blobs.BlobClient client = new Azure.Storage.Blobs.BlobClient(blobConnectionString, "alexa", "news/homebrief.json");
            using (var stm = new MemoryStream(Encoding.UTF8.GetBytes(content)))
            {
                var blob = client.Upload(stm,
                 new Azure.Storage.Blobs.Models.BlobHttpHeaders { ContentType = "application/json" });
            }

            _lastUpload = DateTime.Now;
            var url = client.Uri.ToString();
            return url;
        }


        public class NewsItem
        {
            public string uid { get; set; }
            public DateTimeOffset updateDate { get; set; }
            public string titleText { get; set; }
            public string mainText { get; set; }
            public string redirectionUrl { get; set; }
        }


        private static void GetLastUpdate()
        {
            try
            {
                using (var cli = new WebClient())
                {
                    string s = cli.DownloadString(_urlFeed);
                    if(!string.IsNullOrEmpty(s))
                    {
                        var obj = JsonConvert.DeserializeObject<NewsItem[]>(s);
                        var max = (from z in obj orderby z.updateDate descending select z).FirstOrDefault();
                        if (max != null)
                            _lastUpdate = max.updateDate;
                        else
                            _lastUpdate = DateTime.Now;
                    }
                }
            }
            catch
            {
                _lastUpdate = DateTimeOffset.Now;
            }
        }
    }
}
