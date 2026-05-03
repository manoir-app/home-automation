using Home.Agents.Aurore.Greetings;
using Home.Agents.Aurore.Integrations;
using Home.Agents.Aurore.Integrations.Rhasspy;
using Home.Agents.Aurore.Llm;
using Home.Common;
using Home.Graph.Common;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Home.Agents.Aurore
{
    class Program
    {
        internal static bool _stop = false;

        static void Main(string[] args)
        {
            AgentHelper.WriteStartupMessage("Aurore", typeof(Program).Assembly);

#if DEBUG
            AuroreLlmService aurore = new AuroreLlmService();
            var t = aurore.ChatWithToolsAsync("Quelle est la météo aujourd'hui à Paris ?").GetAwaiter().GetResult();
            return;
#endif

            AgentHelper.SetupReporting("aurore");
            AgentHelper.SetupLocaleFromServer("aurore");

            AgentHelper.ReportStart("aurore", "user-interaction");

            MqttHelper.Start("agents-aurore");
            AuroreMessageHandler.Start();
            AuroreNewsItemService.Start();
            CommonAppGreetingsUpdater.Start();
            
            while (!_stop)
            {
                Thread.Sleep(500);
                AgentHelper.Ping("aurore");
            }
            
            CommonAppGreetingsUpdater.Stop();
            AuroreNewsItemService.Stop();
            AuroreMessageHandler.Stop();
            MqttHelper.Stop();
        }

        private static void CopyCommonFiles(string root, string folder)
        {
            if(!root.EndsWith("/"))
                root = root + "/";

            foreach(var sub in Directory.GetDirectories(folder))
                CopyCommonFiles(root, sub);

            foreach(var t in Directory.GetFiles(folder))
            {
                string dest = "/home-automation/files/cache/" + t.Substring(root.Length);
                var dir = Path.GetDirectoryName(dest);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                File.Copy(t, dest, true);
            }
        }
    }
}
