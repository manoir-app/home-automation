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
        #region ProductCategories
        [Route("categories"), HttpGet]
        public List<ProductCategory> GetProductCategorys()
        {
            var collection = MongoDbHelper.GetClient<ProductCategory>();
            var lst = collection.Find(x => true).ToList();
            return lst;
        }

        [Route("categories/{categoryId}"), HttpGet]
        public ProductCategory GetProductCategory(string categoryId)
        {
            var collection = MongoDbHelper.GetClient<ProductCategory>();
            var lst = collection.Find(x => x.Id == categoryId).FirstOrDefault();
            return lst;
        }

        [Route("categories"), HttpPost]
        public ProductCategory UpsertProductCategory([FromBody] ProductCategory category)
        {
            if (category.Id == null)
                category.Id = Guid.NewGuid().ToString("D").ToLowerInvariant();

            var collection = MongoDbHelper.GetClient<ProductCategory>();
            var lst = collection.Find(x => x.Id == category.Id).FirstOrDefault();

            if (lst == null)
            {
                collection.InsertOne(category);
            }
            else
            {
                collection.ReplaceOne(x => x.Id == lst.Id, category);
            }

            lst = collection.Find(x => x.Id == category.Id).FirstOrDefault();
            return lst;
        }

        [Route("categories/{categoryId}"), HttpDelete]
        public bool DeleteProductCategory(string categoryId, string categoryIdRemplacement)
        {
            var collection = MongoDbHelper.GetClient<ProductCategory>();
            var lst = collection.Find(x => x.Id == categoryId).FirstOrDefault();

            if (lst == null)
                return true;

            var items = MongoDbHelper.GetClient<Product>();
            var arrayFilters = Builders<Product>.Filter.AnyIn("Categories", categoryId);
            var count = items.CountDocuments(arrayFilters);
            if (/*string.IsNullOrEmpty(categoryId) && */count > 0)
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
