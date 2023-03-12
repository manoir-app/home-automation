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

                JournalHelper.Update("https://manoir.app/agents/aurore#greetings", (secUpd) =>
                {
                    var t = RefreshJournalPage(new PageToUpdate(secUpd.Page, secUpd.Section), users);
                    if (t != null)
                    {
                        secUpd.Section = t;
                        return true;
                    }
                    return false;
                });
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error in updating Journal greetings : " + ex);
            }
        }

        private static PageSection RefreshJournalPage(PageToUpdate pg, List<Common.Model.User> users)
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
                PageSection sc = pg.Section;
                if(sc!=null)
                {
                    string olddata = sc.Data;
                    sc.Data = "";
                    resp.ConvertTo("md");
                    foreach(var r in resp.Items)
                    {
                        if (sc.Data.Length > 0) sc.Data += "\r\n\r\n";
                        if (r.ContentKind == GreetingsMessageResponseItem.GreetingsMessageResponseItemKind.HeaderContent)
                            sc.Data += "# ";
                        sc.Data += r.Content;
                    }

                    if (sc.Data.Equals(olddata))
                        return null;

                    sc.Source = "https://manoir.app/agents/aurore#greetings";
                }
                return sc;
            }

            return null;
        }

       

        private class PageToUpdate
        {
            public string User { get; set; }
            public List<string> AllUsers { get; set; }

            public Page Page { get; set; }
            public PageSection Section { get; set; }

            public PageToUpdate(Page p, PageSection sec)
            {
                Page = p;
                Section = sec;
                User = p.IsPublic ? "#mesh#" : (p.UserIds.FirstOrDefault("#mesh#"));
                AllUsers = new List<string>();
                if (p.IsPublic)
                    AllUsers.Add("#mesh#");
                else
                    AllUsers.AddRange(p.UserIds);
            }
        }



        

       
    }
}
