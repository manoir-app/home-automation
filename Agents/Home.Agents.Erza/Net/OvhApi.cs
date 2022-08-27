using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace Home.Agents.Erza.Net
{
    public class OvhApi
    {
        public OvhApi(string appKey, string appSecret, string consumerKey)
        {
            this.AppKey = appKey;
            this.AppSecret = appSecret;
            this.ConsumerKey = consumerKey;
        }

        private static string HashSHA1(string sInputString)
        {
            SHA1 sha = SHA1.Create();
            byte[] bHash = sha.ComputeHash(Encoding.UTF8.GetBytes(sInputString));
            StringBuilder sBuilder = new StringBuilder();

            for (int i = 0; i < bHash.Length; i++)
            {
                sBuilder.Append(bHash[i].ToString("x2"));
            }

            return sBuilder.ToString();
        }

        private String AppKey;
        private String AppSecret;
        private String ConsumerKey;

        public T Post<T>(string url, string body)
        {
            Int32 unixTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc))).TotalSeconds;

            String signature = "$1$" + HashSHA1(AppSecret + "+" + ConsumerKey + "+POST+" + url + "+" + body + "+" + unixTimestamp.ToString("0"));

            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(url);
            req.Method = "POST";
            req.ContentType = "application/json";
            req.Headers.Add("X-Ovh-Application:" + AppKey);
            req.Headers.Add("X-Ovh-Consumer:" + ConsumerKey);
            req.Headers.Add("X-Ovh-Signature:" + signature);
            req.Headers.Add("X-Ovh-Timestamp:" + unixTimestamp.ToString("0"));

            //Ecriture des paramètres body
            using (System.IO.Stream s = req.GetRequestStream())
            {
                using (System.IO.StreamWriter sw = new System.IO.StreamWriter(s))
                    sw.Write(body);
            }

            try
            {
                using (HttpWebResponse myHttpWebResponse = (HttpWebResponse)req.GetResponse())
                using (var st = myHttpWebResponse.GetResponseStream())
                using (var reader = new StreamReader(st))
                {
                    String result = reader.ReadToEnd().Trim();
                    T parsedResult = JsonConvert.DeserializeObject<T>(result);
                    return parsedResult;
                }
            }
            catch (WebException e)
            {
                using (WebResponse response = e.Response)
                using (Stream data = ((HttpWebResponse)response).GetResponseStream())
                using (var reader = new StreamReader(data))
                {
                    throw new ApplicationException(reader.ReadToEnd());
                }
            }
        }

        public T Put<T>(string url, string body)
        {
            Int32 unixTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc))).TotalSeconds;

            String signature = "$1$" + HashSHA1(AppSecret + "+" + ConsumerKey + "+PUT+" + url + "+" + body + "+" + unixTimestamp.ToString("0"));

            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(url);
            req.Method = "PUT";
            req.ContentType = "application/json";
            req.Headers.Add("X-Ovh-Application:" + AppKey);
            req.Headers.Add("X-Ovh-Consumer:" + ConsumerKey);
            req.Headers.Add("X-Ovh-Signature:" + signature);
            req.Headers.Add("X-Ovh-Timestamp:" + unixTimestamp.ToString("0"));

            //Ecriture des paramètres body
            using (System.IO.Stream s = req.GetRequestStream())
            {
                using (System.IO.StreamWriter sw = new System.IO.StreamWriter(s))
                    sw.Write(body);
            }

            try
            {
                using (HttpWebResponse myHttpWebResponse = (HttpWebResponse)req.GetResponse())
                using (var st = myHttpWebResponse.GetResponseStream())
                using (var reader = new StreamReader(st))
                {
                    String result = reader.ReadToEnd().Trim();
                    T parsedResult = JsonConvert.DeserializeObject<T>(result);
                    return parsedResult;
                }
            }
            catch (WebException e)
            {
                using (WebResponse response = e.Response)
                using (Stream data = ((HttpWebResponse)response).GetResponseStream())
                using (var reader = new StreamReader(data))
                {
                    throw new ApplicationException(reader.ReadToEnd());
                }
            }
        }

        public T Delete<T>(string url, string body = null)
        {
            Int32 unixTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;

            String signature = "$1$" + HashSHA1(AppSecret + "+" + ConsumerKey + "+DELETE+" + url + "+" + (string.IsNullOrEmpty(body) ? "" : body) + "+" + unixTimestamp.ToString("0"));

            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(url);
            req.Method = "DELETE";
            req.ContentType = "application/json";
            req.Headers.Add("X-Ovh-Application:" + AppKey);
            req.Headers.Add("X-Ovh-Consumer:" + ConsumerKey);
            req.Headers.Add("X-Ovh-Signature:" + signature);
            req.Headers.Add("X-Ovh-Timestamp:" + unixTimestamp.ToString("0"));

            if (!string.IsNullOrEmpty(body))
            {
                //Ecriture des paramètres body
                using (System.IO.Stream s = req.GetRequestStream())
                {
                    using (System.IO.StreamWriter sw = new System.IO.StreamWriter(s))
                        sw.Write(body);
                }
            }

            try
            {
                using (HttpWebResponse myHttpWebResponse = (HttpWebResponse)req.GetResponse())
                using (var st = myHttpWebResponse.GetResponseStream())
                using (var reader = new StreamReader(st))
                {
                    String result = reader.ReadToEnd().Trim();
                    T parsedResult = JsonConvert.DeserializeObject<T>(result);
                    return parsedResult;
                }
            }
            catch (WebException e)
            {
                using (WebResponse response = e.Response)
                using (Stream data = ((HttpWebResponse)response).GetResponseStream())
                using (var reader = new StreamReader(data))
                {
                    throw new ApplicationException(reader.ReadToEnd());
                }
            }
        }

        private class GetConsumerKeyRequest
        {
            public AccessRule[] accessRules { get; set; }
        }


        private class AccessRule
        {
            public string method { get; set; }
            public string path { get; set; }
        }

        public class ConsumerKeyResult
        {
            public string consumerKey { get; set; }
            public string validationUrl { get; set; }
            public string state { get; set; }
        }

        public ConsumerKeyResult GetConsumerKey()
        {
            Int32 unixTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;

            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create("https://eu.api.ovh.com/1.0/auth/credential");
            req.Method = "POST";
            req.ContentType = "application/json";
            req.Headers.Add("X-Ovh-Application:" + AppKey);
            req.Headers.Add("X-Ovh-Timestamp:" + unixTimestamp.ToString("0"));

            string body = JsonConvert.SerializeObject(new GetConsumerKeyRequest()
            {
                accessRules =
                        new AccessRule[] {
                            new AccessRule() { method = "GET", path ="/*" },
                            new AccessRule() { method = "POST", path ="/*" },
                            new AccessRule() { method = "PUT", path ="/*" },
                            new AccessRule() { method = "DELETE", path ="/*" },
                        }
            });

            //Ecriture des paramètres body
            using (System.IO.Stream s = req.GetRequestStream())
            {
                using (System.IO.StreamWriter sw = new System.IO.StreamWriter(s))
                    sw.Write(body);
            }

            try
            {
                using (HttpWebResponse myHttpWebResponse = (HttpWebResponse)req.GetResponse())
                using (var st = myHttpWebResponse.GetResponseStream())
                using (var reader = new StreamReader(st))
                {
                    String result = reader.ReadToEnd().Trim();
                    ConsumerKeyResult parsedResult = JsonConvert.DeserializeObject<ConsumerKeyResult>(result);
                    return parsedResult;
                }
            }
            catch (WebException e)
            {
                using (WebResponse response = e.Response)
                using (Stream data = ((HttpWebResponse)response).GetResponseStream())
                using (var reader = new StreamReader(data))
                {
                    throw new ApplicationException(reader.ReadToEnd());
                }
            }
        }

        public T Get<T>(string url)
        {
            Int32 unixTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;

            String signature = "$1$" + HashSHA1(AppSecret + "+" + ConsumerKey + "+GET+" + url + "++" + unixTimestamp.ToString("0"));

            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(url);
            req.Method = "GET";
            req.ContentType = "application/json";
            req.Headers.Add("X-Ovh-Application:" + AppKey);
            req.Headers.Add("X-Ovh-Consumer:" + ConsumerKey);
            req.Headers.Add("X-Ovh-Signature:" + signature);
            req.Headers.Add("X-Ovh-Timestamp:" + unixTimestamp.ToString("0"));


            try
            {
                using (HttpWebResponse myHttpWebResponse = (HttpWebResponse)req.GetResponse())
                using (var st = myHttpWebResponse.GetResponseStream())
                using (var reader = new StreamReader(st))
                {
                    String result = reader.ReadToEnd().Trim();
                    T parsedResult = JsonConvert.DeserializeObject<T>(result);
                    return parsedResult;
                }
            }
            catch (WebException e)
            {

                using (WebResponse response = e.Response)
                {
                    if (response != null && response is HttpWebResponse)
                    {
                        using (Stream data = ((HttpWebResponse)response).GetResponseStream())
                        using (var reader = new StreamReader(data))
                        {
                            throw new ApplicationException(reader.ReadToEnd());
                        }
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }


        private class GetAuthTokenRequest : GetConsumerKeyRequest
        {
            public string redirection { get; set; }
        }

        public ConsumerKeyResult GetAuthToken(string redirect)
        {
            string url = "https://eu.api.ovh.com/1.0/auth/credential";

            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(url);

            req.Method = "POST";
            req.Headers.Add("X-OVH-Application:" + AppKey);
            req.ContentType = "application/json";

            string body = JsonConvert.SerializeObject(new GetAuthTokenRequest()
            {
                accessRules =
                        new AccessRule[] {
                            new AccessRule() { method = "GET", path ="/*" },
                            new AccessRule() { method = "POST", path ="/*" },
                            new AccessRule() { method = "PUT", path ="/*" },
                            new AccessRule() { method = "DELETE", path ="/*" },
                        },
                redirection = redirect + "?mode=confirm"
            });

            using (System.IO.Stream s = req.GetRequestStream())
            {
                using (System.IO.StreamWriter sw = new System.IO.StreamWriter(s))
                    sw.Write(body);
            }

            try
            {
                using (HttpWebResponse myHttpWebResponse = (HttpWebResponse)req.GetResponse())
                using (var st = myHttpWebResponse.GetResponseStream())
                using (var reader = new StreamReader(st))
                {
                    String result = reader.ReadToEnd().Trim();
                    ConsumerKeyResult parsedResult = JsonConvert.DeserializeObject<ConsumerKeyResult>(result);
                    return parsedResult;
                }
            }
            catch (WebException e)
            {
                using (WebResponse response = e.Response)
                using (Stream data = ((HttpWebResponse)response).GetResponseStream())
                using (var reader = new StreamReader(data))
                {
                    throw new ApplicationException(reader.ReadToEnd());
                }
            }
        }
    }

}
