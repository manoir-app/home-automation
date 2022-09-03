using Home.Common.Messages;
using Home.Common.Model;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;

namespace Home.Graph.Common
{
    public static class ChatHelper
    {
        public static ChatMessage PushMessage(string channelId, string fromUserId, string content)
        {
            var coll = MongoDbHelper.GetClient<ChatMessage>();
            var chancoll = MongoDbHelper.GetClient<ChatChannel>();


            channelId = channelId.ToLowerInvariant();
            fromUserId = fromUserId.ToLowerInvariant();

            var chan = chancoll.Find(x => x.Id.Equals(channelId)).FirstOrDefault();
            if (chan == null)
            {
                Console.WriteLine($"Channel : {channelId} does not exists");
                throw new InvalidOperationException();
            }

            var okUsr = chan.UserIds.Contains(fromUserId);
            if(!okUsr)
            {
                Console.WriteLine($"Channel : {channelId} does not have {fromUserId} in its users");
                throw new InvalidOperationException();
            }

            var msg = new ChatMessage()
            {
                ChannelId = channelId,
                FromUserId = fromUserId,
                Date = DateTimeOffset.Now,
                Id = Guid.NewGuid().ToString("N"),
                MessageContent = content
            };

            coll.InsertOne(msg);

            MessagingHelper.PushToLocalAgent(new ChatActivityMessage()
            {
                ChannelId = channelId,
                FromUserId= fromUserId,
                Content =  content
            });

            return msg;
        }

        public static List<ChatMessage> GetMessagesInChannel(string channelId, DateTimeOffset since)
        {
            channelId = channelId.ToLowerInvariant();
            var coll = MongoDbHelper.GetClient<ChatMessage>();

            var lst = coll.Find(x => x.ChannelId.Equals(channelId)).Sort(Builders<ChatMessage>.Sort.Ascending(x => x.Date)).ToList();
            
            return lst;
        }

    }
}
