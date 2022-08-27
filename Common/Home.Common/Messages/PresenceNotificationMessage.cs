using Home.Common.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace Home.Common.Messages
{
    public class PresenceNotificationMessage : BaseMessage
    {
        public PresenceNotificationMessage() : base("users.presence.activity")
        {

        }
        public PresenceNotificationMessage(PresenceNotificationData data) : this()
        {
            this.Data = data;
        }

        public PresenceNotificationData Data { get; set; }
    }
}
