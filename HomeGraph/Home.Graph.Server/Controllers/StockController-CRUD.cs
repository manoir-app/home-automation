using Home.Common.Model;
using Home.Graph.Common;
using Home.Graph.Server.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using static Microsoft.Azure.Amqp.Serialization.SerializableType;

namespace Home.Graph.Server.Controllers
{
    public partial class StockController
    {
        [Route("content/find"), HttpGet]
        public List<ProductStock> FindStock(string storageUnitId = null, string productId = null)
        {
            var collection = MongoDbHelper.GetClient<ProductStock>();

            var bldr = Builders<ProductStock>.Filter.Lt<DateTimeOffset>(x => x.DateAdded, DateTimeOffset.Now);

            if (!string.IsNullOrEmpty(productId))
                bldr &= Builders<ProductStock>.Filter.Eq("ProductId", productId);

            if (!string.IsNullOrEmpty(storageUnitId))
                bldr &= Builders<ProductStock>.Filter.Eq("StorageUnitId", storageUnitId);

            var lst = collection.Find(bldr).ToList();

            return lst;
        }

        [Route("content/{id}"), HttpGet]
        public ProductStock Get(string id)
        {
            id = id.ToLowerInvariant();
            var collection = MongoDbHelper.GetClient<ProductStock>();
            return collection.Find(x => x.Id.Equals(id)).FirstOrDefault();
        }


    }
}
