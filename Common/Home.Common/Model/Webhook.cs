using System;
using System.Collections.Generic;
using System.Text;

namespace Home.Common.Model
{
    public class Webhook
    {
        public string Id { get; set; }

        public string Agent { get; set; }
        public string MessageTopic { get; set; }

        public string MessageContent { get; set; }

    }
}
