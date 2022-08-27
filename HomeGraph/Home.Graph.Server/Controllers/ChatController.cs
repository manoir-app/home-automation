using Home.Common.Model;
using Home.Graph.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Security.Claims;

namespace Home.Graph.Server.Controllers
{
    [Route("v1.0/chat"), Authorize(Roles = "Agent,Device,User,Admin")]
    [ApiController]
    public class ChatController : ControllerBase
    {

        [HttpPost, Route("messages/{channelId}")]
        public ChatMessage PushMessage(string channelId, [FromBody] string content, string fromUserId = null)
        {
            var ursName = UsersController.GetCurrentUserId(User);
            if (fromUserId == null && !string.IsNullOrEmpty(ursName))
                fromUserId = ursName;

            if (string.IsNullOrEmpty(fromUserId))
                throw new InvalidOperationException();

            return ChatHelper.PushMessage(channelId, fromUserId, content);
        }

        [HttpGet, Route("messages/{channelId}")]
        public List<ChatMessage> PushMessage(string channelId, DateTimeOffset? since = null)
        {
            if (since == null)
                since = DateTimeOffset.Now.AddMonths(-2);

            return ChatHelper.GetMessagesInChannel(channelId, since.GetValueOrDefault());
        }

        #region Channels

        [Route("channels"), HttpGet]
        public List<ChatChannel> GetChannels()
        {
            var collection = MongoDbHelper.GetClient<ChatChannel>();
            var lst = collection.Find(x => true).ToList();


            if(User.IsInRole("User") || User.IsInRole("Admin"))
            {
                var userId = User.Claims.Where(c => c.Type.Equals(ClaimTypes.NameIdentifier)).FirstOrDefault();

                lst = (from z in lst
                       where z.UserIds == null
                       || z.UserIds.Contains(userId.Value)
                       select z).ToList();
            }

            return lst;
        }

        [Route("channels/{channelId}"), HttpGet]
        public ChatChannel GetChannel(string channelId)
        {
            var collection = MongoDbHelper.GetClient<ChatChannel>();
            var lst = collection.Find(x => x.Id == channelId).FirstOrDefault();
            return lst;
        }

        [Route("channels"), HttpPost]
        public ChatChannel UpsertChannel([FromBody] ChatChannel channel)
        {
            if (channel.Id == null)
                channel.Id = Guid.NewGuid().ToString("D").ToLowerInvariant();

            var collection = MongoDbHelper.GetClient<ChatChannel>();
            var lst = collection.Find(x => x.Id == channel.Id).FirstOrDefault();

            if (lst == null)
            {
                collection.InsertOne(channel);
            }
            else
            {
                collection.ReplaceOne(x => x.Id == lst.Id, channel);
            }

            lst = collection.Find(x => x.Id == channel.Id).FirstOrDefault();
            return lst;
        }

        [Route("channels/{channelId}"), HttpDelete]
        public bool DeleteChannel(string channelId)
        {
            channelId = channelId.ToLowerInvariant();
            var collection = MongoDbHelper.GetClient<ChatChannel>();
            var lst = collection.Find(x => x.Id == channelId).FirstOrDefault();

            if (lst == null)
                return true;

            var items = MongoDbHelper.GetClient<ChatMessage>();
            items.DeleteMany(x => x.ChannelId.Equals(channelId));
            collection.DeleteOne(x => x.Id == lst.Id);

            return true;
        }
        
        #endregion


    }
}
