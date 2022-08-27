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
    partial class FinancesController
    {
        #region Categories
        [Route("categories"), HttpGet]
        public IEnumerable<BudgetCategory> GetCategories()
        {
            var collection = MongoDbHelper.GetClient<BudgetCategory>();
            var lst = collection.Find(x => true).ToList();
            return lst;
        }

        [Route("categories"), HttpPost]
        public BudgetCategory UpsertCategory([FromBody] BudgetCategory category)
        {
            if (category.Id == null)
                category.Id = Guid.NewGuid().ToString("D").ToLowerInvariant();

            var collection = MongoDbHelper.GetClient<BudgetCategory>();
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
        public bool DeleteCategory(string categoryId, string categoryIdRemplacement)
        {
            var collection = MongoDbHelper.GetClient<BudgetCategory>();
            var lst = collection.Find(x => x.Id == categoryId).FirstOrDefault();

            if (lst == null)
                return true;

            var items = MongoDbHelper.GetClient<BankRecord>();
            var count = items.CountDocuments(x => x.BudgetCategoryId == lst.Id);
            if (string.IsNullOrEmpty(categoryId) && count > 0)
            {
                return false;
            }
            else
            {
                var upd = Builders<BankRecord>.Update.Set("BudgetCategoryId", categoryIdRemplacement);
                items.UpdateMany(x => x.BudgetCategoryId == lst.Id, upd);
            }

            collection.DeleteOne(x => x.Id == lst.Id);
            return true;
        }
        #endregion


    }
}
