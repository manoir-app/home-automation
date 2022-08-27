using Certes;
using Certes.Acme;
using Home.Common;
using Home.Common.Messages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Home.Agents.Erza
{
    partial class NetworkChecker
    {
        public static DateTimeOffset dwNextTryCertificate = DateTimeOffset.Now.AddMinutes(-5);

        internal static void RunPublicServerCheck()
        {
            var cert = ServicePointManager.ServerCertificateValidationCallback;

            UpdateNextTryCertificateFromFiles();

            string urlSite = HomeServerHelper.GetPublicGraphUrl();
            var uri = new Uri(urlSite, UriKind.Absolute);

            string publicHost = "vps-8cb2daba.vps.ovh.net";
            string publicIp = "";
            try
            {
                var dnsI = Dns.GetHostEntry(publicHost);
                publicIp = (from z in dnsI.AddressList
                            where z.AddressFamily == AddressFamily.InterNetwork
                            select z).FirstOrDefault()?.ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{publicHost} does not seem to have a IPv4 address");
                return;
            }


            if (string.IsNullOrEmpty(publicIp))
            {
                Console.WriteLine($"{publicHost} does not seem to have a IPv4 address");
                return;
            }

            // on commence par checker si le dns est valide
            try
            {
                var dnsI = Dns.GetHostEntry(uri.Host);
                if (dnsI == null)
                {
                    Console.WriteLine($"{uri.Host} does not exists, creating DNS entry");
                    CreateDnsA(uri.Host, publicIp);
                    return;
                }
                else
                {

                }
            }
            catch (SocketException ex) when (ex.SocketErrorCode == SocketError.HostNotFound)
            {
                Console.WriteLine($"{uri.Host} does not exists, creating DNS entry");
                CreateDnsA(uri.Host, publicIp);
                return;
            }


            // puis si le fichier frp est à jour

            CheckFrpForPublicHost(uri.Host, Dns.GetHostEntry(publicHost));

            string rootFolder = "/home-automation/";
#if DEBUG
            rootFolder = @"c:\temp\home-automation\";
#endif


            DateTime dtExpiration = DateTime.Today.AddDays(60);

            try
            {
                ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, policyError) =>
                {
                    if (certificate != null)
                    {
                        var tmp = certificate.Subject;

                        // c'est très très moche pour l'instant
                        if (!tmp.EndsWith("manoir.app"))
                            return true;

                        if (tmp.StartsWith("*."))
                            tmp = tmp.Substring(2);

                        if (!uri.Host.EndsWith(tmp, StringComparison.InvariantCultureIgnoreCase))
                            return false;

                        var tmpDt = certificate.GetExpirationDateString();
                        if (!DateTime.TryParse(tmpDt, out dtExpiration))
                        {
                            dtExpiration = DateTime.Today.AddDays(60);
                        }
                    }

                    

                    return true;
                };

                string realUrl = urlSite;
                if (!realUrl.EndsWith("/"))
                    realUrl += "/";
                realUrl += "v1.0/system/mesh/local/graph/check";

                HttpWebRequest rq = HttpWebRequest.CreateHttp(realUrl) as HttpWebRequest;
                rq.Method = "GET";
                rq.Timeout = 30000;
                Console.WriteLine($"call to {urlSite} started");

                HttpWebResponse resp = rq.GetResponse() as HttpWebResponse;
                Console.WriteLine($"call to {urlSite} : expiration date is : {dtExpiration.ToString("dd/MM/yyyy")}");

                if (dtExpiration < DateTime.Today.AddDays(5))
                {
                    CertificateRequest(uri, rootFolder, publicHost);
                }
            }
            catch (WebException ex)
            {
                Console.WriteLine($"call to {urlSite} failed : {ex.Message}");
                if (ex.Status == WebExceptionStatus.SecureChannelFailure)
                {
                    CertificateRequest(uri, rootFolder, publicHost);
                }
                else if (ex.InnerException != null
                    && ex.InnerException is System.Net.Http.HttpRequestException
                    && (ex.InnerException as System.Net.Http.HttpRequestException).HResult == -2146232800)
                { // handshake failed
                    CertificateRequest(uri, rootFolder, publicHost);
                }
                else if (ex.InnerException != null && ex.InnerException.InnerException != null
                    && ex.InnerException.InnerException is SocketException
                    && (ex.InnerException.InnerException as SocketException).SocketErrorCode == SocketError.ConnectionRefused)
                {
                    CertificateRequest(uri, rootFolder, publicHost);
                }


            }
            finally
            {
                ServicePointManager.ServerCertificateValidationCallback = cert;
            }
        }

        private static void CertificateRequest(Uri uri, string rootFolder, string publicHost)
        {
            // si on n'a pas le droit de récupérer un certificat
            // tout de suite (rate limiting ou en cours de finition)
            // on laisse tomber
            if (dwNextTryCertificate > DateTimeOffset.Now)
                return;
            try
            {
                MakeNewCertificateAsync(Path.Combine(rootFolder, "frps/"), "generic", uri.Host);
            }
            catch (Exception innerEx)
            {
                if (innerEx.Message.Contains("urn:ietf:params:acme:error:rateLimited"))
                    dwNextTryCertificate = DateTimeOffset.Now.AddDays(5);
                Console.WriteLine("Failed to create certificate : " + innerEx.ToString());
            }

            CheckFrpForPublicHost(uri.Host, Dns.GetHostEntry(publicHost));
            NotifyChangeInFrpConfig();
        }

        private static void NotifyChangeInFrpConfig()
        {
            // il faudra demander à gaia de mettre à jour
            // le secret et de relancer frpc

            var file = "/home-automation/frps/frpc.ini";
            if (!File.Exists(file))
                return;

            file = File.ReadAllText(file);
            NatsMessageThread.Push(new SystemReverseProxyChangeMessage()
            {
                FrpConfigFile = file
            });
        }

        private static void UpdateNextTryCertificateFromFiles()
        {
            var file = "/home-automation/frps/generic.crt";

            // si on est déjà pas supposé faire le test
            // on ne change rien
            if (dwNextTryCertificate > DateTimeOffset.Now)
                return;

            // sinon, on regarde si il y a moins de 10 minutes
            // que le certificat est créé, si oui, on attends
            // encore un peu (total d'environ 20 minutes / date création)
            if (File.Exists(file))
            {
                var t = new FileInfo(file);
                if (Math.Abs((DateTime.UtcNow - t.LastWriteTimeUtc).TotalMinutes) < 10)
                    dwNextTryCertificate = DateTimeOffset.Now.AddMinutes(10);
            }
        }

        private static void CheckFrpForPublicHost(string host, IPHostEntry publicHost)
        {
            var file = "/home-automation/frps/frpc.ini";
#if DEBUG
            file = @"c:\temp\home-automation\frps\frpc.ini";
#endif

            var folder = Path.GetDirectoryName(file);
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            var ipv4 = (from z in publicHost.AddressList
                        where z.AddressFamily == AddressFamily.InterNetwork
                        select z).FirstOrDefault();

            List<string> strs = new List<string>();
            List<string> outstrs = new List<string>();
            if (File.Exists(file))
            {
                bool isInNonCopyBloc = false;
                strs.AddRange(File.ReadAllLines(file));
                foreach (string str in strs)
                {
                    var trimmed = str.Trim();
                    if (trimmed.StartsWith("["))
                    {
                        switch (trimmed.ToLowerInvariant())
                        {
                            case "[dev-alexandra]":
                            case "[dev-public-api]":
                            case "[alexandra]":
                            case "[public-api]":
                                isInNonCopyBloc = true;
                                break;
                            default:
                                isInNonCopyBloc = false;
                                break;
                        }
                    }

                    if (!isInNonCopyBloc)
                        outstrs.Add(str);
                }
                string certiffile = Path.Combine(folder, "generic.crt");

                if (host.Contains("dev"))
                    outstrs.Add("[dev-public-api]");
                else
                    outstrs.Add("[public-api]");
                if (File.Exists(certiffile))
                {
                    outstrs.Add("type=https");
                    outstrs.Add($"custom_domains={host}");
                    outstrs.Add("plugin=https2http");
                    outstrs.Add("plugin_local_addr=public-api:80");
                    outstrs.Add("plugin_crt_path=/home-automation/frps/generic.pem.crt");
                    outstrs.Add("plugin_key_path=/home-automation/frps/generic.pem.key");
                    outstrs.Add("plugin_host_header_rewrite = 127.0.0.1");
                    outstrs.Add("plugin_header_X-From-Where = frp");
                    outstrs.Add("proxy_protocol_version = v2");
                }
                else
                {
                    outstrs.Add("type=http");
                    outstrs.Add("local_ip=public-api");
                    outstrs.Add("local_port=80");
                    outstrs.Add($"custom_domain={host}");
                    outstrs.Add("proxy_protocol_version = v2");
                }

                File.WriteAllLines(file, outstrs);
            }

        }

        public static void MakeNewCertificateAsync(string outputFolder, string filename, string domainName)
        {
            if (domainName.StartsWith("public."))
                domainName = "*" + domainName.Substring(6);

            if (!Directory.Exists(outputFolder))
                Directory.CreateDirectory(outputFolder);

            string pemAccountFile = Path.Combine(outputFolder, "acmeaccount.pem");

            IAccountContext account = null;
            AcmeContext acme = null;
            var uri = WellKnownServers.LetsEncryptV2;
#if DEBUG
            uri = WellKnownServers.LetsEncryptStagingV2;
#endif

            if (!File.Exists(pemAccountFile))
            {
                Console.WriteLine($"creating new account for Let's Encrypt");
                acme = new AcmeContext(uri);
                account = acme.NewAccount("michael@manoir.app", true).Result;
                File.WriteAllText(pemAccountFile, acme.AccountKey.ToPem());
                Console.WriteLine($"created new account for Let's Encrypt");
            }
            else
            {
                string pemKey = File.ReadAllText(pemAccountFile);
                var accountKey = KeyFactory.FromPem(pemKey);
                acme = new AcmeContext(uri, accountKey);
                Console.WriteLine($"loading existing account for Let's Encrypt");
                account = acme.Account().Result;
                Console.WriteLine($"loaded existing account for Let's Encrypt");
            }


            try
            {
                var dnsApi = Dns.GetHostEntry(uri.Host);
                Console.WriteLine($"Let's Encrypt API : {dnsApi.AddressList.FirstOrDefault()}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("LetsEncrypt API : " + ex);
            }

            Console.WriteLine($"creating Let's Encrypt order for {domainName} ");
            var order = acme.NewOrder(new string[] { domainName }).Result;
            Console.WriteLine($"getting authorizations for {domainName} ");
            var auth = (order.Authorizations()).Result.First();
            Console.WriteLine($"getting Let's Encrypt DNS challenge for {domainName} ");
            var dns = auth.Dns().Result;
            Console.WriteLine($"Let's Encrypt DNS challenge for {domainName} is {dns.Token} ");
            var txt = acme.AccountKey.DnsTxt(dns.Token);

            if (!CreateDnsTxt(domainName, txt))
                return;

            Thread.Sleep(30000);

            bool done = false;
            for (int i = 0; i < 30; i++)
            {
                try
                {
                    var check = dns.Validate().Result;
                    switch (check.Status.GetValueOrDefault(Certes.Acme.Resource.ChallengeStatus.Invalid))
                    {
                        case Certes.Acme.Resource.ChallengeStatus.Valid:
                            DownloadCertificate(order, outputFolder, filename, domainName);
                            done = true;
                            break;
                        case Certes.Acme.Resource.ChallengeStatus.Invalid:
                            done = true;
                            break;
                        default:
                            Thread.Sleep((dns.RetryAfter == 0 ? 45 : dns.RetryAfter) * 1000);
                            break;
                    }
                    if (done)
                        break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    throw;
                }
            }
        }

        private static bool CreateDnsTxt(string domainName, string txt)
        {
            if (domainName.StartsWith("*."))
            {
                domainName = domainName.Substring(2);
            }

            Console.WriteLine($"creating challenge TXT record for _acme-challenge.{domainName}");


            // pour l'instant, on teste un par un :)
            //if (!new Net.Office365NetHelper().SetTxtValue(domainName, "_acme-challenge", txt))
            //{
            if (!new Net.OvhHelper().SetTxtValue(domainName, "_acme-challenge", txt))
            {
                if (!new Net.AzureNetHelper().SetTxtValue(domainName, "_acme-challenge", txt))
                    return false;
            }
            //}

            Console.WriteLine($"created challenge TXT record for _acme-challenge.{domainName}");

            return true;
        }

        private static bool CreateDnsCname(string domainName, string serverIp)
        {
            if (domainName.StartsWith("*."))
            {
                return false;
            }

            // pour l'instant, on teste un par un :)
            //if (!new Net.Office365NetHelper().SetTxtValue(domainName, "_acme-challenge", txt))
            //{
            if (!new Net.OvhHelper().SetCnameForPublicSite(domainName, "", serverIp))
            {
                return false;
            }
            //}
            return true;
        }

        private static bool CreateDnsA(string domainName, string serverIp)
        {
            if (domainName.StartsWith("*."))
            {
                return false;
            }

            // pour l'instant, on teste un par un :)
            //if (!new Net.Office365NetHelper().SetTxtValue(domainName, "_acme-challenge", txt))
            //{
            if (!new Net.OvhHelper().SetAForPublicSite(domainName, "", serverIp))
            {
                return false;
            }
            //}
            return true;
        }

        private static void DownloadCertificate(IOrderContext order, string outputFolder, string filename, string domainName)
        {
            filename = Path.GetFileNameWithoutExtension(filename);
            var key = KeyFactory.NewKey(KeyAlgorithm.ES256);
            var cert = order.Generate(new CsrInfo
            {
                CountryName = "FR",
                State = "Nord",
                Locality = "Lille",
                Organization = "maNoir",
                OrganizationUnit = "home-automation",
                CommonName = domainName,
            }, key, "ISRG Root X1").Result;

            File.WriteAllBytes(Path.Combine(outputFolder, filename + ".crt"), cert.Certificate.ToDer());
            File.WriteAllBytes(Path.Combine(outputFolder, filename + ".key"), key.ToDer());
            File.WriteAllText(Path.Combine(outputFolder, filename + ".pem.crt"), cert.Certificate.ToPem());
            File.WriteAllText(Path.Combine(outputFolder, filename + ".pem.key"), key.ToPem());
#if !DEBUG
            File.WriteAllText(Path.Combine(outputFolder, filename + ".pem"), cert.ToPem());
#endif
        }
    }
}
