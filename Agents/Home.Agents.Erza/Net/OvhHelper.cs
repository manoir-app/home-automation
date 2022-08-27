using Home.Common;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace Home.Agents.Erza.Net
{
    internal class OvhHelper : BaseNetHelper
    {
        static OvhHelper()
        {
            AppKey = ConfigurationSettingsHelper.Get(ConfigurationSettingsHelper.OvhAppKey);
            AppSecret = ConfigurationSettingsHelper.Get(ConfigurationSettingsHelper.OvhAppSecret);
            ConsumerKey = ConfigurationSettingsHelper.Get(ConfigurationSettingsHelper.OvhConsumerSecret);
        }

        private static String AppKey;
        private static String AppSecret;
        private static String ConsumerKey;


        public class CreateRecordRequest
        {
            public string fieldType { get; set; }
            public string subDomain { get; set; }
            public string target { get; set; }
            public int ttl { get; set; }
        }

        public class UpdateRecordRequest
        {
            public string target { get; set; }
        }


        public class CreateRecordResponse
        {
            public string fieldType { get; set; }
            public int ttl { get; set; }
            public long id { get; set; }
            public string target { get; set; }
            public string zone { get; set; }
            public string subDomain { get; set; }
        }



        public override bool SetTxtValue(string domain, string subdomain, string txtValue)
        {
            if (!ConvertToClassicDnsDomainNames(ref domain, ref subdomain))
                return false;

            var ovhApi = new OvhApi(AppKey, AppSecret, ConsumerKey);
            try
            {
                var dnsOvh = Dns.GetHostEntry("eu.api.ovh.com");
                Console.WriteLine($"Ovh API : {dnsOvh.AddressList.FirstOrDefault()}");
            }
            catch
            {

            }

            try
            {
                long[] dnsInfo = ovhApi.Get<long[]>($"https://eu.api.ovh.com/1.0/domain/zone/{domain}/record?fieldType=TXT&subDomain={subdomain}");

                if (dnsInfo.Length == 0)
                {
                    Console.WriteLine($"{subdomain}.{domain} : new TXT");
                    var reps = ovhApi.Post<CreateRecordResponse>($"https://eu.api.ovh.com/1.0/domain/zone/{domain}/record",
                       JsonConvert.SerializeObject(new CreateRecordRequest()
                       {
                           target = txtValue,
                           ttl = 360,
                           fieldType = "TXT",
                           subDomain = subdomain
                       }));
                }
                else
                {
                    Console.WriteLine($"{subdomain}.{domain} : updating TXT");
                    ovhApi.Put<string>($"https://eu.api.ovh.com/1.0/domain/zone/{domain}/record/{dnsInfo[0]}",
                        JsonConvert.SerializeObject(new UpdateRecordRequest()
                        {
                            target = txtValue
                        }));
                }

                ovhApi.Post<string>($"https://eu.api.ovh.com/1.0/domain/zone/{domain}/refresh", null);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return false;
            }


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

        private T AppelOvh<T>(string method, string url, string body)
        {
            Int32 unixTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            String signature = "$1$" + HashSHA1(AppSecret + "+" + ConsumerKey + "+" + method.ToUpperInvariant() + "+" + url + "+" + body + "+" + unixTimestamp.ToString("0"));

            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(url);
            req.Method = method;
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
                    var z = JsonConvert.DeserializeObject<T>(result);
                    if (z == null)
                        Console.Error.WriteLine("Impossible de désérialiser le retour d'OVH : " + result);
                    return z;
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

        internal bool SetCnameForPublicSite(string domain, string subdomain, string pointsTo)
        {
            if (!ConvertToClassicDnsDomainNames(ref domain, ref subdomain))
                return false;

            var ovhApi = new OvhApi(AppKey, AppSecret, ConsumerKey);

            if(!pointsTo.EndsWith("."))
                pointsTo += ".";

            try
            {
                long[] dnsInfo = ovhApi.Get<long[]>($"https://eu.api.ovh.com/1.0/domain/zone/{domain}/record?fieldType=CNAME&subDomain={subdomain}");

                
                if (dnsInfo.Length == 0)
                {
                    Console.WriteLine($"{subdomain}.{domain} : new CNAME");
                    var reps = ovhApi.Post<CreateRecordResponse>($"https://eu.api.ovh.com/1.0/domain/zone/{domain}/record",
                       JsonConvert.SerializeObject(new CreateRecordRequest()
                       {
                           target = pointsTo,
                           ttl = 360,
                           fieldType = "CNAME",
                           subDomain = subdomain
                       }));
                }
                else
                {
                    Console.WriteLine($"{subdomain}.{domain} : updating CNAME");
                    ovhApi.Put<string>($"https://eu.api.ovh.com/1.0/domain/zone/{domain}/record/{dnsInfo[0]}",
                        JsonConvert.SerializeObject(new UpdateRecordRequest()
                        {
                            target = pointsTo
                        }));
                }

                ovhApi.Post<string>($"https://eu.api.ovh.com/1.0/domain/zone/{domain}/refresh", null);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return false;
            }

        }


        internal bool SetAForPublicSite(string domain, string subdomain, string pointsTo)
        {
            if (!ConvertToClassicDnsDomainNames(ref domain, ref subdomain))
                return false;

            var ovhApi = new OvhApi(AppKey, AppSecret, ConsumerKey);

            
            try
            {
                long[] dnsInfo = ovhApi.Get<long[]>($"https://eu.api.ovh.com/1.0/domain/zone/{domain}/record?fieldType=A&subDomain={subdomain}");


                if (dnsInfo.Length == 0)
                {
                    Console.WriteLine($"{subdomain}.{domain} : new A");
                    var reps = ovhApi.Post<CreateRecordResponse>($"https://eu.api.ovh.com/1.0/domain/zone/{domain}/record",
                       JsonConvert.SerializeObject(new CreateRecordRequest()
                       {
                           target = pointsTo,
                           ttl = 360,
                           fieldType = "A",
                           subDomain = subdomain
                       }));
                }
                else
                {
                    Console.WriteLine($"{subdomain}.{domain} : updating A");
                    ovhApi.Put<string>($"https://eu.api.ovh.com/1.0/domain/zone/{domain}/record/{dnsInfo[0]}",
                        JsonConvert.SerializeObject(new UpdateRecordRequest()
                        {
                            target = pointsTo
                        }));
                }

                ovhApi.Post<string>($"https://eu.api.ovh.com/1.0/domain/zone/{domain}/refresh", null);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return false;
            }

        }
    }
}
