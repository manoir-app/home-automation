using Newtonsoft.Json;
using System;
using System.Net;

namespace Home.Common
{
    public class MainApiAgentWebClient : WebClient
    {
        public MainApiAgentWebClient(string agentName)
        {
            var tmp = HomeServerHelper.GetLocalIP();
            this.BaseAddress = "http://" + tmp;


            var passwordToGet = Environment.GetEnvironmentVariable("HOMEAUTOMATION_APIKEY");
            var pwd = agentName + ":" + passwordToGet;
            pwd = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(pwd));
            this.Headers.Add(HttpRequestHeader.Authorization, "Basic " + pwd);
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
