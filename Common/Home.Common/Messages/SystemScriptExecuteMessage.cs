using System;
using System.Collections.Generic;
using System.Text;

namespace Home.Common.Messages
{
    public class SystemScriptExecuteMessage : BaseMessage
    {
        public const string ExecuteScriptTopic = "system.script.execute";
        public SystemScriptExecuteMessage(string messageTopic) : base(messageTopic)
        {
        }

        public string ScriptContent { get; set; }
    }
}
