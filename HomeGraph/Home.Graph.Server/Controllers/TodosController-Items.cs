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
        [Route("todoItems"), HttpGet]
        public List<TodoItem> Get(string userId = null, string listId = null, string syncServiceId=null, int nextDays = 7, bool includeDone = false, bool onlyScheduled = false)
        {
            if(userId==null)
                userId = UsersController.GetCurrentUserId(User);

            var collection = MongoDbHelper.GetClient<TodoItem>();

            var bldr = Builders<TodoItem>.Filter.Eq("Type", TodoItemType.TodoItem)
                & Builders<TodoItem>.Filter.Or(
                    Builders<TodoItem>.Filter.Eq<DateTimeOffset?>("DueDate", null),
                    Builders<TodoItem>.Filter.Lt<DateTimeOffset?>("DueDate", DateTimeOffset.Now.AddDays(nextDays)))
                ;

            if (!includeDone)
                bldr &= Builders<TodoItem>.Filter.Ne("Status", TodoItemStatus.Done);

            if (!string.IsNullOrEmpty(listId))
                bldr &= Builders<TodoItem>.Filter.Eq("ListId", listId);

            if (!string.IsNullOrEmpty(userId))
                bldr &= Builders<TodoItem>.Filter.Eq("UserId", userId);


            if (!string.IsNullOrEmpty(syncServiceId))
                bldr &= Builders<TodoItem>.Filter.Eq("SyncDatas.ExternalServiceId", syncServiceId);

            if (onlyScheduled)
                bldr &= Builders<TodoItem>.Filter.SizeGt("Schedule", 0);

            var lst = collection.Find(bldr).ToList();
            return lst;
        }

        [Route("todoItems/mesh"), HttpGet]
        public List<TodoItem> Get(string listId = null, string syncServiceId = null, int nextDays = 7, bool includeDone = false, bool onlyScheduled = false)
        {
            var collection = MongoDbHelper.GetClient<TodoItem>();

            var bldr = Builders<TodoItem>.Filter.Eq("Type", TodoItemType.TodoItem)
                & Builders<TodoItem>.Filter.Eq<string>("UserId", "#MESH#")
                & Builders<TodoItem>.Filter.Or(
                    Builders<TodoItem>.Filter.Eq<DateTimeOffset?>("DueDate", null),
                    Builders<TodoItem>.Filter.Lt<DateTimeOffset?>("DueDate", DateTimeOffset.Now.AddDays(nextDays)))
                ;

            if (!includeDone)
                bldr &= Builders<TodoItem>.Filter.Ne("Status", TodoItemStatus.Done);

            if (!string.IsNullOrEmpty(listId))
                bldr &= Builders<TodoItem>.Filter.Eq("ListId", listId);


            if (!string.IsNullOrEmpty(syncServiceId))
                bldr &= Builders<TodoItem>.Filter.Eq("SyncDatas.ExternalServiceId", syncServiceId);

            if (onlyScheduled)
                bldr &= Builders<TodoItem>.Filter.SizeGt("Schedule", 0);

            var lst = collection.Find(bldr).ToList();
            return lst;
        }

        [Route("all"), HttpPost]
        public List<TodoItem> UpsertItem([FromBody] TodoItem item)
        {
            return UpsertItem(new TodoItem[] { item });
        }

        [Route("all/bulk"), HttpPost]
        public List<TodoItem> UpsertItem([FromBody] TodoItem[] list)
        {
            List<TodoItem> ret = new List<TodoItem>();
            foreach (var item in list)
            {
                if (item.Id == null)
                    item.Id = Guid.NewGuid().ToString("D").ToLowerInvariant();

                if (item.ListId == null)
                    BadRequest("ListId must be set");
                item.ListId = item.ListId.ToLowerInvariant();

                var lstcoll = MongoDbHelper.GetClient<TodoList>();
                var laListe = lstcoll.Find(x => x.Id == item.ListId).FirstOrDefault();

                if(laListe == null)
                    BadRequest("ListId is invalid");

                if (laListe.UserId.Equals("#MESH#"))
                    item.UserId = "#MESH#";

                if (item.UserId == null)
                    item.UserId = laListe.UserId;

                var collection = MongoDbHelper.GetClient<TodoItem>();
                var lst = collection.Find(x => x.Id == item.Id).FirstOrDefault();

                if (lst == null)
                {
                    collection.InsertOne(item);
                }
                else
                {
                    collection.ReplaceOne(x => x.Id == item.Id, item);
                }

                lst = collection.Find(x => x.Id == item.Id).FirstOrDefault();
                if(lst!=null)
                    ret.Add(lst);
            }

            return ret;
        }

        [Route("todoItems/{itemId}"), HttpDelete]
        public bool DeleteTodoItem(string itemId)
        {
            itemId = itemId.ToLowerInvariant();
            var collection = MongoDbHelper.GetClient<TodoItem>();
            return collection.DeleteOne(x => x.Type == TodoItemType.TodoItem && x.Id == itemId).DeletedCount == 1;
        }


        [Route("todoItems/{itemId}/markasdone"), HttpGet]
        public bool MarkAsDone(string itemId)
        {
            return MarkAsDone(new string[] { itemId });
        }

        [Route("todoItems/markasdone"), HttpGet]
        public bool MarkAsDone([FromBody] string[] itemIds)
        {
                var collection = MongoDbHelper.GetClient<TodoItem>();
            var ret = collection.UpdateMany(x => itemIds.Contains(x.Id),
                Builders<TodoItem>.Update
                .Set("Status", TodoItemStatus.Done));

            return ret.IsAcknowledged && ret.ModifiedCount > 0;
        }
    }
}
