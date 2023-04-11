using Home.Common;
using Home.Journal.Common.Model;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Home.Agents.Aurore
{
    internal class JournalHelper
    {
        public class SectionDataForUpdate
        {
            public Page Page { get; set; }
            public PageSection Section { get; set; }
        }

        public static PageSection UploadProperties(PageSection sc)
        {
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    using (var cli = new MainApiAgentWebClient("aurore"))
                    {
                        var pg = cli.UploadData<PageSection, Dictionary<string, string>>($"/v1.0/journal/sections/{sc.Id}/properties", "POST", sc.Properties);
                        return pg;
                    }
                }
                catch (WebException ex) when (ex.Response is HttpWebResponse)
                {
                    var resp = ex.Response as HttpWebResponse;
                    if (resp.StatusCode == HttpStatusCode.NotFound)
                        return null;
                }
                catch (Exception e)
                {
                    Console.WriteLine("Err updating a page section (" + sc.Id + ") : " + e);
                }
            }

            return null;
        }

        public static List<SectionDataForUpdate> FindPageSection(string source)
        {
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    using (var cli = new MainApiAgentWebClient("aurore"))
                    {
                        var pg = cli.DownloadData<List<SectionDataForUpdate>>($"/v1.0/journal/find/bysource?source={HttpUtility.UrlEncode(source)}");
                        return pg;
                    }
                }
                catch (WebException ex) when (ex.Response is HttpWebResponse)
                {
                    var resp = ex.Response as HttpWebResponse;
                    if (resp.StatusCode == HttpStatusCode.NotFound)
                        return null;
                }
                catch (Exception e)
                {
                    Console.WriteLine("Err gettings journal items : " + e);
                }
            }

            return null;
        }

        public static List<SectionDataForUpdate> UpdateProperties(string source, Func<SectionDataForUpdate, bool> update)
        {
            var ret = new List<SectionDataForUpdate>();

            var l = FindPageSection(source);
            foreach(var r in l)
            {
                try
                {
                    if(update(r))
                    {
                        ret.Add(r);
                    }
                }
                catch(NotImplementedException ex)
                {

                }
            }

            foreach (var r in ret)
                UploadProperties(r.Section);

            return ret;

        }


    }
}
