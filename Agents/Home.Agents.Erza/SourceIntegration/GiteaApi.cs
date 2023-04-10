using Home.Common.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace Home.Agents.Erza.SourceIntegration
{
    internal class GiteaApi : IGitRepo
    {
        string _user = "", _password = "";
        string _baseUrl = "";
        string _repoOwner = "";
        string _repo = "";
        string _refName = "main";

        private string Get(string url)
        {
            using (var cli = new HttpClient())
            {
                var auth = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_user}:{_password}"));
                cli.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", auth);
                var resp = cli.GetAsync(url).Result;
                if (resp.IsSuccessStatusCode)
                    return resp.Content.ReadAsStringAsync().Result;
                else
                    Console.WriteLine($"Call to {url} failed with : {resp.StatusCode}/{resp.ReasonPhrase}");
            }

            return null;
        }



        public class GiteaGetFileResponse
        {
            public string name { get; set; }
            public string path { get; set; }
            public string sha { get; set; }
            public string type { get; set; }
            public int size { get; set; }
            public string encoding { get; set; }
            public string content { get; set; }
        }


        public GitFile GetFile(string path)
        {
            List<GitFile> ret = new List<GitFile>();
            var url = _baseUrl;
            if (!url.EndsWith("/"))
                url += "/";
            url += $"api/v1/repos/{_repoOwner}/{_repo}/contents/{path}?ref={_refName}";
            var content = Get(url);
            if (content != null)
            {
                var t = JsonConvert.DeserializeObject<GiteaGetFileResponse>(content);
                
                var f = new GitFile()
                {
                    Path = t.path,
                    ServerSignature = t.sha,
                };
                if (t.encoding.Equals("base64", StringComparison.InvariantCultureIgnoreCase))
                    f.Content = Convert.FromBase64String(t.content);
                ret.Add(f);
            }

            return ret.FirstOrDefault();
        }

        public IEnumerable<GitFileEntry> GetFiles(string path)
        {
            List<GitFileEntry> ret = new List<GitFileEntry>();
            var url = _baseUrl;
            if (!url.EndsWith("/"))
                url += "/";
            url += $"api/v1/repos/{_repoOwner}/{_repo}/contents/{path}?ref={_refName}";
            var content = Get(url);
            if(content!=null)
            {
                var tmp = JsonConvert.DeserializeObject<GiteaGetFileResponse[]>(content);
                foreach(var t in tmp)
                {
                    if (t.type.Equals("dir"))
                    {
                        var sub = GetFiles(t.path);
                        ret.AddRange(sub);
                    }
                    else
                    {
                        ret.Add(new GitFileEntry()
                        {
                            Path = t.path,
                            ServerSignature = t.sha,
                            Type = t.type
                        });
                    }
                }
            }

            return ret;
        }

        public void Init(AutomationMeshSouceCodeIntegration config)
        {
            string url = config.GitRepoUrl;
            _baseUrl = url.Substring(0, url.IndexOf("/dev/gitea/") + 11);
            _repoOwner = url.Substring(_baseUrl.Length, url.IndexOf("/", _baseUrl.Length) - _baseUrl.Length);
            _repo = Path.GetFileNameWithoutExtension(url);
            _user = config.GitUsername;
            _password = config.GitPassword;

            if (!string.IsNullOrEmpty(config.GitBranch))
                _refName = config.GitBranch;
            else
                _refName = "main";
        }

        private class DeleteCommit
        {
            public string branch { get; set; }
            public string message { get; set; }

            public string sha { get; set; }
        }

        public void DeleteFile(string path, string sha)
        {
            DeleteCommit cm = new DeleteCommit()
            {
                message = "Suppr auto via synchro",
                branch = _refName,
                sha = sha
            };

            var url = _baseUrl;
            if (!url.EndsWith("/"))
                url += "/";
            url += $"api/v1/repos/{_repoOwner}/{_repo}/contents/{path}";

            using (var cli = new HttpClient())
            {
                var auth = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_user}:{_password}"));
                cli.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", auth);
                var rq = new HttpRequestMessage(HttpMethod.Delete, url);
                rq.Content = new StringContent(JsonConvert.SerializeObject(cm), Encoding.UTF8, "application/json");
                var resp = cli.SendAsync(rq).Result;
                if (!resp.IsSuccessStatusCode)
                    Console.WriteLine($"Call to {url} failed with : {resp.StatusCode}/{resp.ReasonPhrase}");
            }
        }

        private class UpdateCommit
        {
            public string branch { get; set; }
            public string message { get; set; }
            public string content { get; set; }
            public string sha { get; set; }
        }

        private class CreateCommit
        {
            public string branch { get; set; }
            public string message { get; set; }
            public string content { get; set; }
        }

        public void UpsertFile(string path, string content, string sha)
        {
            if(string.IsNullOrEmpty(sha))
            {
                CreateCommit cm = new CreateCommit()
                {
                    message = "Création auto via synchro",
                    branch = _refName,
                    content = Convert.ToBase64String(Encoding.UTF8.GetBytes(content))
                };

                var url = _baseUrl;
                if (!url.EndsWith("/"))
                    url += "/";
                url += $"api/v1/repos/{_repoOwner}/{_repo}/contents/{path}";

                using (var cli = new HttpClient())
                {
                    var auth = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_user}:{_password}"));
                    cli.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", auth);
                    var rq = new HttpRequestMessage(HttpMethod.Post, url);
                    rq.Content = new StringContent(JsonConvert.SerializeObject(cm), Encoding.UTF8, "application/json");
                    var resp = cli.SendAsync(rq).Result;
                    if (!resp.IsSuccessStatusCode)
                        Console.WriteLine($"Call to {url} failed with : {resp.StatusCode}/{resp.ReasonPhrase}");
                }

            }
            else
            {
                UpdateCommit cm = new UpdateCommit()
                {
                    message = "Maj auto via synchro",
                    branch = _refName,
                    sha = sha,
                    content = Convert.ToBase64String(Encoding.UTF8.GetBytes(content))
                };

                var url = _baseUrl;
                if (!url.EndsWith("/"))
                    url += "/";
                url += $"api/v1/repos/{_repoOwner}/{_repo}/contents/{path}";

                using (var cli = new HttpClient())
                {
                    var auth = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_user}:{_password}"));
                    cli.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", auth);
                    var rq = new HttpRequestMessage(HttpMethod.Put, url);
                    rq.Content = new StringContent(JsonConvert.SerializeObject(cm), Encoding.UTF8, "application/json");
                    var resp = cli.SendAsync(rq).Result;
                    if (!resp.IsSuccessStatusCode)
                        Console.WriteLine($"Call to {url} failed with : {resp.StatusCode}/{resp.ReasonPhrase}");
                }

            }
        }
    }
}
