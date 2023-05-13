using Home.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YamlDotNet.Serialization.TypeResolvers;

namespace Home.Agents.Aurore.Integrations.Rhasspy
{
    partial class RhasspyService
    {
        private static void RefreshSentences()
        {

            Dictionary<string, string[]> sentences = new Dictionary<string, string[]>();

            sentences.Merge(GetConversationsSentences());
            sentences.Merge(GetPlaySentences());
            sentences.Merge(GetBasicHomeAutomationSentences());
            sentences.Merge(GetPresenceSentences());
            sentences.Merge(GetSceneSentences());
            sentences.Merge(GetShoppingListSentences());

            using (var cli = new WebClient())
            {
                cli.BaseAddress = _mainRhasspyUrl;
                cli.Headers.Add("Content-Type", "text/plain");
                cli.Headers.Add("Accept", "text/plain");
                cli.UploadString("/api/sentences", "POST", ConvertToSentencesIni(sentences));
            }

        }

        private static Dictionary<string, string[]> GetConversationsSentences()
        {
            var ret = new Dictionary<string, string[]>();

            ret.Add("Cancel", new string[] {
                "annule",
                "annuler",
                "laisse tomber",
                "pas la peine",
                "arrête",
                "stop",
                "cancel",
                "fais pas chier"
            });

            ret.Add("Yes", new string[] {
                "oui",
                "affirmatif",
                "bien sûr",
            });

            ret.Add("No", new string[] {
                "non",
                "pas du tout",
                "surtout pas",
            });

            ret.Add("Agents:Aurore:Greetings", new string[] {
                "(Hello | Salut | Bonjour | Coucou)",
            });


            ret.Add("Agents:Aurore:NewsBriefing", new string[] {
                "quelles sont les (nouvelles | news)",
                "quoi de neuf",
                "(démarre | lance) [mon] (briefing | récap)",
            });

            ret.Add("Agents:Aurore:TellTime", new string[] {
                "quelle heure est-il",
                "quelle heure il est",
                "(donne | dit) [moi] (quelle | l') heure [il est]",
            });

            ret.Add("Agents:Aurore:TellDate", new string[] {
                "quelle date est-il",
                "quelle jour est-il",
                "quelle jour sommes nous",
                "quelle date sommes nous",
                "quelle est la date [du] [(jour | aujourd'hui)]",
                "(donne | dit) [moi] (quelle) est la date [il est] [du] [(jour | aujourd'hui)]",
            });

            return ret;
        }

        private static string ConvertToSentencesIni(Dictionary<string, string[]> sentences)
        {

            StringBuilder blr = new StringBuilder();

            foreach (var k in sentences.Keys)
            {
                var items = sentences[k];
                blr.Append("[");
                blr.Append(k);
                blr.AppendLine("]");

                foreach (var item in items)
                {
                    if (item.StartsWith("["))
                        blr.Append("\\");
                    blr.AppendLine(item);
                }

                blr.AppendLine();
            }

            return blr.ToString();
        }

        public static void Merge(this Dictionary<string, string[]> source, Dictionary<string, string[]> toMerge)
        {
            if (toMerge == null || toMerge.Count == 0)
                return;

            foreach (var k in toMerge.Keys)
            {
                var newItems = toMerge[k];

                string[] tmp = null;
                if (!source.TryGetValue(k, out tmp))
                    source.Add(k, newItems);
                else
                {
                    string[] full = new string[tmp.Length + newItems.Length];
                    tmp.CopyTo(full, 0);
                    newItems.CopyTo(full, tmp.Length);
                    source[k] = full;
                }
            }
        }

        private static Dictionary<string, string[]> GetPlaySentences()
        {
            var ret = new Dictionary<string, string[]>();

            ret.Add("Agents:Aurore:Games:Marco", new string[] { "Marco" });


            ret.Add("Agents:Aurore:Games:Jokes", new string[] { 
                "(Raconte | sors | dis) [(moi | nous)] [une] blague",
            });

            return ret;
        }

        private static Dictionary<string, string[]> GetShoppingListSentences()
        {
            var ret = new Dictionary<string, string[]>();

            ret.Add("Agents:Erina:ShoppingList:AddItem", new string[] {
                "ajoute [(du | de | des)] [la] ($manoir/products) (à | dans) [la | ma] liste de courses",
                "[(je | nous)] [(vais | allons)] avoir besoin [acheter] [plus] [(du | de | des)] [la] ($manoir/products)",
                "(rappelle moi | fais moi penser à) acheter [plus] [(du | de | des)] [la] ($manoir/products)",
            });

            return ret;
        }

        private static Dictionary<string, string[]> GetBasicHomeAutomationSentences()
        {
            var ret = new Dictionary<string, string[]>();

            ret.Add("Agents:Sarah:SwitchOff:ByRoom", new string[] {
                "(éteindre|éteins|ferme|fermer) [toutes] les (lumières|lampes){devicetype:LIGHTS} (de | du) ($manoir/rooms)",
                "peux tu éteindre [toutes] [les (lumières|lampes)] :{devicetype:LIGHTS} [(de | du)] ($manoir/rooms)",
                "peux tu fermer [tous] les volets:{devicetype:SHUTTERS} [(de | du)] ($manoir/rooms)",
            });

            ret.Add("Agents:Sarah:SwitchOff:ByZone", new string[] {
                "(éteindre|éteins|ferme|fermer) [toutes] les (lumières|lampes){devicetype:LIGHTS} (de | du) ($manoir/zones)",
                "peux tu éteindre [toutes] [les (lumières|lampes)] :{devicetype:LIGHTS} [(de | du)] ($manoir/zones)",
                "peux tu fermer [tous] les volets:{devicetype:SHUTTERS} [(de | du)] ($manoir/zones)",
            });

            ret.Add("Agents:Sarah:SwitchOn:ByRoom", new string[] {
                "(allumer|allumes|ouvre|ouvrir) [toutes] les (lumières|lampes){devicetype:LIGHTS} (de | du) ($manoir/rooms)",
                "peux tu allumer [toutes] [les (lumières|lampes)] :{devicetype:LIGHTS} [(de | du)] ($manoir/rooms)",
                "peux tu ouvrir [tous] les volets:{devicetype:SHUTTERS} [(de | du)] ($manoir/rooms)",
            });

            ret.Add("Agents:Sarah:SwitchOn:ByZone", new string[] {
                "(allumer|allumes|ouvre|ouvrir) [toutes] les (lumières|lampes){devicetype:LIGHTS} (de | du) ($manoir/zones)",
                "peux tu allumer [toutes] [les (lumières|lampes)] :{devicetype:LIGHTS} [(de | du)] ($manoir/zones)",
                "peux tu ouvrir [tous] les volets:{devicetype:SHUTTERS} [(de | du)] ($manoir/zones)",
            });

            return ret;
        }

        private static Dictionary<string, string[]> GetPresenceSentences()
        {
            var ret = new Dictionary<string, string[]>();

            ret.Add("Presence:User:CheckIn", new string[] {
                "($manoir/users) (est | vient) [d'] (arrivé|arriver)",
            });

            ret.Add("Presence:User:CheckOut", new string[] {
                "($manoir/users) (va | est en train) [de] (partir|s'en aller) :{when:SOON} [de] [la] [maison]",
                "($manoir/users) (vient | est) [de] (partir|s'en aller|parti) :{when:DONE} [de] [la] [maison]",
            });

            ret.Add("Presence:Guests:Checkin", new string[] {
                "nous avons des invités",
                "(les|mes|nos) invités sont arrivés",
            });

            ret.Add("Presence:Guests:CheckOut", new string[] {
                "(les|mes|nos) invités sont [tous] partis",
                "(le | les) dernier (de | des) (les|mes|nos) invités (est | sont) parti",
            });

            return ret;
        }

        private static Dictionary<string, string[]> GetSceneSentences()
        {
            var ret = new Dictionary<string, string[]>();

            ret.Add("Agents:Sarah:Scene", new string[] {
                "(exécute | active) [le] [scénario] ($manoir/scenes)",
            });

            SetupCache();

            foreach (var scene in _allScenes)
            {
                if (scene.InvocationStrings != null && scene.InvocationStrings.Count > 0)
                {
                    ret.Add("Agents:Sarah:Scene:" + scene.Id,
                    scene.InvocationStrings.ToArray());
                }
            }

            return ret;
        }
    }
}
