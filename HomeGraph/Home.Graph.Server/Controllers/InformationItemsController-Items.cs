using Home.Common.Messages;
using Home.Common.Model;
using Home.Graph.Common;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Home.Graph.Server.Controllers
{
    partial class InformationItemsController
    {
        [Route("{user}/items"), HttpGet]
        public IEnumerable<InformationItem> GetItems(string user, string bucketId = null, string source = null, string kind = null, DateTimeOffset? since = null, bool includeRead = false)
        {
            if (user == null)
                return null;

            user = user.ToLowerInvariant();

            var collection = MongoDbHelper.GetClient<InformationItem>();

            var filter = Builders<InformationItem>.Filter.Eq("UserId", user);

            if (!string.IsNullOrEmpty(bucketId))
                filter &= Builders<InformationItem>.Filter.Eq("BucketId", bucketId);
            if (!string.IsNullOrEmpty(source))
                filter &= Builders<InformationItem>.Filter.Eq("Source", source);

            if (!string.IsNullOrEmpty(kind))
            {
                InformationItemKind enumkind = InformationItemKind.NewsItem;
                if (!Enum.TryParse<InformationItemKind>(kind, out enumkind))
                    throw new ArgumentException(kind + " is not a valid value", "kind");
                filter &= Builders<InformationItem>.Filter.Eq("Kind", enumkind);
            }

            if (!includeRead)
                filter &= Builders<InformationItem>.Filter.Eq("Status", InformationItemStatus.New);

            if (since.HasValue)
                filter &= Builders<InformationItem>.Filter.Gte("Date", since);

            var lst = collection.Find(filter);
            var ret = lst.ToList();
            ret.Sort((a, b) => a.Date.CompareTo(b.Date));
            return ret;
        }

        [Route("{user}/items"), HttpPost]
        public bool PostItems(string user, [FromBody] InformationItem[] items)
        {
            if (user == null)
                return false;

            user = user.ToLowerInvariant();

            var buckets = GetBucketsForUser(user);
            var defBuck = (from z in buckets where z.Name.Equals("default") select z).FirstOrDefault();
            if (defBuck == null)
                defBuck = buckets.FirstOrDefault();

            if (defBuck == null)
                throw new InvalidOperationException("No buckets");

            var collection = MongoDbHelper.GetClient<InformationItem>();




            foreach (var item in items)
            {
                item.UserId = user;
                if (item.Source == null)
                    item.Source = "default";
                if (item.BucketId == null)
                    item.BucketId = defBuck.Id;
                else
                {
                    var bck = (from z in buckets
                               where z.Id.Equals(item.BucketId)
                               select z).FirstOrDefault();
                    if (bck == null)
                    {
                        bck = (from z in buckets
                               where z.Name.Equals(item.BucketId)
                               select z).FirstOrDefault();
                    }

                    if (bck == null)
                        item.BucketId = defBuck.Id;
                    else
                        item.BucketId = bck.Id;
                }


                // si pas d'item, on va quand même regarder si
                // ce n'est pas un doublon...
                if (item.Id == null)
                {
                    if (item.Source != null && item.SourceUrl != null)
                    {
                        var exists = collection.Find(x => x.Source == item.Source && x.SourceUrl == item.SourceUrl).FirstOrDefault();
                        if (exists != null)
                            item.Id = exists.Id;
                    }
                }

                if (item.Id == null)
                {
                    item.Id = Guid.NewGuid().ToString("N");
                    collection.InsertOne(item);
                    PimHelper.NotifyPimItemUpdate(PimItemUpdateMessage.PimItemNew,
                        PimItemUpdateMessage.PimItemKindInfoItem,
                        item.Id, item.Title);
                }
                else
                {
                    collection.ReplaceOne(x => x.Id == item.Id, item);
                    PimHelper.NotifyPimItemUpdate(PimItemUpdateMessage.PimItemUpdate,
                        PimItemUpdateMessage.PimItemKindInfoItem,
                        item.Id, item.Title);
                }
            }

            return true;
        }

        [Route("{user}/items/{id}/read"), HttpGet]
        public bool MarkAsRead(string user, string id)
        {
            if (user == null)
                return false;

            user = user.ToLowerInvariant();

            var collection = MongoDbHelper.GetClient<InformationItem>();
            var filter = Builders<InformationItem>.Filter.Eq("Id", id);
            var up = Builders<InformationItem>.Update.Set("Status", InformationItemStatus.Read);

            var res = collection.UpdateOne(filter, up);
            if(res.ModifiedCount==1)
            {
                var item = collection.Find(x => x.Id == id).FirstOrDefault();
                if(item!=null)
                    PimHelper.NotifyPimItemUpdate(PimItemUpdateMessage.PimItemUpdate,
                        PimItemUpdateMessage.PimItemKindInfoItem,
                        item.Id, item.Title);
            }
            return true;
        }

        [Route("{user}/items/{id}/keep"), HttpGet]
        public bool MarkAsKept(string user, string id)
        {
            if (user == null)
                return false;

            user = user.ToLowerInvariant();

            var collection = MongoDbHelper.GetClient<InformationItem>();
            var filter = Builders<InformationItem>.Filter.Eq("Id", id);
            var up = Builders<InformationItem>.Update.Set("Status", InformationItemStatus.Kept);

            var res = collection.UpdateOne(filter, up);
            if (res.ModifiedCount == 1)
            {
                var item = collection.Find(x => x.Id == id).FirstOrDefault();
                if (item != null)
                    PimHelper.NotifyPimItemUpdate(PimItemUpdateMessage.PimItemUpdate,
                        PimItemUpdateMessage.PimItemKindInfoItem,
                        item.Id, item.Title);
            }
            return true;
        }

    }
}
