using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Text;

namespace Home.Common.Messages
{
   
    public class SystemCertificateChangeMessage : BaseMessage
    {
        public const string TopicName = "gaia.refresh-certificate";

        public SystemCertificateChangeMessage() : base(TopicName)
        {
        }
    }
}
