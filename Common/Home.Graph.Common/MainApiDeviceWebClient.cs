using System;
using System.Net;
using Home.Common;
using Newtonsoft.Json;

namespace Home.Graph.Common
{
    public class MainApiDeviceWebClient : WebClient
    {
        public MainApiDeviceWebClient(string deviceId)
        {

            string srv = LocalDebugHelper.GetLocalGraphHost();
            this.BaseAddress = "http://" + srv;


            var passwordToGet = LocalDebugHelper.GetApiKey();
            var pwd = deviceId + ":" + passwordToGet;
            pwd = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(pwd));
            this.Headers.Add(HttpRequestHeader.Authorization, "Bearer " + pwd);
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
