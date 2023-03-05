using Home.Graph.Common;
using Newtonsoft.Json;
using System;
using System.IO;
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
            var sett = new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore
            };

            try
            {
                var s = this.UploadString(url, method, JsonConvert.SerializeObject(data, sett));
                return JsonConvert.DeserializeObject<T>(s);
            }
            catch (WebException ex) when (ex.Response is HttpWebResponse
                            && (ex.Response as HttpWebResponse).StatusCode == HttpStatusCode.BadRequest)

            {
                var errEx = (ex.Response as HttpWebResponse);
                using (var rdr = new StreamReader(errEx.GetResponseStream()))
                {
                    string err = rdr.ReadToEnd();
                    var toThrow = TypeSenseException.FromJson(err, ex);
                    toThrow.Data.Add("sent", data);
                    throw toThrow;
                }
                throw;
            }
            catch (WebException ex)
            {
                throw;
            }
        }


        protected override WebRequest GetWebRequest(Uri address)
        {
            var t = base.GetWebRequest(address);
            t.ContentType = "application/json";
            return t;
        }
    }



    [Serializable]
    public class TypeSenseException : ApplicationException
    {

        public class TypeSenseError
        {
            public string message { get; set; }
        }


        public TypeSenseException() { }
        public TypeSenseException(string message) : base(message) { }
        public TypeSenseException(string message, Exception inner) : base(message, inner) { }
        protected TypeSenseException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }

        public static TypeSenseException FromJson(string json)
        {
            return TypeSenseException.FromJson(json, null);
        }

        public static TypeSenseException FromJson(string json, Exception innerException)
        {
            var err = JsonConvert.DeserializeObject<TypeSenseError>(json);
            return new TypeSenseException(err?.message, innerException);
        }
    }
}
