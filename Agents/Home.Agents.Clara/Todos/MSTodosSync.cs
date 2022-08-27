using Home.Common;
using Home.Common.Model;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Net;
using Newtonsoft.Json;

namespace Home.Agents.Clara.Todos
{
    static class MSTodosSync
    {
        public static void Sync(string userName)
        {
            var tk = GetToken(userName);
            if (tk == null)
                return;

            var lists = GetUserTodosLists(userName);

            foreach (var lst in lists)
            {
                if (lst.SyncDatas != null && lst.SyncDatas.Count > 0)
                {
                    var sc = (from z in lst.SyncDatas
                              where z.ExternalServiceId.Equals("MSTODOS", StringComparison.InvariantCultureIgnoreCase)
                              select z).FirstOrDefault();
                    if (sc != null)
                    {
                        SyncList(lst, sc.ItemId, userName, tk);
                    }
                }
            }

            lists = GetMeshTodosLists();
            foreach (var lst in lists)
            {
                if (lst.SyncDatas != null && lst.SyncDatas.Count > 0)
                {
                    var sc = (from z in lst.SyncDatas
                              where z.ExternalServiceId.Equals("MSTODOS", StringComparison.InvariantCultureIgnoreCase)
                              select z).FirstOrDefault();
                    if (sc == null)
                    {
                        sc = new TodoItemSyncData()
                        {
                            ExternalServiceId = "MSTODOS",
                            ForUserId = userName,
                            ItemId = CreateTodoList(tk, lst),
                            LastSync = DateTime.Now
                        };
                        lst.SyncDatas.Add(sc);
                        UpdateTodoList(lst);
                    }

                    SyncList(lst, sc.ItemId, userName, tk);
                }
            }

        }

        private static string CreateTodoList(ExternalToken tk, TodoList lst)
        {
            using (var cli = new WebClient())
            {
                cli.Headers.Set(HttpRequestHeader.Authorization, "Bearer " + tk.Token);
                cli.Headers.Set(HttpRequestHeader.ContentType, "application/json");
                string ret = cli.UploadString("https://graph.microsoft.com/v1.0/me/todo/lists", "POST",
                    JsonConvert.SerializeObject(new CreateTodoListRequest()
                    {
                        displayName = "Home Automation"
                    }));
                var tmp = JsonConvert.DeserializeObject<CreateTodoListResponse>(ret);
                return tmp?.id;
            }
        }

        public class CreateTodoListRequest
        {
            public string displayName { get; set; }
        }


        public class CreateTodoListResponse
        {
            [JsonProperty("@odata.type")]
            public string odatatype { get; set; }
            public string id { get; set; }
            public string displayName { get; set; }
            public bool isOwner { get; set; }
            public bool isShared { get; set; }
            public string wellknownListName { get; set; }
        }

        public class TodoTaskResponse
        {
            [JsonProperty("@odata.context")]
            public string odatacontext { get; set; }
            [JsonProperty("@odata.deltaLink")]
            public string odatadeltalink { get; set; }
            [JsonProperty("@odata.nextLink")]
            public string odatanextlink { get; set; }
            public List<MSTodoItem> value { get; set; }
            
        }

        public class MSTodoItem
        {
            [JsonProperty("@odata.etag")]
            public string odataetag { get; set; }
            public string importance { get; set; }
            public bool isReminderOn { get; set; }
            public string status { get; set; }
            public string title { get; set; }
            public DateTime createdDateTime { get; set; }
            public DateTime lastModifiedDateTime { get; set; }
            public string id { get; set; }
            public Body body { get; set; }
            public string extensionsodatacontext { get; set; }
            public Extension[] extensions { get; set; }
        }

        public class Body
        {
            public string content { get; set; }
            public string contentType { get; set; }
        }

     

        public class Linkedresource
        {
            public string webUrl { get; set; }
            public string applicationName { get; set; }
            public string displayName { get; set; }
            public string id { get; set; }
        }

        public class Extension
        {
            public string extensionName { get; set; }
            public string id { get; set; }
            public string idBis { get; set; }
            public string value { get; set; }
        }

        private static List<MSTodoItem> GetMSTodos(ExternalToken tk, string externalListId)
        {
            var ret = new List<MSTodoItem>();

            string url = string.Format("https://graph.microsoft.com/v1.0/me/todo/lists/{0}/tasks?$expand=extensions($filter=id%20eq%20'microsoft.graph.openTypeExtension.maNoirId')", externalListId);
            try
            {
                TodoTaskResponse todoTasks;
                string response;
                do
                {
                    using (var cli = new WebClient())
                    {
                        cli.Headers.Add(HttpRequestHeader.Authorization, "Bearer " + tk.Token);
                        cli.Headers.Add(HttpRequestHeader.ContentType, "application/json; charset=utf-8");
                        cli.Encoding = Encoding.UTF8;
                        response = cli.DownloadString(url);
                    }
                    todoTasks = JsonConvert.DeserializeObject<TodoTaskResponse>(response);
                    ret.AddRange(todoTasks.value);
                    url = todoTasks.odatanextlink;
                }
                while (!string.IsNullOrEmpty(url));
            }
            catch (Exception ex)
            {
            }

            return ret;
        }



        private static void SyncList(TodoList lst, string externalListId, string userId, ExternalToken token)
        {
            var localItems = GetTodosForList(lst.Id);
            var distantItem = GetMSTodos(token, externalListId);
            foreach (var item in localItems)
            {
                var sc = (from z in item.SyncDatas
                          where z.ExternalServiceId.Equals("MSTODOS", StringComparison.InvariantCultureIgnoreCase)
                          && z.ForUserId.Equals(userId)
                          select z).FirstOrDefault();
                if (sc != null)
                {
                    var distIt = (from z in distantItem
                                  where z.id.Equals(sc.ItemId, StringComparison.InvariantCultureIgnoreCase)
                                  select z).FirstOrDefault();
                    sc.LastSync = DateTimeOffset.Now;
                }
                else
                {
                    sc = new TodoItemSyncData()
                    {
                        ExternalServiceId = "MSTODOS",
                        ForUserId = userId,
                        ItemId = CreateTodoItemInMS(item, externalListId, token)?.id,
                        LastSync = DateTimeOffset.Now,
                    };
                    item.SyncDatas.Add(sc);
                }
            }
        }


        public class CreateTodoRequest
        {
            public string title { get; set; }
            public string body { get; set; }

            public DateTimeWithZone dueDateTime { get; set; }
            public TodoLinkedResource[] linkedResources { get; set; }

            public Extension[] extensions { get; set; }
        }

        public class TodoLinkedResource
        {
            public string webUrl { get; set; }
            public string applicationName { get; set; }
            public string displayName { get; set; }
        }

        public class DateTimeWithZone
        {
            public string dateTime { get; set; }
            public string timeZone { get; set; }
        }


        private static MSTodoItem CreateTodoItemInMS(TodoItem item, string externalListId, ExternalToken token)
        {
            using (var cli = new WebClient())
            {
                cli.Headers.Set(HttpRequestHeader.Authorization, "Bearer " + token.Token);
                cli.Headers.Set(HttpRequestHeader.ContentType, "application/json");
                string ret = cli.UploadString($"https://graph.microsoft.com/v1.0/me/todo/lists/{externalListId}/tasks",
                    "POST",
                    JsonConvert.SerializeObject(new CreateTodoRequest()
                    {
                        title = item.Label,
                        body = item.Description,
                        dueDateTime = item.DueDate.HasValue ? new DateTimeWithZone()
                        {
                            dateTime = item.DueDate.Value.UtcDateTime.ToString("yyyyMMddTHHmmss.fff"),
                            timeZone = "UTC"
                        } : null,
                        extensions = new Extension[]
                        {
                            new Extension() { id = "maNoirId" , value = item.Id}
                        }
                    }));
                var tmp = JsonConvert.DeserializeObject<MSTodoItem>(ret);



                return tmp;
            }
        }

        private static ExternalToken GetToken(string username)
        {
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    using (var cli = new MainApiAgentWebClient("clara"))
                    {
                        var exts = cli.DownloadData<List<ExternalToken>>($"/v1.0/security/tokens/{username}/azuread");
                        if (exts != null && exts.Count >= 1)
                            return exts[0];
                        else
                        {

                        }
                        break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }

            return null;
        }

        private static List<TodoList> GetUserTodosLists(string username)
        {
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    using (var cli = new MainApiAgentWebClient("clara"))
                    {
                        var exts = cli.DownloadData<List<TodoList>>($"/v1.0/todos/users/{username}/lists");
                        return exts;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }

            return null;
        }
        private static List<TodoList> GetMeshTodosLists()
        {
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    using (var cli = new MainApiAgentWebClient("clara"))
                    {
                        var exts = cli.DownloadData<List<TodoList>>($"/v1.0/todos/mesh/lists");
                        return exts;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }

            return null;
        }

        private static List<TodoItem> GetTodosForList(string listId)
        {
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    using (var cli = new MainApiAgentWebClient("clara"))
                    {
                        var exts = cli.DownloadData<List<TodoItem>>($"/v1.0/todos/todoItems?listId={listId}&includeDone=true");
                        return exts;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }

            return null;
        }

        private static TodoList UpdateTodoList(TodoList list)
        {
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    using (var cli = new MainApiAgentWebClient("clara"))
                    {
                        var exts = cli.UploadData<TodoList, TodoList>($"/v1.0/todos/lists", "POST", list);
                        return exts;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }

            return null;
        }

    }
}
