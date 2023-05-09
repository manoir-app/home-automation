using Home.Graph.Common;
using Newtonsoft.Json;
using SharpCompress.Common.Rar;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;

namespace Home.Common
{
    public class MainApiAgentWebClient : WebClient
    {
        internal static void ClearTestHandlers()
        {
            _rewrite.Clear();
        }

        static Dictionary<string, Delegate> _rewrite = new Dictionary<string, Delegate>();
        internal static void RegisterTestHandler<T>(string url, Func<T> function)
        {
            _rewrite["GET_" + url] = function;
        }
        internal static void RegisterTestHandler<T, R>(string url, string verb, Func<R, T> function)
        {
            _rewrite[verb.ToUpperInvariant() + "_" + url] = function;
        }

        public MainApiAgentWebClient(string agentName)
        {
            var srv = LocalDebugHelper.GetLocalGraphHost();
            this.BaseAddress = "http://" + srv;


            var passwordToGet = LocalDebugHelper.GetApiKey(); 
            var pwd = agentName + ":" + passwordToGet;
            pwd = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(pwd));
            this.Headers.Add(HttpRequestHeader.Authorization, "Basic " + pwd);
        }

        public T DownloadData<T>(string url)
        {
            if(_rewrite.ContainsKey("GET_" + url))
            {
                Delegate dlg = _rewrite["GET_" + url];
                object ret = dlg.DynamicInvoke();
                return (T) ret;
            }

            var s = this.DownloadString(url);
            return JsonConvert.DeserializeObject<T>(s);
        }

        public T UploadData<T, R>(string url, string method, R data)
        {
            if (_rewrite.ContainsKey(method.ToUpper() + "_" + url))
            {
                Delegate dlg = _rewrite[method.ToUpper() + "_" + url];
                object ret = dlg.DynamicInvoke(data);
                return (T)ret;
            }

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
