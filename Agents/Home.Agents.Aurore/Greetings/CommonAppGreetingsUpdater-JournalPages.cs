using Home.Common;
using Home.Common.Messages;
using Home.Common.Model;
using Home.Journal.Common.Model;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Rest;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Bcpg.OpenPgp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Home.Agents.Aurore.Greetings
{
    partial class CommonAppGreetingsUpdater
    {
        private static void UpdateJournal()
        {
            _nextJournalUpdate = DateTime.Now.AddMinutes(30);
            try
            {
                var users = AgentHelper.GetMainUsers("aurore");
                var pgs = GetPagesToUpdate();
                foreach(var pg in pgs)
                {
                    RefreshJournalPage(pg, users);
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error in updating Journal greetings : " + ex);
            }
        }

        private static void RefreshJournalPage(PageToUpdate pg, List<Common.Model.User> users)
        {
            GreetingsMessageResponse resp = null;
            if (pg.User.Equals("#mesh#", StringComparison.InvariantCultureIgnoreCase))
                resp = CommonScreenGreetings.GetNoOneHomeMessage();
            else
            {
                var lst = new List<Common.Model.User>();
                foreach(var r in pg.AllUsers)
                {
                    var tmp = (from z in users where z.Id.Equals(r) select z).FirstOrDefault();
                    lst.Add(tmp);
                }
                resp = CommonScreenGreetings.GetGreetingsForUserList(new GreetingsMessage(GreetingsMessage.SimpleGetGreetings)
                {
                    Destination = GreetingsMessage.GreetingsDestination.Screen,
                    Users = lst
                }) as GreetingsMessageResponse;
            }

            if(resp!=null)
            {
                PageSection sc = FindPageSection(pg.PagePath, pg.User, pg.Order);
                if(sc!=null)
                {
                    sc.Data = "";
                    resp.ConvertTo("md");
                    foreach(var r in resp.Items)
                    {
                        if (sc.Data.Length > 0) sc.Data += "\r\n\r\n";
                        if (r.ContentKind == GreetingsMessageResponseItem.GreetingsMessageResponseItemKind.HeaderContent)
                            sc.Data += "# ";
                        sc.Data += r.Content;
                    }
                    sc.Source = "https://manoir.app/agents/aurore";
                }
                UploadSection(sc);
            }
        }

       

        private class PageToUpdate
        {
            public string PagePath { get; set; }
            public int Order { get; set; }
            public string User { get; set; }
            public List<string> AllUsers { get; set; }

            public PageToUpdate(Page p, string user, int order)
            {
                PagePath = p.Path;
                Order = order; 
                User = p.IsPublic ? "#mesh#" : (p.UserIds.FirstOrDefault("#mesh#"));
                AllUsers = new List<string>();
                if (p.IsPublic)
                    AllUsers.Add("#mesh#");
                else
                    AllUsers.AddRange(p.UserIds);
            }
        }

        private static List<PageToUpdate> GetPagesToUpdate()
        {
            // pour l'instant en dur, pour tester
            var ret = new List<PageToUpdate>();

            Page pg;

            pg = GetPage("/index.html", "#mesh#");
            if (pg != null)
                ret.Add(new PageToUpdate(pg, "#mesh#", 0));

            pg = GetPage("/user/index.html", "mcarbenay");
            if (pg != null)
                ret.Add(new PageToUpdate(pg, "mcarbenay", 0));

            return ret;
        }


        private static PageSection FindPageSection(string path, string user, int order)
        {
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    using (var cli = new MainApiAgentWebClient("aurore"))
                    {
                        var pg = cli.DownloadData<PageSection>($"/v1.0/journal/find/section?pagePath={HttpUtility.UrlEncode(path)}&order={order}&userId={HttpUtility.UrlEncode(user)}");
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
                    Console.WriteLine("Err gettings scheduled items : " + e);
                }
            }

            return null;
        }

        private static PageSection UploadSection(PageSection sc)
        {
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    using (var cli = new MainApiAgentWebClient("aurore"))
                    {
                        var pg = cli.UploadData<PageSection, PageSection>($"/v1.0/journal/pages/{sc.PageId}/sections", "POST", sc);
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
                    Console.WriteLine("Err gettings scheduled items : " + e);
                }
            }

            return null;
        }

        private static Page GetPage(string path, string user)
        {
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    using (var cli = new MainApiAgentWebClient("aurore"))
                    {
                        var pg = cli.DownloadData<Page>($"/v1.0/journal/find/pages?pagePath={HttpUtility.UrlEncode(path)}&userId={HttpUtility.UrlEncode(user)}");
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
                    Console.WriteLine("Err gettings pages : " + e);
                }
            }

            return null;
        }
    }
}
