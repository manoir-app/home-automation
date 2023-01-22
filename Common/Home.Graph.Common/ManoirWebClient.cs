using Home.Graph.Common;
using Newtonsoft.Json;
using SharpCompress.Common.Rar;
using System;
using System.IO;
using System.Net;

namespace Home.Common
{
    public class ManoirWebClient : WebClient
    {
        public ManoirWebClient()
        {
        }

        protected override WebRequest GetWebRequest(Uri address)
        {
            var t = (HttpWebRequest) base.GetWebRequest(address);
            t.AllowAutoRedirect = true;
            return t;
        }

        protected override WebResponse GetWebResponse(WebRequest request)
        {
            try
            {
                var t = base.GetWebResponse(request);
                return t;
            }
            catch (WebException ex)
            {
                if(ex.Response!=null && ex.Response is HttpWebResponse)
                {
                    var resp = (HttpWebResponse)ex.Response;
                    if(resp.StatusCode==HttpStatusCode.Redirect)
                    {
                        if (resp.Headers["Location"]!=null)
                        {
                            var req2 = GetWebRequest(new Uri(resp.Headers["Location"]));
                            var t = base.GetWebResponse(req2);
                            return t;
                        }
                    }
                }

                throw;
            }
        }
    }
}
