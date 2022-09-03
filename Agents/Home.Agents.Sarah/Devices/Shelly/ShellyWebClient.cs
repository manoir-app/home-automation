using Home.Graph.Common;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Home.Agents.Sarah.Devices.Shelly
{
    internal class ShellyWebClient : WebClient
    {
        public ShellyWebClient()
        {
        }

        protected override WebRequest GetWebRequest(Uri address)
        {
            var rq = base.GetWebRequest(address);

            string passwordToGet = LocalDebugHelper.GetApiKey(); 
            
            if (!string.IsNullOrEmpty(passwordToGet))
            {
                rq.Credentials = new NetworkCredential("sarah", passwordToGet);
                //string u = $"sarah:{passwordToGet}";
                //u = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(u));
                //rq.Headers.Add(HttpRequestHeader.Authorization, u);
                //rq.Headers.Add(HttpRequestHeader.UserAgent, "maNoir Home Automation - Sarah");
            }
            return rq;
        }

    }
}
