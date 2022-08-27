using Home.Common;
using Home.Common.Messages;
using Home.Common.Model;
using Home.Graph.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Home.Graph.Server.Controllers
{
    [Route("v1.0/downloads"), Authorize(Roles = "Device,Agent")]
    [ApiController]
    public class DownloadsController : ControllerBase
    {
        [HttpPost]
        public bool PostNewDownloads(DownloadItem[] items)
        {
            var collection = MongoDbHelper.GetClient<DownloadItem>();

            foreach (var item in items)
            {
                var exists = collection.Find(x => x.SourceUrl == item.SourceUrl).FirstOrDefault();
                if (exists != null)
                {
                    var up = Builders<DownloadItem>.Update
                        .Set("Title", item.Title);
                    collection.UpdateOne(x => x.Id == exists.Id, up);
                }
                else
                {
                    item.Id = Guid.NewGuid().ToString("D");
                    if (item.Status != DownloadItemStatus.DontDownload
                        && item.Status != DownloadItemStatus.New
                        && item.Status != DownloadItemStatus.Queued)
                    {
                        item.Status = DownloadItemStatus.New;
                    }

                    if (item.DateAdded < new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero))
                        item.DateAdded = DateTimeOffset.Now;

                    collection.InsertOne(item);

                    if (item.Status == DownloadItemStatus.Queued)
                    {
                        StartDownload(collection, item);
                    }
                }
            }

            return true;
        }

        [Route("find/byprivateId")]
        public DownloadItem FindByPrivateId(string id)
        {
            var collection = MongoDbHelper.GetClient<DownloadItem>();
            var exists = collection.Find(x => x.DownloadPrivateId == id).FirstOrDefault();
            return exists;
        }

        [Route("{status}")]
        public IEnumerable<DownloadItem> List(DownloadItemStatus status)
        {
            var collection = MongoDbHelper.GetClient<DownloadItem>();
            var exists = collection.Find(x => x.Status == status).ToList();
            exists.Sort((a, b) => b.DateAdded.CompareTo(a.DateAdded));
            return exists;
        }

        [Route("all")]
        public IEnumerable<DownloadItem> ListAll()
        {
            var collection = MongoDbHelper.GetClient<DownloadItem>();
            var exists = collection.Find(x => x.DateAdded > DateTimeOffset.Now.Date.AddMonths(-6)).ToList();
            return exists;
        }

        [Route("{id}/linked")]
        public IEnumerable<DownloadItem> ListLinked(string id)
        {
            var collection = MongoDbHelper.GetClient<DownloadItem>();
            var exists = collection.Find(x => x.LinkedDownloadId != null && x.LinkedDownloadId == id).ToList();
            return exists;
        }

        [Route("{id}/linkTo/{linkedItemId}")]
        public bool LinkTo(string id, string linkedItemId)
        {
            var collection = MongoDbHelper.GetClient<DownloadItem>();
            var up = Builders<DownloadItem>.Update
                    .Set("LinkedDownloadId", linkedItemId);
            var ret = collection.UpdateOne(x => x.Id == id, up);
            return ret.IsAcknowledged && ret.ModifiedCount == 1;
        }

        [Route("{id}/queue")]
        public bool Queue(string id)
        {
            var collection = MongoDbHelper.GetClient<DownloadItem>();
            var exists = collection.Find(x => x.Id == id).FirstOrDefault();
            if (exists != null)
            {
                if (StartDownload(collection, exists))
                    return true;
            }

            return false;
        }

        [Route("all/clear")]
        public bool Clear()
        {
            var collection = MongoDbHelper.GetClient<DownloadItem>();
            var res = collection.DeleteMany(x => x.Status != DownloadItemStatus.InProgress
            && x.Status != DownloadItemStatus.Queued);
            return res.IsAcknowledged;
        }

        [Route("all/abandon/byids"), HttpPost]
        public int MarkAsDontDownload([FromBody] string[] ids)
        {
            var collection = MongoDbHelper.GetClient<DownloadItem>();
            int cancelled = 0;
            foreach (var id in ids)
            {
                var exists = collection.Find(x => x.Id == id).FirstOrDefault();
                if (exists != null)
                {
                    cancelled++;
                    if (exists.Status != DownloadItemStatus.DontDownload)
                    {
                        var up = Builders<DownloadItem>.Update
                                .Set("Status", DownloadItemStatus.DontDownload);
                        var ret = collection.UpdateOne(x => x.Id == id, up);

                        MessagingHelper.PushToLocalAgent(new DownloadItemProgressMessage(DownloadItemProgressMessage.CancelledTopicName)
                        {
                            DownloadId = exists.Id,
                            SourceUrl = exists.SourceUrl
                        });

                        MessagingHelper.PushItemChange<DownloadItem>(exists.Id, ItemChangeKind.Update);
                    }
                }
            }

            return cancelled;
        }

        [Route("{id}/abandon")]
        public bool MarkAsDontDownload(string id)
        {
            var collection = MongoDbHelper.GetClient<DownloadItem>();
            var exists = collection.Find(x => x.Id == id).FirstOrDefault();
            if (exists != null)
            {
                var up = Builders<DownloadItem>.Update
                        .Set("Status", DownloadItemStatus.DontDownload);
                var ret = collection.UpdateOne(x => x.Id == id, up);

                MessagingHelper.PushToLocalAgent(new DownloadItemProgressMessage(DownloadItemProgressMessage.CancelledTopicName)
                {
                    DownloadId = exists.Id,
                    SourceUrl = exists.SourceUrl
                });

                MessagingHelper.PushItemChange<DownloadItem>(exists.Id, ItemChangeKind.Update);

                return true;
            }

            return false;
        }

        [Route("{id}/markascompleted")]
        public bool MarkAsCompleted(string id)
        {
            var collection = MongoDbHelper.GetClient<DownloadItem>();
            var exists = collection.Find(x => x.Id == id).FirstOrDefault();
            if (exists != null)
            {
                var up = Builders<DownloadItem>.Update
                        .Set("Status", DownloadItemStatus.Done);
                var ret = collection.UpdateOne(x => x.Id == id, up);
                MessagingHelper.PushItemChange<DownloadItem>(exists.Id, ItemChangeKind.Update);
                NotifyUsers(exists);
                return true;
            }

            return false;
        }

        [Route("{id}/markascompleted"), HttpPost]
        public bool MarkAsCompleted(string id, [FromBody] List<DownloadItemResult> files)
        {
            var collection = MongoDbHelper.GetClient<DownloadItem>();
            var exists = collection.Find(x => x.Id == id).FirstOrDefault();
            if (exists != null)
            {
                var up = Builders<DownloadItem>.Update
                        .Set("Results", files)
                        .Set("Status", DownloadItemStatus.Done);
                var ret = collection.UpdateOne(x => x.Id == id, up);

                // on notifie le changement général
                // la mise à dispo de fichiers se fait aussi
                // par un message NATS, mais il doit être envoyé
                // par le composant qui a fait le téléchargement
                MessagingHelper.PushItemChange<DownloadItem>(exists.Id, ItemChangeKind.Update);
                NotifyUsers(exists);

                return true;
            }

            return false;
        }

        private static void NotifyUsers(DownloadItem exists)
        {
            // on notifie tous les mains du téléchargement
            var usrctl = MongoDbHelper.GetClient<User>();
            var usrs = usrctl.Find(x => x.IsMain == true).ToList();

            foreach (var user in usrs)
            {
                try
                {
                    new UsersController(null, null).NotifyUser(user.Id, new UserNotification()
                    {
                        Category = "downloads",
                        Date = DateTime.Now,
                        SourceAgent = "home",
                        Title = "Téléchargement terminé",
                        Description = $"OK pour {exists.VideoMetadata.LocalTitle} ({exists.Title})",
                        UserId = user.Id
                    }, true); ;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }

        [Route("{id}/markasinprogress")]
        public bool MarkAsInProgress(string id, string privateid = null)
        {
            var collection = MongoDbHelper.GetClient<DownloadItem>();
            var exists = collection.Find(x => x.Id == id).FirstOrDefault();
            if (exists != null)
            {
                var up = Builders<DownloadItem>.Update
                        .Set("Status", DownloadItemStatus.InProgress);

                if (privateid != null)
                    up.Set("DownloadPrivateId", privateid);

                var ret = collection.UpdateOne(x => x.Id == id
                            && x.Status != DownloadItemStatus.Done, up);
                if (ret.ModifiedCount == 1)
                {
                    MessagingHelper.PushToLocalAgent(new DownloadItemProgressMessage(DownloadItemProgressMessage.StartedTopicName)
                    {
                        DownloadId = exists.Id,
                        SourceUrl = exists.SourceUrl
                    });
                    MessagingHelper.PushItemChange<DownloadItem>(exists.Id, ItemChangeKind.Update);

                }
                return true;
            }

            return false;
        }

        public class DirectDownloadRequest
        {
            public string Name { get; set; }
            public string Url { get; set; }
        }

        [Route("direct/{protocol}")]
        public bool StartDirectDownload(DirectDownloadRequest item)
        {
            return PostNewDownloads(new DownloadItem[] { new DownloadItem()
            {
                DateAdded = DateTime.Now,
                SourceUrl = item.Url,
                IsPrivate=true,
                Title = item.Name,
                Status = DownloadItemStatus.Queued
            }});
        }

        private static bool StartDownload(IMongoCollection<DownloadItem> collection, DownloadItem item)
        {
            try
            {
                var ret = MessagingHelper.RequestFromLocalAgent<DownloadItemMessageResponse>(new DownloadItemMessage()
                {
                    DownloadId = item.Id,
                    SourceUrl = item.SourceUrl
                });

                if (ret != null && ret.WasQueued)
                {
                    var up = Builders<DownloadItem>.Update
                            .Set("DownloadPrivateId", ret.Identifier)
                            .Set("Status", DownloadItemStatus.InProgress);

                    collection.UpdateOne(x => x.Id == item.Id, up);

                    MessagingHelper.PushToLocalAgent(new DownloadItemProgressMessage(DownloadItemProgressMessage.StartedTopicName)
                    {
                        DownloadId = item.Id,
                        SourceUrl = item.SourceUrl
                    });
                    MessagingHelper.PushItemChange<DownloadItem>(item.Id, ItemChangeKind.Update);

                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Download Controller : push failed : " + ex.Message);
            }

            return false;
        }
    }
}
