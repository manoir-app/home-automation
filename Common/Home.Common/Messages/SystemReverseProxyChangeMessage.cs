using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Text;

namespace Home.Common.Messages
{
   
    public class SystemReverseProxyChangeMessage : BaseMessage
    {
        public const string TopicName = "gaia.refresh-reverse-proxy";

        public SystemReverseProxyChangeMessage() : base(TopicName)
        {
        }

        public string FrpConfigFile { get; set; }
    }
}
