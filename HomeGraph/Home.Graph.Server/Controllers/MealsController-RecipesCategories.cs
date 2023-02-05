using Home.Common.Model;
using Home.Graph.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System.Collections.Generic;
using System;
using MQTTnet.Packets;

namespace Home.Graph.Server.Controllers
{
    partial class MealsController 
    {
        #region RecipeCategorys
        [Route("recipes/categories"), HttpGet]
        public List<RecipeCategory> GetRecipeCategories()
        {
            var collection = MongoDbHelper.GetClient<RecipeCategory>();
            var lst = collection.Find(x => true).ToList();
            return lst;
        }

        [Route("recipes/categories/{accountId}"), HttpGet]
        public RecipeCategory GetRecipeCategory(string categoryId)
        {
            var collection = MongoDbHelper.GetClient<RecipeCategory>();
            var lst = collection.Find(x => x.Id == categoryId).FirstOrDefault();
            return lst;
        }

        [Route("recipes/categories"), HttpPost]
        public RecipeCategory UpsertRecipeCategory([FromBody] RecipeCategory category)
        {
            if (string.IsNullOrEmpty(category.Id))
                category.Id = Guid.NewGuid().ToString("D").ToLowerInvariant();

            var collection = MongoDbHelper.GetClient<RecipeCategory>();
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

        [Route("recipes/categories/{accountId}"), HttpDelete]
        public bool DeleteRecipeCategory(string categoryId, string categoryIdRemplacement)
        {
            var collection = MongoDbHelper.GetClient<RecipeCategory>();
            var lst = collection.Find(x => x.Id == categoryId).FirstOrDefault();

            if (lst == null)
                return true;

            var items = MongoDbHelper.GetClient<Recipe>();
            var count = items.CountDocuments(x => x.RecipeCategoryId == lst.Id);
            if (string.IsNullOrEmpty(categoryId) && count > 0)
            {
                return false;
            }
            else
            {
                var upd = Builders<Recipe>.Update.Set("RecipeCategoryId", categoryIdRemplacement);
                items.UpdateMany(x => x.RecipeCategoryId == lst.Id, upd);
            }

            collection.DeleteOne(x => x.Id == lst.Id);
            return true;
        }
        #endregion


    }
}
