using System;
using System.Collections.Generic;
using System.Text;

namespace Home.Common.Model
{
    public class LogData
    {
        public string Id { get; set; }
        public DateTimeOffset Date { get; set; }
        public string Source { get; set; }
        public string SourceId { get; set; }
        public string ImageUrl { get; set; }
        public string Message { get; set; }
    }
}
