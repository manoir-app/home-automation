using System;
using System.Collections.Generic;
using System.Text;

namespace Home.Common.Model
{
    public class ChatMessage
    {
        public string Id { get; set; }

        public string ChannelId { get; set; }

        public string FromUserId { get; set; }

        public DateTimeOffset Date { get; set; }

        public string MessageContent { get; set; }

    }

    public class ChatChannel
    {
        public ChatChannel()
        {
            PrivacyLevel = PrivacyLevel.Private;
            UserIds = new List<string>();
        }

        public PrivacyLevel PrivacyLevel { get; set; }

        public string Id { get; set; }

        public string Name { get; set; }

        public List<string> UserIds { get; set; }
    }
}
