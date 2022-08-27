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
    partial class ProductsController
    {
        #region ProductType
        [Route("types"), HttpGet]
        public List<ProductType> GetProductTypes()
        {
            var collection = MongoDbHelper.GetClient<ProductType>();
            var lst = collection.Find(x => true).ToList();
            return lst;
        }

        [Route("types/{typeId}"), HttpGet]
        public ProductType GetProductType(string typeId)
        {
            var collection = MongoDbHelper.GetClient<ProductType>();
            var lst = collection.Find(x => x.Id == typeId).FirstOrDefault();
            return lst;
        }

        [Route("types"), HttpPost]
        public ProductType UpsertProductType([FromBody] ProductType type)
        {
            if (type.Id == null)
                type.Id = Guid.NewGuid().ToString("D").ToLowerInvariant();

            var collection = MongoDbHelper.GetClient<ProductType>();
            var lst = collection.Find(x => x.Id == type.Id).FirstOrDefault();

            if (lst == null)
            {
                collection.InsertOne(type);
            }
            else
            {
                collection.ReplaceOne(x => x.Id == lst.Id, type);
            }

            lst = collection.Find(x => x.Id == type.Id).FirstOrDefault();
            return lst;
        }

        [Route("types/{typeId}"), HttpDelete]
        public bool DeleteProductType(string typeId, string typeIdRemplacement)
        {
            var collection = MongoDbHelper.GetClient<ProductType>();
            var lst = collection.Find(x => x.Id == typeId).FirstOrDefault();

            if (lst == null)
                return true;

            var items = MongoDbHelper.GetClient<Product>();
            var arrayFilters = Builders<Product>.Filter.AnyIn("productTypeId", typeId);
            var count = items.CountDocuments(arrayFilters);
            if (/*string.IsNullOrEmpty(typeId) && */count > 0)
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
