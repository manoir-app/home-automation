using Home.Common.Model;
using Home.Graph.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System;
using System.Collections.Generic;

namespace Home.Graph.Server.Controllers
{
    public partial class StorageController 
    {
        #region storageunits
        [Route("storageunits"), HttpGet]
        public List<StorageUnit> GetStorageUnits(string roomId = null)
        {
            if (roomId != null)
                roomId = roomId.ToUpperInvariant();

            var collection = MongoDbHelper.GetClient<StorageUnit>();
            if (roomId == null)
            {
                var lst = collection.Find(x => true).ToList();
                return lst;
            }
            else
            {
                var lst = collection.Find(x => x.RoomId.Equals(roomId)).ToList();
                return lst;
            }
        }

        [Route("storageunits/{unitId}"), HttpGet]
        public StorageUnit GetStorageUnit(string unitId)
        {
            unitId = unitId.ToLowerInvariant();

            var collection = MongoDbHelper.GetClient<StorageUnit>();
            var lst = collection.Find(x => x.Id == unitId).FirstOrDefault();
            return lst;
        }

        [Route("storageunits"), HttpPost]
        public StorageUnit UpsertUnit([FromBody] StorageUnit unit)
        {
            if (unit.Id == null)
                unit.Id = Guid.NewGuid().ToString("D").ToLowerInvariant();

            var collection = MongoDbHelper.GetClient<StorageUnit>();
            var lst = collection.Find(x => x.Id == unit.Id).FirstOrDefault();

            if (lst == null)
            {
                collection.InsertOne(unit);
            }
            else
            {
                collection.ReplaceOne(x => x.Id == lst.Id, unit);
            }

            lst = collection.Find(x => x.Id == unit.Id).FirstOrDefault();
            return lst;
        }

        [Route("storageunits/{unitId}"), HttpDelete]
        public bool DeleteUnit(string unitId)
        {
            var collection = MongoDbHelper.GetClient<StorageUnit>();
            var lst = collection.Find(x => x.Id == unitId).FirstOrDefault();

            if (lst == null)
                return true;

            //var items = MongoDbHelper.GetClient<Product>();
            //var arrayFilters = Builders<Product>.Filter.AnyIn("StorageUnitId", unitId);
            //var count = items.CountDocuments(arrayFilters);
            //if (/*string.IsNullOrEmpty(unitId) && */count > 0)
            //{
            //    return false;
            //}
            //else
            //{

            //}

            collection.DeleteOne(x => x.Id == lst.Id);
            return true;
        }
        #endregion
    }
}
