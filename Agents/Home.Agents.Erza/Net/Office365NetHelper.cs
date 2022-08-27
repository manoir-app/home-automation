using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Home.Agents.Erza.Net
{
    internal class Office365NetHelper : BaseNetHelper
    {
        public override bool SetTxtValue(string domain, string subdomain, string txtValue)
        {
            var t = GetToken();
            if (t != null)
                Console.WriteLine("Office 365 : token found");
            else
            {
                Console.WriteLine("Office 365 : no token");
                return false;
            }

            if (!ConvertToClassicDnsDomainNames(ref domain, ref subdomain))
                return false;

            string txt = null;
            using (var cli = new WebClient())
            {
                cli.Headers.Set("Authorization", "Bearer " + t);
                try
                {
                    txt = cli.DownloadString("https://graph.microsoft.com/v1.0/domains/" + domain);
                }
                catch(WebException ex) when ((ex.Response as HttpWebResponse).StatusCode == HttpStatusCode.NotFound)
                {
                    return false;
                }
            }



            return true;
        }

    }
}
