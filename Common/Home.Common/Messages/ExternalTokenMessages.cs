using System;
using System.Collections.Generic;
using System.Text;

namespace Home.Common.Messages
{
    public class NewExternalTokenMessage : BaseMessage
    {
        public const string TopicName = "users.security.extenaltokens.new";
        public NewExternalTokenMessage() : base(TopicName)
        {
        }

        public string UserId { get; set; }
        public string TokenType { get; set; }

    }

    public class ExternalTokenDeletedMessage : BaseMessage
    {
        public const string TopicName = "users.security.extenaltokens.deleted";
        public ExternalTokenDeletedMessage() : base(TopicName)
        {
        }

        public string UserId { get; set; }
        public string TokenType { get; set; }

    }

}
