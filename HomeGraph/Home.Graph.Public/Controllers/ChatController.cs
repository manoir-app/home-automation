using Home.Common.Model;
using Home.Graph.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System;
using System.Collections.Generic;

namespace Home.Graph.Public.Controllers
{
    [Route("v1.0/chat"), Authorize(Roles = "Device,User")]
    [ApiController]
    public class ChatController : ControllerBase
    {

        [HttpPost, Route("messages/{channelId}")]
        public ChatMessage PushMessage(string channelId, string fromUserId, [FromBody] string content)
        {
            return ChatHelper.PushMessage(channelId, fromUserId, content);
        }

        [HttpGet, Route("messages/{channelId}")]
        public List<ChatMessage> PushMessage(string channelId, DateTimeOffset? since = null)
        {
            if (since == null)
                since = DateTimeOffset.Now.AddMonths(-2);

            return ChatHelper.GetMessagesInChannel(channelId, since.GetValueOrDefault());
        }

        [HttpGet, Route("channels/me")]
        public List<ChatChannel> GetMyChannels(string userName = null)
        {

            var collection = MongoDbHelper.GetClient<ChatChannel>();
            var chans = collection.Find(x => x.UserIds.Contains(userName)).ToList();
            return chans;
        }
    }
}
