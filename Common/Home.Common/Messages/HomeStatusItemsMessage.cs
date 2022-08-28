using System;
using System.Collections.Generic;
using System.Text;

namespace Home.Common.Messages
{
    public class HomeStatusItemsMessage : BaseMessage
    {
        public const string GetHomeStatusItems = "communication.homestatus.items.get";

        public HomeStatusItemsMessage(string messageTopic) : base(messageTopic)
        {
        }

        public HomeStatusItemsMessage() : this(GetHomeStatusItems)
        {
        }

        public string UserId { get; set; }

    }

    public class HomeStatusItemsMessageResponse : MessageResponse
    {
        public HomeStatusItemsMessageResponse() : base()
        {
            Items = new List<HomeStatusItem>();
        }

        public List<HomeStatusItem> Items { get; set; }
    }


    public class HomeStatusItem
    {
        public DateTimeOffset Date { get; set; }
        public string MessageKind { get; set; }
        public string Message { get; set; }
    }
}
