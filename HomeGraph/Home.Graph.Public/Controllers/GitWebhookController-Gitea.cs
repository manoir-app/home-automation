using Home.Common.Messages;
using Home.Common.Model;
using Home.Graph.Common;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System;

namespace Home.Graph.Public.Controllers
{
    partial class GitWebhookController 
    {
        [Route("{guid}/gitea")]
        public bool FromGitea(string guid, [FromBody] GiteaPushNotification notif)
        {
            if(string.IsNullOrEmpty(guid))
            {
                BadRequest();
                return false;
            }
            var meshColl = MongoDbHelper.GetClient<AutomationMesh>();
            var mesh = meshColl.Find(x => x.Id.Equals("local")).FirstOrDefault();
            if(mesh==null || mesh.SourceCodeIntegration==null || !guid.Equals(mesh.SourceCodeIntegration.WebhookNotificationPrefix))
            {
                BadRequest();
                return false;
            }

            if(notif==null)
            {
                BadRequest();
                return false;
            }

            string pusher = notif?.pusher?.username;
            // notif faite par nous, pas la peine de resync
            if (pusher.Equals("robot"))
                return true;

            MessagingHelper.PushToLocalAgent(new SourceCodeChangedMessage
            {
                CommitId = notif?.after,
                Pusher = pusher
            });

            return false;
        }

        #region Gitea Push Notification data classes
        public class GiteaPushNotification
        {
            public string secret { get; set; }
            public string _ref { get; set; }
            public string before { get; set; }
            public string after { get; set; }
            public string compare_url { get; set; }
            public GiteaCommit[] commits { get; set; }
            public GiteaRepository repository { get; set; }
            public GiteaPusher pusher { get; set; }
            public GiteaSender sender { get; set; }
        }

        public class GiteaRepository
        {
            public int id { get; set; }
            public GiteaOwner owner { get; set; }
            public string name { get; set; }
            public string full_name { get; set; }
            public string description { get; set; }
            public bool _private { get; set; }
            public bool fork { get; set; }
            public string html_url { get; set; }
            public string ssh_url { get; set; }
            public string clone_url { get; set; }
            public string website { get; set; }
            public int stars_count { get; set; }
            public int forks_count { get; set; }
            public int watchers_count { get; set; }
            public int open_issues_count { get; set; }
            public string default_branch { get; set; }
            public DateTime created_at { get; set; }
            public DateTime updated_at { get; set; }
        }

        public class GiteaOwner
        {
            public int id { get; set; }
            public string login { get; set; }
            public string full_name { get; set; }
            public string email { get; set; }
            public string avatar_url { get; set; }
            public string username { get; set; }
        }

        public class GiteaPusher
        {
            public int id { get; set; }
            public string login { get; set; }
            public string full_name { get; set; }
            public string email { get; set; }
            public string avatar_url { get; set; }
            public string username { get; set; }
        }

        public class GiteaSender
        {
            public int id { get; set; }
            public string login { get; set; }
            public string full_name { get; set; }
            public string email { get; set; }
            public string avatar_url { get; set; }
            public string username { get; set; }
        }

        public class GiteaCommit
        {
            public string id { get; set; }
            public string message { get; set; }
            public string url { get; set; }
            public GiteaAuthor author { get; set; }
            public GiteaCommitter committer { get; set; }
            public DateTime timestamp { get; set; }
        }

        public class GiteaAuthor
        {
            public string name { get; set; }
            public string email { get; set; }
            public string username { get; set; }
        }

        public class GiteaCommitter
        {
            public string name { get; set; }
            public string email { get; set; }
            public string username { get; set; }
        }

        #endregion
    }
}
