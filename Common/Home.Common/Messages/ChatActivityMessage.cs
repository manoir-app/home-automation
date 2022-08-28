using System;
using System.Collections.Generic;
using System.Text;

namespace Home.Common.Messages
{
    public class ChatActivityMessage : BaseMessage
    {
        public const string TopicName = "pim.chat.activity";

        public ChatActivityMessage() : base(TopicName)
        {
        }

        public string ChannelId { get; set; }
        public string FromUserId { get; set; }
        public string Content { get; set; }
    }
}
