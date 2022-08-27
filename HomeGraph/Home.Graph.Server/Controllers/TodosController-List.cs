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
    partial class TodosController
    {

        [Route("lists"), HttpGet]
        public List<TodoList> GetLists()
        {
            var usrId = UsersController.GetCurrentUserId(User);
            if (usrId == null)
                return new List<TodoList>();

            var collection = MongoDbHelper.GetClient<TodoList>();
            var lst = collection.Find(x => x.UserId == usrId && !x.IsDeleted).ToList();
            return lst;
        }

        [Route("mesh/lists"), HttpGet]
        public List<TodoList> GetMeshLists()
        {
            var collection = MongoDbHelper.GetClient<TodoList>();
            var lst = collection.Find(x => x.UserId == "#MESH#" && !x.IsDeleted).ToList();

            if (lst == null || lst.Count == 0)
            {
                var t = new TodoList()
                {
                    UserId = "#MESH#",
                    Id = Guid.NewGuid().ToString("D").ToLowerInvariant(),
                    IsDeleted = false,
                    Label = "Mesh List",
                    ListType = "mesh",
                    PrivacyLevel = PrivacyLevel.Private
                };
                collection.InsertOne(t);
                lst.Add(t);
            }

            return lst;
        }

        [Route("users/{userId}/lists")]
        [Route("lists"), HttpGet]
        public List<TodoList> GetLists(string userId)
        {
            var collection = MongoDbHelper.GetClient<TodoList>();
            var lst = collection.Find(x => x.UserId == userId && !x.IsDeleted).ToList();
            return lst;
        }

        [Route("lists/{listId}"), HttpGet]
        public TodoList GetList(string listId)
        {
            // TODO : vérifier que l'utilisateur est admin, ou que c'est
            // un agent / device, ou que la todo list lui apparatient bien

            var collection = MongoDbHelper.GetClient<TodoList>();
            var lst = collection.Find(x => x.Id == listId).FirstOrDefault();
            return lst;
        }

        [Route("lists"), HttpPost]
        public TodoList UpsertList([FromBody] TodoList list)
        {
            if (list.Id == null)
                list.Id = Guid.NewGuid().ToString("D").ToLowerInvariant();

            var collection = MongoDbHelper.GetClient<TodoList>();
            var lst = collection.Find(x => x.Id == list.Id).FirstOrDefault();

            if (lst == null)
            {
                collection.InsertOne(list);
            }
            else
            {
                if (lst.UserId.Equals(list.UserId))
                    collection.ReplaceOne(x => x.Id == lst.Id, list);
                else
                    BadRequest("User can't be changed by updating a list : please delete and recreate");
            }

            lst = collection.Find(x => x.Id == list.Id).FirstOrDefault();
            return lst;
        }

        [Route("lists/{listId}"), HttpDelete]
        public bool DeleteList(string listId, string listIdReplacement)
        {
            var collection = MongoDbHelper.GetClient<TodoList>();
            var lst = collection.Find(x => x.Id == listId).FirstOrDefault();

            if (lst == null || lst.IsDeleted)
                return true;

            // impossible de supprimer les listes automatiques
            // on peut supprimer une liste perso de #MESH#, mais pas celles
            // qui ont été créée par le système
            if (lst.ListType.Equals("mesh", StringComparison.InvariantCultureIgnoreCase)
                || lst.ListType.Equals("home", StringComparison.InvariantCultureIgnoreCase)
                || lst.ListType.Equals("notif", StringComparison.InvariantCultureIgnoreCase)
                || lst.ListType.Equals("notifs", StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }



            var items = MongoDbHelper.GetClient<TodoItem>();
            var count = items.CountDocuments(x => x.ListId == lst.Id);
            if (count == 0)
            {
                collection.DeleteOne(x => x.Id == lst.Id);
                return true;
            }
            else if (string.IsNullOrEmpty(listIdReplacement))
            {
                count = items.CountDocuments(x => x.ListId == lst.Id && x.Status != TodoItemStatus.Done);
                if (count > 0)
                    return false;
            }
            else
            {
                var upd = Builders<TodoItem>.Update.Set("ListId", listIdReplacement);
                items.UpdateMany(x => x.ListId == lst.Id && x.Status != TodoItemStatus.Done, upd);
            }

            var updList = Builders<TodoList>.Update.Set("IsDeleted", true);
            collection.UpdateOne(x => x.Id == lst.Id, updList);

            return true;
        }
    }
}
