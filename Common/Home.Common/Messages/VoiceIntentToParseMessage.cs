using Home.Common.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace Home.Common.Messages
{

    public class VoiceIntentSource
    {
        public string Topic { get; set; }
        public string[] Tokens { get; set; }

        public decimal Confidence { get; set; } = 1;

        public Dictionary<string, string> Datas { get; set; } = new Dictionary<string, string>();
    }

    public enum ParsedVoiceIntentKind
    {
        NatsMessage
    }

    public class ParsedVoiceIntent
    {
        public ParsedVoiceIntentKind Kind { get; set; }
    }

    public class NatsParsedVoiceIntent : ParsedVoiceIntent
    { 
        public string MessageTopic { get; set; }
        public object MessageBody { get; set; }
    }

    public class VoiceIntentToParseMessage : BaseMessage
    {
        public VoiceIntentToParseMessage() : base("voice.intent.parsing")
        {

        }
        public VoiceIntentToParseMessage(VoiceIntentSource data) : this()
        {
            this.Data = data;
        }

        public VoiceIntentSource Data { get; set; }
    }

    public class VoiceIntentToParseMessageResponse : MessageResponse
    {
        public VoiceIntentToParseMessageResponse() : base()
        {

        }
    }
}
