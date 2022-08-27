using System;
using System.Collections.Generic;
using System.Text;

namespace Home.Common.Messages
{
    public class PimItemUpdateMessage : BaseMessage
    {
        public const string PimItemNew = "pim.item.new";
        public const string PimItemUpdate = "pim.item.update";
        public const string PimItemDone = "pim.item.done";

        public const string WakeUpTimeUpdate = "pim.scheduler.update.wakeuptime";


        public const string PimItemKindInfoItem = "InformationItem";
        public const string PimItemKindScheduler = "Scheduler";

        public PimItemUpdateMessage(string messageTopic) : base(messageTopic)
        {

        }

        public string ItemKind { get; set; }

        public string ItemId { get; set; }

        public string ItemTitle { get; set; }
    }
}
