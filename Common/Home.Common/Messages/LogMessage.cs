using Home.Common.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace Home.Common.Messages
{
    public class LogMessage : BaseMessage
    {
        public const string LogMessageTopic = "system.mesh.log";
        public LogMessage() : base(LogMessageTopic)
        {
        }
        public LogMessage(string source, string sourceId, string message) : base(LogMessageTopic)
        {
            Data = new LogData()
            {
                Source = source,
                SourceId = sourceId,
                Message = message
            };
        }
        public LogMessage(string source, string sourceId, string imageUrl, string message) : base(LogMessageTopic)
        {
            Data = new LogData()
            {
                Source = source,
                SourceId = sourceId,
                Message = message,
                ImageUrl = imageUrl
            };
        }

        public LogData Data { get; set; }

    }
}
