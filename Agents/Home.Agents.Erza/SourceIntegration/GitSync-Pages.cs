using Home.Common;
using Home.Common.Messages;
using Home.Common.Model;
using Home.Journal.Common.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

namespace Home.Agents.Erza.SourceIntegration
{
    partial class GitSync
    {
        public static void SyncPagesFromServer()
        {
            // à appeler de façon périodique pour s'assurer que les
            // sources sont à jour
            var git = GetGit();
            Dictionary<string, SyncFile> sync = GetAllPagesFromBoth(git);

            foreach (var grp in sync.Values)
            {
                if (grp.ServerContent == null)
                    grp.ServerContent = "";
                if (grp.GitContent == null)
                    grp.GitContent = "";

                if (string.IsNullOrEmpty(grp.ServerContent)
                    && !string.IsNullOrEmpty(grp.GitFile?.Path))
                {
                    Console.WriteLine("Deleting in Git : " + grp.GitFile.Path);
                    git.DeleteFile(grp.GitFile?.Path, grp.GitFile.ServerSignature);
                }
                else if (!grp.ServerContent.Equals(grp.GitContent))
                {
                    if (grp.GitFile != null && !string.IsNullOrEmpty(grp.GitFile.Path))
                    {
                        Console.WriteLine("Updating in Git : " + grp.GitFile.Path);

                        git.UpsertFile(grp.GitFile.Path, grp.ServerContent, grp.GitFile.ServerSignature);
                    }
                    else
                    {
                        string path = Path.ChangeExtension($"pages/{grp.PathFromData}", ".md");
                        path = path.Replace("//", "/");
                        Console.WriteLine("Creating in Git : " + path);
                        git.UpsertFile(path, grp.ServerContent, "");
                    }
                }
            }
        }

        private static Dictionary<string, SyncFile> GetAllPagesFromBoth()
        {
            IGitRepo git = GetGit();
            return GetAllPagesFromBoth(git);
        }

        private static Dictionary<string, SyncFile> GetAllPagesFromBoth(IGitRepo git)
        {
            Dictionary<string, SyncFile> sync = new Dictionary<string, SyncFile>();
            GetPagesFilesFromGit(git, sync, "pages/");
            GetPagesFilesFromServer(sync);
            return sync;
        }



        private static void GetPagesFilesFromServer(Dictionary<string, SyncFile> sync)
        {
            var pages = GetAllPages();
            var sections = GetAllSections();
            foreach (var item in pages)
            {
                SyncFile sf = null;
                if (!sync.TryGetValue(item.Id, out sf))
                {
                    if (item.UserIds != null && item.UserIds.Count > 0)
                    {
                        var parts = item.Path.Split(new char[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries);
                        item.Path = parts[0];

                        item.Path += "[";
                        for (int i = 0; i < item.UserIds.Count; i++)
                        {
                            if (i > 0)
                                item.Id += ",";
                            item.Path += item.UserIds[i];
                        }
                        item.Path += "]";

                        for (int i = 1; i < parts.Length; i++)
                        {
                            item.Path += "/";
                            item.Path += parts[i];
                        }
                    }

                    sf = new SyncFile
                    {
                        Id = item.Id,
                        PathFromData = item.Path
                    };
                    sync.Add(item.Id, sf);
                }



                sf.ServerContent = CreatePageContent(item,
                    (from z in sections where z.PageId.Equals(item.Id, StringComparison.InvariantCultureIgnoreCase) select z).ToArray());
            }
        }

        private static string CreatePageContent(Page item, PageSection[] pageSections)
        {
            StringBuilder blr = new StringBuilder();
            blr.AppendLine("---");
            blr.Append(" title:");
            blr.AppendLine(item.Title);
            blr.Append(" id:");
            blr.AppendLine(item.Id);
            if (!string.IsNullOrEmpty(item.PageIcon))
            {
                blr.Append(" pageIcon:");
                blr.AppendLine(item.PageIcon);
            }
            if (!string.IsNullOrEmpty(item.ParentId))
            {
                blr.Append(" parentId:");
                blr.AppendLine(item.ParentId);
            }
            blr.Append(" public:");
            blr.AppendLine(item.IsPublic.ToString());
            if (!string.IsNullOrEmpty(item.ThemeId))
            {
                blr.Append(" theme:");
                blr.AppendLine(item.ThemeId);
            }
            blr.AppendLine("---");
            blr.AppendLine();

            foreach (var section in pageSections)
            {
                if (blr.Length > 0)
                    blr.AppendLine();

                blr.Append("[JournalApp.ContentModule]::");
                blr.AppendLine(section.Kind);
                blr.AppendLine();
                blr.AppendLine(section.Data);
            }

            return blr.ToString();
        }

        private static Page[] GetAllPages()
        {
            for (int tryCount = 0; tryCount < 3; tryCount++)
            {
                try
                {
                    using (var cli = new MainApiAgentWebClient("erza"))
                    {
                        var tmp = cli.DownloadData<Page[]>("v1.0/journal/pages");
                        if (tmp != null)
                            return tmp;
                    }
                }
                catch (WebException ex)
                {
                    Thread.Sleep(1000);
                }
            }
            return null;
        }

        private static PageSection[] GetAllSections()
        {
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    using (var cli = new MainApiAgentWebClient("erza"))
                    {
                        var tmp = cli.DownloadData<PageSection[]>("v1.0/journal/sections/all");
                        if (tmp != null)
                            return tmp;
                    }
                }
                catch (WebException ex)
                {
                    Thread.Sleep(1000);
                }
            }
            return null;
        }




        private static void GetPagesFilesFromGit(IGitRepo git, Dictionary<string, SyncFile> sync, string folder)
        {
            var files = git.GetFiles(folder);
            foreach (var file in files)
            {
                string filename = Path.GetFileName(file.Path);
                var gf = git.GetFile(file.Path);
                SyncFile sf = new SyncFile()
                {
                    GitFile = gf,
                    GitContent = Encoding.UTF8.GetString(gf.Content),
                    PathFromData = file.Path.Substring("pages".Length)
                };
                var id = GetFrontMatterProp("id", gf.Content);
                if (id != null)
                {
                    sf.Id = id;
                    sync.Add(id, sf);
                }
            }
        }

        private static string GetFrontMatterProp(string prop, byte[] content)
        {
            using (var mem = new MemoryStream(content))
            using (var st = new StreamReader(mem))
            {
                bool inFM = false;
                while (true)
                {
                    string line = st.ReadLine();
                    if (line == null)
                        break;
                    if (string.IsNullOrEmpty(line))
                        continue;

                    // a partir d'ici, si on a du contenu, ca doit
                    // être le frontmatter, sinon c'est KO
                    if (line.TrimEnd().StartsWith("---"))
                    {
                        if (!inFM)
                            inFM = true;
                        else // fin du FrontMatter, pas d'id
                            return null;
                    }
                    else if (!inFM) // pas vide et pas FM => pas d'id
                    {
                        return null;
                    }
                    else if (line.Trim().StartsWith(prop))
                    {
                        var id = line.Substring(line.IndexOf(prop) + (prop.Length)).TrimStart();
                        if (id.StartsWith(":"))
                            id = id.Substring(1).Trim();
                        id = id.Trim();
                        return id;
                    }
                }
            }


            return null;
        }


        public static void SyncPagesFromServer(string messageBody)
        {
            // et en réponse à une modification des triggers via l'api ou 
            // le back-office
            try
            {
                SyncPagesFromServer();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }

        private static void SyncPagesFromSources(string messageBody)
        {
            var t = JsonConvert.DeserializeObject<SourceCodeChangedMessage>(messageBody);

            SyncPagesFromSources();
        }

        public static void SyncPagesFromSources()
        {
            var sync = GetAllPagesFromBoth();
            foreach (var s in sync.Values)
            {
                if (s.ServerContent == null)
                    s.ServerContent = "";
                if (s.GitContent == null)
                    s.GitContent = "";

                if (!s.ServerContent.Equals(s.GitContent))
                {
                    if (!string.IsNullOrEmpty(s.GitContent))
                    {
                        // on réhydrate les données et on push depuis git
                        // vers les points API.
                        Page p = ParsePage(s.PathFromData, s.GitContent);
                        if (p.Id == null)
                            p.Id = Guid.NewGuid().ToString().ToLowerInvariant();


                        var sections = ParseSections(p.Id, s.GitContent);
                        //Trigger cengrp = string.IsNullOrEmpty(s.ServerContent) ? new Trigger() : (GetYamlDeserializer().Deserialize<Trigger>(s.ServerContent));


                        Console.WriteLine($"Upserting Page : {p.Path}");
                        //PushPage(p);
                    }
                    else if (!string.IsNullOrEmpty(s.ServerContent))
                    {
                        var cengrp = GetYamlDeserializer().Deserialize<Trigger>(s.ServerContent);

                        Console.WriteLine($"Deleting Trigger : {cengrp.Label}/{cengrp.Id}");
                        DeleteTrigger(cengrp);
                    }
                }
            }
        }

        private static Page ParsePage(string pathFromData, string gitContent)
        {
            Page p = new Page();
            p.Path = pathFromData;
            var bs = Encoding.UTF8.GetBytes(gitContent);
            p.Id = GetFrontMatterProp("id", bs);
            p.Title = GetFrontMatterProp("title", bs);
            p.ThemeId = GetFrontMatterProp("theme", bs);
            p.ThemeId = GetFrontMatterProp("pageIcon", bs);
            p.ParentId = GetFrontMatterProp("parentId", bs);
            p.IsPublic = false;
            string tmp = GetFrontMatterProp("public", bs);
            if (tmp != null)
            {
                if (bool.TryParse(tmp, out bool ispublic))
                    p.IsPublic = ispublic;
            }
            p.Path = GetPagePath(pathFromData);
            p.UserIds = new List<string>(GetPageUsers(pathFromData));

            return p;
        }


        public static List<PageSection> ParseSections(string pageId, string gitContent)
        {
            List<PageSection> ret = new List<PageSection>();

            PageSection current = null;
            StringBuilder blrCurrent = new StringBuilder();
            using (var sr = new StringReader(gitContent))
            {
                while (true)
                {
                    string line = sr.ReadLine();
                    if (line == null)
                        break;
                    if(line.StartsWith("[JournalApp.ContentModule]::"))
                    {
                        if (current != null)
                        {
                            current.Data = blrCurrent.ToString();
                            ret.Add(current);
                        }

                        current = new PageSection();
                        current.PageId = pageId;
                        current.Kind = line.Substring("[JournalApp.ContentModule]::".Length).Trim();
                        current.Order = ret.Count;
                        blrCurrent = new StringBuilder();
                    }
                    else
                    {
                        blrCurrent.AppendLine(line);
                    }
                }
            }
            if(current!=null && !ret.Contains(current))
            {
                current.Data = blrCurrent.ToString();
                ret.Add(current);
            }

            return ret;
        }

        private static string GetPagePath(string pathFromData)
        {

            StringBuilder blr = new StringBuilder();
            var parts = pathFromData.Split(new char[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length > 0)
            {
                if (parts[0].Contains("["))
                {
                    string pth = parts[0].Substring(0, parts[0].IndexOf("["));
                    blr.Append(pth);
                    string tmp = parts[0].Substring(parts[0].IndexOf("[") + 1);
                    string[] usrs = tmp.Split(new char[] { ',', ']' }, StringSplitOptions.RemoveEmptyEntries);

                }
                else
                    blr.Append(parts[0]);

                for (int i = 1; i < parts.Length; i++)
                {
                    blr.Append("/");
                    blr.Append(parts[i]);
                }
            }
            return blr.ToString();
        }

        private static string[] GetPageUsers(string pathFromData)
        {
            StringBuilder blr = new StringBuilder();
            var parts = pathFromData.Split(new char[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length > 0)
            {
                if (parts[0].Contains("["))
                {
                    string pth = parts[0].Substring(0, parts[0].IndexOf("["));
                    string tmp = parts[0].Substring(parts[0].IndexOf("[") + 1);
                    string[] usrs = tmp.Split(new char[] { ',', ']' }, StringSplitOptions.RemoveEmptyEntries);
                    return usrs;
                }
            }

            return new string[0];
        }


        private static Page PushPage(Page grp)
        {
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    using (var cli = new MainApiAgentWebClient("erza"))
                    {
                        var tmp = cli.UploadData<Page, Page>("/v1.0/journal/pages", "POST", grp);
                        if (tmp != null)
                            return tmp;
                    }
                }
                catch (WebException ex)
                {
                    Thread.Sleep(1000);
                }
            }
            return null;
        }



        //private static bool DeleteTrigger(Trigger grp)
        //{
        //    for (int i = 0; i < 3; i++)
        //    {
        //        try
        //        {
        //            using (var cli = new MainApiAgentWebClient("erza"))
        //            {
        //                var tmp = cli.UploadData<bool, string>($"v1.0/system/mesh/local/triggers/{grp.Id}", "DELETE", "");
        //                return tmp;
        //            }
        //        }
        //        catch (WebException ex)
        //        {
        //            Thread.Sleep(1000);
        //        }
        //    }
        //    return false;
        //}

    }
}
