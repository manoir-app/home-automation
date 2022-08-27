using System;
using System.Collections.Generic;
using System.Text;

namespace Home.Common.Messages
{
    public class DownloadItemMessage : BaseMessage
    {
        public const string TopicName = "services.downloads.queuefile";
        public DownloadItemMessage() : base(TopicName)
        {
        }

        public string SourceUrl { get; set; }
        public string DownloadId { get; set; }
    }

    public class DownloadItemProgressMessage : BaseMessage
    {
        public const string CancelledTopicName = "services.downloads.cancelled";

        public const string StartedTopicName = "services.downloads.started";

        public const string PausedTopicName = "services.downloads.paused";
        
        public DownloadItemProgressMessage(string name) : base(name)
        {
        }

        public string SourceUrl { get; set; }
        public string DownloadId { get; set; }
    }

    public class DownloadItemMessageResponse : MessageResponse
    {
        public string SourceUrl { get; set; }
        public bool WasQueued { get; set; }
        public string Identifier { get; set; }
    }

}
