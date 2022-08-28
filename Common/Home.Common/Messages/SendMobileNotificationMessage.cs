using System;
using System.Collections.Generic;
using System.Text;

namespace Home.Common.Messages
{
    public class SendMobileNotificationMessage : BaseMessage
    {
        public static readonly string MAIN_CHANNEL_ID = "ApplicationNotifs";
        public static readonly string ALERTS_CHANNEL_ID = "AlertsNotifs";
        public static readonly string PERSONAL_CHANNEL_ID = "PersonalNotifs";
        public static readonly string CHAT_CHANNEL_ID = "PersonalNotifs";
        public static readonly string DOWNLOADS_CHANNEL_ID = "DownloadsNotifs";

        public SendMobileNotificationMessage() : base("communication.notification.mobile.send")
        {

        }
        public SendMobileNotificationMessage(string messageTopic) : base(messageTopic)
        {
        }

        public string User { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
    }
}
