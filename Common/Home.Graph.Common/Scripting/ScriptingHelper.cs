using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.Scripting.Hosting;
using Microsoft.CSharp.RuntimeBinder;
using MongoDB.Driver;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Dynamic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Home.Common.Messages;

namespace Home.Graph.Common.Scripting
{
    public static class ScriptingHelper
    {
        public class ScriptingGlobals
        {
            public ScriptingGlobals()
            {
                Mesh = new MeshProvider();
                Devices = new DeviceProvider();
                Entities = new EntityProvider();
            }

            public string AgentId { get; internal set; }

            public MeshProvider Mesh { get; set; }
            public DeviceProvider Devices { get; set; }
            public EntityProvider Entities { get; set; }
        }

        

        public static MessageResponse HandleMessage(string agentId, SystemScriptExecuteMessage systemScriptExecuteMessage)
        {
            try
            {
                Execute(systemScriptExecuteMessage.ScriptContent, agentId);
                return MessageResponse.OK;
            }
            catch(Exception e)
            {
                return MessageResponse.GenericFail;
            }
        }

        public class EntityProvider
        {
            public ExpandoObject this[string index]
            {
                get
                {
                    return null;
                }
            }
        }
        public static object Execute(string script, string agentId)
        {
            var tmp = new ScriptingGlobals
            {
                AgentId = agentId
            };

            StringBuilder realScript = new StringBuilder();
            realScript.AppendLine("using System;");
            //realScript.AppendLine("using System.Dynamic;");
            realScript.AppendLine("using System.Linq;");
            realScript.AppendLine("using System.Collections.Generic;");
            realScript.AppendLine("using System.Text;");
            realScript.AppendLine("using Home.Common;");
            realScript.AppendLine("using Home.Common.HomeAutomation;");
            realScript.AppendLine("using Home.Common.Model;");
            realScript.AppendLine("using Home.Common.Messages;");
            realScript.AppendLine("using Home.Graph.Common;");
            realScript.AppendLine("");
            realScript.AppendLine("");
            realScript.Append(script);

            ScriptOptions opt = ScriptOptions.Default;
            opt = opt.AddReferences(typeof(Object).Assembly);
            opt = opt.AddReferences(typeof(DynamicObject).Assembly);
            opt = opt.AddReferences(typeof(DynamicAttribute).Assembly);
            opt = opt.AddReferences(typeof(RuntimeBinderException).Assembly);
            opt = opt.AddReferences(typeof(Action).Assembly);
            opt = opt.AddReferences(typeof(Home.Common.Model.Agent).Assembly);
            opt = opt.AddReferences(typeof(ScriptingHelper).Assembly);

            var t = CSharpScript.Create(realScript.ToString(), globalsType: typeof(ScriptingGlobals), options: opt);

            var ret = t.RunAsync(tmp, e =>
            {
                Console.WriteLine(e);
                return false;
            }).Result;

            return ret.ReturnValue;
        }
    }
}
