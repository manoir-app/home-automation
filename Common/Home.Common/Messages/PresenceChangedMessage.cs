using Home.Common.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace Home.Common.Messages
{
    public class PresenceChangedMessage : BaseMessage
    {
        public const string TopicName = "users.presence.changed";

        public PresenceChangedMessage() : base(TopicName)
        {

        }
        public PresenceChangedMessage(PresenceChangedMessageData data) : this()
        {
            this.Data = data;
        }

        public PresenceChangedMessage(string userid, PresenceData data) : this()
        {
            this.Data = new PresenceChangedMessageData()
            {
                Presence = data,
                UserId = userid 
            };
        }
        public PresenceChangedMessageData Data { get; set; }
    }

    public class PresenceChangedMessageData
    {
        public PresenceChangedMessageData()
        {
        }

        public string UserId { get; set; }
        public PresenceData Presence { get; set; }
    }
}
