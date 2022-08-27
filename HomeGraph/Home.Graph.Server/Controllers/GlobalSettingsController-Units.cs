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
    public partial class GlobalSettingsController
    {
        #region Units
        [Route("units"), HttpGet]
        public List<UnitOfMeasurement> GetUnits()
        {
            var collection = MongoDbHelper.GetClient<UnitOfMeasurement>();
            var lst = collection.Find(x => true).ToList();
            return lst;
        }

        [Route("units/{unitId}"), HttpGet]
        public UnitOfMeasurement GetUnit(string unitId)
        {
            var collection = MongoDbHelper.GetClient<UnitOfMeasurement>();
            var lst = collection.Find(x => x.Id == unitId).FirstOrDefault();
            return lst;
        }

        [Route("units"), HttpPost]
        public UnitOfMeasurement UpsertUnit([FromBody] UnitOfMeasurement unit)
        {
            if (unit.Id == null)
                unit.Id = Guid.NewGuid().ToString("D").ToLowerInvariant();

            var collection = MongoDbHelper.GetClient<UnitOfMeasurement>();
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

        [Route("units/{unitId}"), HttpDelete]
        public bool DeleteUnit(string unitId)
        {
            var collection = MongoDbHelper.GetClient<UnitOfMeasurement>();
            var lst = collection.Find(x => x.Id == unitId).FirstOrDefault();

            if (lst == null)
                return true;

            var items = MongoDbHelper.GetClient<Product>();
            var arrayFilters = Builders<Product>.Filter.AnyIn("UnitId", unitId);
            var count = items.CountDocuments(arrayFilters);
            if (/*string.IsNullOrEmpty(unitId) && */count > 0)
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
