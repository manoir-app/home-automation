using Home.Common.FileFormats;
using Home.Common.Model;
using Home.Graph.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using MongoDB.Driver;
using System;
using System.Text;

namespace Home.Graph.Public.Controllers
{
    [Route("v1.0/todos/events")]
    [ApiController]
    public class EventsController : ControllerBase
    {
        [Route("export/{listId}/ical")]
        public IActionResult GetAsICal(string listId, bool includePrivate = false)
        {
            int nextDays = 60;

            var collection = MongoDbHelper.GetClient<TodoItem>();

            var bldr = Builders<TodoItem>.Filter.Ne("Type", TodoItemType.TodoItem)
                & Builders<TodoItem>.Filter.Or(
                    Builders<TodoItem>.Filter.Eq<DateTimeOffset?>("DueDate", null),
                    Builders<TodoItem>.Filter.Lt<DateTimeOffset?>("DueDate", DateTimeOffset.Now.AddDays(nextDays)))
                ;

            if (!string.IsNullOrEmpty(listId))
                bldr &= Builders<TodoItem>.Filter.Eq("ListId", listId);

            var lst = collection.Find(bldr).ToList();
            var iCalStr = VCalendarFormat.FromEventList(lst);
            var bs = Encoding.UTF8.GetBytes(iCalStr);
            return new FileContentResult(bs, MediaTypeHeaderValue.Parse("text/calendar"));
        }
    }
}
