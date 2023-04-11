using System;
using System.Collections.Generic;
using System.Text;

namespace Home.Common.Messages
{
    public class JournalPageChangedMessage : BaseMessage
    {
        public const string PageUpdatedTopic = "journal.page.updated";

        public JournalPageChangedMessage() : base(PageUpdatedTopic)
        {

        }

        public string PageId { get; set; }
        public string SectionId { get; set; }
    }

}
