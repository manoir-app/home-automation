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
        #region RecipeCuisines
        [Route("recipes/cuisines"), HttpGet]
        public List<RecipeCuisine> GetRecipeCuisines()
        {
            var collection = MongoDbHelper.GetClient<RecipeCuisine>();
            var lst = collection.Find(x => true).ToList();
            return lst;
        }

        [Route("recipes/cuisines/{accountId}"), HttpGet]
        public RecipeCuisine GetRecipeCuisine(string categoryId)
        {
            var collection = MongoDbHelper.GetClient<RecipeCuisine>();
            var lst = collection.Find(x => x.Id == categoryId).FirstOrDefault();
            return lst;
        }

        [Route("recipes/cuisines"), HttpPost]
        public RecipeCuisine UpsertRecipeCuisine([FromBody] RecipeCuisine cuisine)
        {
            if (string.IsNullOrEmpty(cuisine.Id))
                cuisine.Id = Guid.NewGuid().ToString("D").ToLowerInvariant();

            var collection = MongoDbHelper.GetClient<RecipeCuisine>();
            var lst = collection.Find(x => x.Id == cuisine.Id).FirstOrDefault();

            if (lst == null)
            {
                collection.InsertOne(cuisine);
            }
            else
            {
                collection.ReplaceOne(x => x.Id == lst.Id, cuisine);
            }

            lst = collection.Find(x => x.Id == cuisine.Id).FirstOrDefault();
            return lst;
        }

        [Route("recipes/cuisines/{accountId}"), HttpDelete]
        public bool DeleteRecipeCuisine(string cuisineId, string cuisineIdRemplacement)
        {
            var collection = MongoDbHelper.GetClient<RecipeCuisine>();
            var lst = collection.Find(x => x.Id == cuisineId).FirstOrDefault();

            if (lst == null)
                return true;

            var items = MongoDbHelper.GetClient<Recipe>();
            var count = items.CountDocuments(x => x.RecipeCuisineId == lst.Id);
            if (string.IsNullOrEmpty(cuisineId) && count > 0)
            {
                return false;
            }
            else
            {
                var upd = Builders<Recipe>.Update.Set("RecipeCuisineId", cuisineIdRemplacement);
                items.UpdateMany(x => x.RecipeCuisineId == lst.Id, upd);
            }

            collection.DeleteOne(x => x.Id == lst.Id);
            return true;
        }
        #endregion
    }
}
