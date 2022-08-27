using Home.Common.Model;
using Home.Graph.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Home.Graph.Server.Controllers
{
    partial class InformationItemsController
    {
        [Route("{user}/buckets"), HttpGet]
        public IEnumerable<InformationItemsBucket> GetBucketsForUser(string user)
        {
            if(user==null)
                return null;

            user = user.ToLowerInvariant();

            var collection = MongoDbHelper.GetClient<InformationItemsBucket>();
            var lst = collection.Find(x => x.UserId == user).ToList();

            if(lst.Count==0)
            {
                var buk = new InformationItemsBucket()
                {
                    Id = Guid.NewGuid().ToString("D").ToLowerInvariant(),
                    Name = "default",
                    UserId = user
                };
                collection.InsertOne(buk);
                lst.Add(buk);
            }

            return lst;
        }

        [Route("{user}/buckets"), HttpPost]
        public InformationItemsBucket UpsertBucketForUser(string user, [FromBody] InformationItemsBucket bucket)
        {
            if (user == null)
                return null;

            user = user.ToLowerInvariant();

            if(bucket.UserId==null)
                bucket.UserId = user;
            if (bucket.Id == null)
                bucket.Id = Guid.NewGuid().ToString("D").ToLowerInvariant();

            var collection = MongoDbHelper.GetClient<InformationItemsBucket>();
            var lst = collection.Find(x => x.Id == bucket.Id).FirstOrDefault();

            if(lst==null)
            {
                collection.InsertOne(bucket);
            }
            else
            {
                collection.ReplaceOne(x => x.Id == lst.Id, bucket);
            }

            lst = collection.Find(x => x.Id == bucket.Id).FirstOrDefault();
            return lst;
        }

        [Route("{user}/buckets/{bucketId}/clear")]
        public bool ClearBucketsForUser(string user, string bucketId)
        {
            if (user == null)
                return false;

            user = user.ToLowerInvariant();

            var collection = MongoDbHelper.GetClient<InformationItemsBucket>();
            var lst = collection.Find(x => x.Id == bucketId).FirstOrDefault();

            if (lst == null)
                return true;

            if (lst.UserId == user)
            {
                var items = MongoDbHelper.GetClient<InformationItem>();
                items.DeleteMany(x => x.BucketId == bucketId);
                items.DeleteMany(x => x.BucketId == lst.Name);
            }

            return true;
        }

        [Route("{user}/buckets/{bucketId}"), HttpDelete]
        public bool DeleteBucket(string user, string bucketId, string bucketIdRemplacement)
        {
            if (user == null)
                return false;

            user = user.ToLowerInvariant();

            var collection = MongoDbHelper.GetClient<InformationItemsBucket>();
            var lst = collection.Find(x => x.Id == bucketId).FirstOrDefault();

            if (lst == null)
                return true;

            if (lst.UserId == user)
            {
                var items = MongoDbHelper.GetClient<InformationItem>();
                var count = items.CountDocuments(x => x.UserId == user && x.BucketId == lst.Id);
                if (string.IsNullOrEmpty(bucketIdRemplacement) && count > 0)
                {
                    return false;
                }
                else
                {
                    var upd = Builders<InformationItem>.Update.Set("BucketId", bucketIdRemplacement);
                    items.UpdateMany(x => x.UserId == user && x.BucketId == lst.Id,upd);
                }

                collection.DeleteOne(x => x.Id == lst.Id);
                return true;
            }

            return false;
        }


    }
}
