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
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Home.Agents.Erza.SourceIntegration
{
    partial class GitSync
    {
        private class SceneGroupWithScenes : SceneGroup
        {
            public SceneGroupWithScenes()
            {
                Scenes = new List<Scene>();
            }

            public List<Scene> Scenes { get; set; }
        }


        private static void SyncSceneGroupsFromServer()
        {
            // à appeler de façon périodique pour s'assurer que les
            // sources sont à jour
            var git = GetGit();
            Dictionary<string, SyncFile> sync = GetAllGroupScenesFromBoth(git);

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
                        string path = $"scenes/{grp.Id}.yaml";
                        Console.WriteLine("Creating in Git : " + path);
                        git.UpsertFile(path, grp.ServerContent, "");
                    }
                }
            }
        }

        private static Dictionary<string, SyncFile> GetAllGroupScenesFromBoth()
        {
            IGitRepo git = GetGit();
            return GetAllGroupScenesFromBoth(git);
        }

        private static Dictionary<string, SyncFile> GetAllGroupScenesFromBoth(IGitRepo git)
        {
            Dictionary<string, SyncFile> sync = new Dictionary<string, SyncFile>();
            GetScenesFilesFromGit(git, sync, "scenes/");
            GetFilesFromServer(sync);
            return sync;
        }

        private static void GetScenesFilesFromGit(IGitRepo git, Dictionary<string, SyncFile> sync, string folder)
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
                var grp = GetYamlDeserializer().Deserialize<SceneGroupWithScenes>(sf.GitContent);
                if (grp != null)
                {
                    sf.Id = grp.Id;
                    if (grp.Scenes != null)
                    {
                        foreach (var r in grp.Scenes)
                        {
                            r.GroupId = grp.Id;
                        }
                    }
                    // cette propriété est non deterministe : elle
                    // depend de l'état actuel de la mesh
                    grp.CurrentActiveScenes = null;
                    sync.Add(grp.Id, sf);
                }
            }
        }

        private static void GetFilesFromServer(Dictionary<string, SyncFile> sync)
        {
            var grps = GetGroups();
            var scenes = GetAllScenes();
            foreach (var grp in grps)
            {
                grp.Scenes = (from z in scenes
                              where z.GroupId != null && z.GroupId.Equals(grp.Id, StringComparison.InvariantCultureIgnoreCase)
                              select z).ToList();
                grp.CurrentActiveScenes = null;
                SyncFile sf = null;
                if (!sync.TryGetValue(grp.Id, out sf))
                {
                    sf = new SyncFile
                    {
                        Id = grp.Id
                    };
                    sync.Add(grp.Id, sf);
                }
                sf.ServerContent = GetYamlSerializer().Serialize(grp);
            }
        }

        private static SceneGroupWithScenes[] GetGroups()
        {
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    using (var cli = new MainApiAgentWebClient("erza"))
                    {
                        var tmp = cli.DownloadData<SceneGroupWithScenes[]>("v1.0/homeautomation/scenes/groups");
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

        private static Scene[] GetAllScenes()
        {
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    using (var cli = new MainApiAgentWebClient("erza"))
                    {
                        var tmp = cli.DownloadData<Scene[]>("v1.0/homeautomation/scenes/scenes");
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






        

        public static void SyncSceneGroupsFromServer(string messageBody)
        {

            try
            {
                SyncSceneGroupsFromServer();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }

        private static void SyncSceneGroupsFromSources(string messageBody)
        {
            var t = JsonConvert.DeserializeObject<SourceCodeChangedMessage>(messageBody);

            var sync = GetAllGroupScenesFromBoth();
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
                        SceneGroupWithScenes gitgrp = GetYamlDeserializer().Deserialize<SceneGroupWithScenes>(s.GitContent);
                        SceneGroupWithScenes cengrp = string.IsNullOrEmpty(s.ServerContent) ? new SceneGroupWithScenes() : (GetYamlDeserializer().Deserialize<SceneGroupWithScenes>(s.ServerContent));

                        if (gitgrp.Id == null)
                            gitgrp.Id = Guid.NewGuid().ToString().ToLowerInvariant();
                        if (cengrp.Scenes == null)
                            cengrp.Scenes = new List<Scene>();
                        if (gitgrp.Scenes == null)
                            gitgrp.Scenes = new List<Scene>();

                        Console.WriteLine($"Upserting group : {gitgrp.Label}/{gitgrp.Id}");
                        PushGroup(gitgrp);

                        foreach (var sc in gitgrp.Scenes)
                        {
                            sc.GroupId = gitgrp.Id;
                            Console.WriteLine($"Upserting scene : {sc.Label}/{sc.Id}");
                            PushScene(sc);
                        }

                        foreach (var sc in cengrp.Scenes)
                        {
                            var ingit = (from z in gitgrp.Scenes
                                         where z.Id.Equals(sc.Id)
                                         select z).FirstOrDefault();
                            if (ingit == null)
                            {
                                Console.WriteLine($"Deleting scene : {sc.Label}/{sc.Id}");
                                DeleteScene(sc);
                            }
                        }
                    }
                    else if (!string.IsNullOrEmpty(s.ServerContent))
                    {
                        var cengrp = GetYamlDeserializer().Deserialize<SceneGroupWithScenes>(s.ServerContent);
                        foreach (var sc in cengrp.Scenes)
                        {
                            Console.WriteLine($"Deleting scene : {sc.Label}/{sc.Id}");
                            DeleteScene(sc);
                        }
                        Console.WriteLine($"Deleting group : {cengrp.Label}/{cengrp.Id}");
                        DeleteGroup(cengrp);
                    }
                }
            }
        }

        private static SceneGroup PushGroup(SceneGroup grp)
        {
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    using (var cli = new MainApiAgentWebClient("erza"))
                    {
                        var tmp = cli.UploadData<SceneGroup, SceneGroup>("/v1.0/homeautomation/scenes/groups", "POST", grp);
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

        private static Scene PushScene(Scene grp)
        {
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    using (var cli = new MainApiAgentWebClient("erza"))
                    {
                        var tmp = cli.UploadData<Scene, Scene>("/v1.0/homeautomation/scenes/scenes", "POST", grp);
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

        private static bool DeleteScene(Scene scene)
        {
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    using (var cli = new MainApiAgentWebClient("erza"))
                    {
                        var tmp = cli.UploadData<bool, string>($"/v1.0/homeautomation/scenes/scenes/{scene.Id}", "DELETE", "");
                        return tmp;
                    }
                }
                catch (WebException ex)
                {
                    Thread.Sleep(1000);
                }
            }
            return false;
        }

        private static bool DeleteGroup(SceneGroup grp)
        {
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    using (var cli = new MainApiAgentWebClient("erza"))
                    {
                        var tmp = cli.UploadData<bool, string>($"/v1.0/homeautomation/scenes/groups/{grp.Id}", "DELETE", "");
                        return tmp;
                    }
                }
                catch (WebException ex)
                {
                    Thread.Sleep(1000);
                }
            }
            return false;
        }

    }
}
