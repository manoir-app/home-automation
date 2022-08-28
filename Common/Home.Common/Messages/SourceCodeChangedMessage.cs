using System;
using System.Collections.Generic;
using System.Text;

namespace Home.Common.Messages
{
    public class SourceCodeChangedMessage : BaseMessage
    {
        public const string SourceCodeChangedTopic = "sources.changed";

        public SourceCodeChangedMessage() : base(SourceCodeChangedTopic)
        {
        }

        public string CommitId { get; set; }
        public string Pusher { get; set; }
    }
}
