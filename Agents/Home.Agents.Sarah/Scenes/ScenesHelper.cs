using Home.Common;
using Home.Common.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Home.Agents.Sarah.Scenes
{
    public static partial class ScenesHelper
    {
        private static List<SceneGroup> _Groups = null;
        private static List<Scene> _Scenes = null;

        private static DateTimeOffset _lastRefresh = DateTime.MinValue;

        public static bool _stop = false;
        public static void Start()
        {
            var t = new Thread(() => ScenesHelper.Run());
            t.Name = "Scenes refresh";
            t.Start();
        }

        private static void Run()
        {
            while (!_stop)
            {
                try
                {
                    if((DateTimeOffset.Now-_lastRefresh).TotalMinutes>2)
                    {
                        RefreshCache();
                        Console.WriteLine("Scenes - refreshed");
                        _lastRefresh = DateTimeOffset.Now;
                    }
                    Thread.Sleep(1000);
                }
                catch (Exception ex)
                {
                    Console.Write("Scene - ERR in SceneThread - ");
                    Console.WriteLine(ex.ToString());
                }
            }
        }

        internal static void RefreshCache()
        {
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    using (var cli = new MainApiAgentWebClient("sarah"))
                    {
                        var exts = cli.DownloadData<List<SceneGroup>>(
                            $"/v1.0/homeautomation/scenes/groups");
                        _Groups = exts;
                        break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }

            }

            for (int i = 0; i < 3; i++)
            {
                try
                {
                    using (var cli = new MainApiAgentWebClient("sarah"))
                    {
                        var exts = cli.DownloadData<List<Scene>>(
                            $"/v1.0/homeautomation/scenes/scenes");
                        _Scenes = exts;
                        break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }

            }

            _lastRefresh = DateTimeOffset.Now;
        }

        public static void Stop()
        {
            _stop = true;
        }


    }
}
