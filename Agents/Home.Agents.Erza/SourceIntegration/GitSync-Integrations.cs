using Home.Common;
using Home.Common.Messages;
using Home.Common.Model;
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
        private static void SyncIntegrationsFromServer()
        {
            // à appeler de façon périodique pour s'assurer que les
            // sources sont à jour
            var git = GetGit();
            Dictionary<string, SyncFile> sync = GetAllIntegrationsFromBoth(git);

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
                        string path = $"integrations/{grp.Id}.yaml";
                        Console.WriteLine("Creating in Git : " + path);
                        git.UpsertFile(path, grp.ServerContent, "");
                    }
                }
            }
        }

        private static Dictionary<string, SyncFile> GetAllIntegrationsFromBoth()
        {
            IGitRepo git = GetGit();
            return GetAllIntegrationsFromBoth(git);
        }

        private static Dictionary<string, SyncFile> GetAllIntegrationsFromBoth(IGitRepo git)
        {
            Dictionary<string, SyncFile> sync = new Dictionary<string, SyncFile>();
            GetIntegrationsFilesFromGit(git, sync, "integrations/");
            GetIntegrationFilesFromServer(sync);
            return sync;
        }



        private static void GetIntegrationFilesFromServer(Dictionary<string, SyncFile> sync)
        {
            var Integrations = GetAllIntegrations();
            foreach (var item in Integrations)
            {
                SyncFile sf = null;
                if (!sync.TryGetValue(item.Id, out sf))
                {
                    sf = new SyncFile
                    {
                        Id = item.Id
                    };
                    sync.Add(item.Id, sf);
                }
                sf.ServerContent = GetYamlSerializer().Serialize(item);
            }
        }

        private static Integration[] GetAllIntegrations()
        {
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    using (var cli = new MainApiAgentWebClient("erza"))
                    {
                        var tmp = cli.DownloadData<Integration[]>("v1.0/system/mesh/local/integrations");
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





        private static void GetIntegrationsFilesFromGit(IGitRepo git, Dictionary<string, SyncFile> sync, string folder)
        {
            var files = git.GetFiles(folder);
            foreach (var file in files)
            {
                string filename = Path.GetFileName(file.Path);
                var gf = git.GetFile(file.Path);
                SyncFile sf = new SyncFile()
                {
                    GitFile = gf,
                    GitContent = Encoding.UTF8.GetString(gf.Content)
                };
                var grp = GetYamlDeserializer().Deserialize<Integration>(sf.GitContent);
                if (grp != null)
                {
                    sf.Id = grp.Id;
                    sync.Add(grp.Id, sf);
                }
            }
        }



        public static void SyncIntegrationsFromServer(string messageBody)
        {
            // et en réponse à une modification des Integrations via l'api ou 
            // le back-office
            try
            {
                SyncIntegrationsFromServer();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }

        private static void SyncIntegrationsFromSources(string messageBody)
        {
            var t = JsonConvert.DeserializeObject<SourceCodeChangedMessage>(messageBody);

            var sync = GetAllIntegrationsFromBoth();
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
                        Integration gitgrp = GetYamlDeserializer().Deserialize<Integration>(s.GitContent);
                        Integration cengrp = string.IsNullOrEmpty(s.ServerContent) ? new Integration() : (GetYamlDeserializer().Deserialize<Integration>(s.ServerContent));

                        if (gitgrp.Id == null)
                            gitgrp.Id = Guid.NewGuid().ToString().ToLowerInvariant();

                        Console.WriteLine($"Upserting Integration : {gitgrp.Label}/{gitgrp.Id}");
                        PushIntegration(gitgrp);
                    }
                    else if (!string.IsNullOrEmpty(s.ServerContent))
                    {
                        var cengrp = GetYamlDeserializer().Deserialize<Integration>(s.ServerContent);
                       
                        Console.WriteLine($"Deleting Integration : {cengrp.Label}/{cengrp.Id}");
                        DeleteIntegration(cengrp);
                    }
                }
            }
        }

        private static Integration PushIntegration(Integration grp)
        {
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    using (var cli = new MainApiAgentWebClient("erza"))
                    {
                        var tmp = cli.UploadData<Integration, Integration>("/v1.0/system/mesh/local/integrations/raw", "POST", grp);
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

      

        private static bool DeleteIntegration(Integration grp)
        {
            //for (int i = 0; i < 3; i++)
            //{
            //    try
            //    {
            //        using (var cli = new MainApiAgentWebClient("erza"))
            //        {
            //            var tmp = cli.UploadData<bool, string>($"v1.0/system/mesh/local/integrations/{grp.Id}", "DELETE", "");
            //            return tmp;
            //        }
            //    }
            //    catch (WebException ex)
            //    {
            //        Thread.Sleep(1000);
            //    }
            //}
            return false;
        }

    }
}
