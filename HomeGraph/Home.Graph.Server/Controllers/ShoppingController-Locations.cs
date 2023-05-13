using Home.Common.Model;
using Home.Graph.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Home.Graph.Server.Controllers
{
    partial class ShoppingController 
    {
        #region ShoppingLocations
        [Route("locations"), HttpGet]
        public List<ShoppingLocation> GetShoppingLocations()
        {
            var collection = MongoDbHelper.GetClient<ShoppingLocation>();
            var lst = collection.Find(x => true).ToList();
            return lst;
        }

        [Route("locations/{locationId}"), HttpGet]
        public ShoppingLocation GetShoppingLocation(string locationId)
        {
            var collection = MongoDbHelper.GetClient<ShoppingLocation>();
            var lst = collection.Find(x => x.Id == locationId).FirstOrDefault();
            return lst;
        }

        [Route("locations"), HttpPost]
        public ShoppingLocation UpsertShoppingLocation([FromBody] ShoppingLocation account)
        {
            if (account.Id == null)
                account.Id = Guid.NewGuid().ToString("D").ToLowerInvariant();

            var collection = MongoDbHelper.GetClient<ShoppingLocation>();
            var lst = collection.Find(x => x.Id == account.Id).FirstOrDefault();

            if (lst == null)
            {
                collection.InsertOne(account);
            }
            else
            {
                collection.ReplaceOne(x => x.Id == lst.Id, account);
            }

            lst = collection.Find(x => x.Id == account.Id).FirstOrDefault();
            return lst;
        }

        [Route("locations/{locationId}"), HttpDelete]
        public bool DeleteShoppingLocation(string locationId)
        {
            var collection = MongoDbHelper.GetClient<ShoppingLocation>();
            var lst = collection.Find(x => x.Id == locationId).FirstOrDefault();

            if (lst == null)
                return true;

            var items = MongoDbHelper.GetClient<ShoppingSession>();
            var count = items.CountDocuments(x => x.ShoppingLocationId == lst.Id);
            if (count > 0)
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
