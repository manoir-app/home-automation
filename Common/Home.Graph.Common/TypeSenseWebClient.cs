using Home.Graph.Common;
using Newtonsoft.Json;
using SharpCompress.Common.Rar;
using System;
using System.Net;

namespace Home.Common
{
    public class TypeSenseWebClient : WebClient
    {
        public TypeSenseWebClient()
        {
            var srv = Environment.GetEnvironmentVariable("TYPESENSE_SERVICE_HOST");
            var port = Environment.GetEnvironmentVariable("TYPESENSE_SERVICE_PORT");
            this.BaseAddress = "http://" + srv + ":" + port;
            var passwordToGet = LocalDebugHelper.GetApiKey(); 
            this.Headers.Add("X-TYPESENSE-API-KEY", passwordToGet);
        }

        public T DownloadData<T>(string url)
        {
            var s = this.DownloadString(url);
            return JsonConvert.DeserializeObject<T>(s);
        }

        public T UploadData<T, R>(string url, string method, R data)
        {
            var s = this.UploadString(url, method, JsonConvert.SerializeObject(data));
            return JsonConvert.DeserializeObject<T>(s);
        }


        protected override WebRequest GetWebRequest(Uri address)
        {
            var t = base.GetWebRequest(address);
            t.ContentType = "application/json";
            return t;
        }
    }
}
