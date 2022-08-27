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

namespace Home.Graph.Server.Controllers
{
    public partial class StockController
    {
        #region StorageUnits
        [Route("storageUnits"), HttpGet]
        public List<StorageUnit> GetStorageUnits()
        {
            var collection = MongoDbHelper.GetClient<StorageUnit>();
            var lst = collection.Find(x => true).ToList();
            return lst;
        }

        [Route("storageUnits/{storageUnitId}"), HttpGet]
        public StorageUnit GetStorageUnit(string storageUnitId)
        {
            var collection = MongoDbHelper.GetClient<StorageUnit>();
            var lst = collection.Find(x => x.Id == storageUnitId).FirstOrDefault();
            return lst;
        }

        [Route("storageUnits"), HttpPost]
        public StorageUnit UpsertStorageUnit([FromBody] StorageUnit storageUnit)
        {
            if (storageUnit.Id == null)
                storageUnit.Id = Guid.NewGuid().ToString("D").ToLowerInvariant();

            if (storageUnit.SubElements != null)
            {
                foreach (var sub in storageUnit.SubElements)
                {
                    if (sub.Id == null)
                        sub.Id = Guid.NewGuid().ToString("D").ToLowerInvariant();
                }
            }

            var collection = MongoDbHelper.GetClient<StorageUnit>();
            var lst = collection.Find(x => x.Id == storageUnit.Id).FirstOrDefault();

            if (lst == null)
            {
                collection.InsertOne(storageUnit);
            }
            else
            {
                collection.ReplaceOne(x => x.Id == lst.Id, storageUnit);
            }

            lst = collection.Find(x => x.Id == storageUnit.Id).FirstOrDefault();
            return lst;
        }

        [Route("storageUnits/{storageUnitId}"), HttpDelete]
        public bool DeleteStorageUnit(string storageUnitId, string StorageUnitIdRemplacement)
        {
            var collection = MongoDbHelper.GetClient<StorageUnit>();
            var lst = collection.Find(x => x.Id == storageUnitId).FirstOrDefault();

            if (lst == null)
                return true;

            var items = MongoDbHelper.GetClient<ProductStock>();
            var arrayFilters = Builders<ProductStock>.Filter.AnyIn("storageUnitId", storageUnitId);
            var count = items.CountDocuments(arrayFilters);
            if (/*string.IsNullOrEmpty(StorageUnitId) && */count > 0)
            {
                return false;
            }
            else
            {

            }

            collection.DeleteOne(x => x.Id == lst.Id);
            return true;
        }
        #endregion

    }
}
