using Home.Common;
using Home.Common.Messages;
using System;
using System.Collections.Generic;
using System.Text;

namespace Home.Graph.Common
{
    public static class PimHelper
    {
        public static void NotifyPimItemUpdate(string topic, string type, string id, string title)
        {
            MessagingHelper.PushToLocalAgent(new PimItemUpdateMessage(topic)
            {
                ItemId = id,
                ItemTitle = title,
                ItemKind = type
            });
        }
    }
}
