using Home.Common;
using Home.Common.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Home.Agents.Erza
{
    public static partial class WeatherChecker
    {
        public static void Start()
        {
            var t = new Thread(() => WeatherChecker.RunCheck());
            t.Name = "Weather Checker";
            t.Start();
        }

        public static void Stop()
        {
            _stop = true;
        }


        public static bool _stop = false;
        public static DateTimeOffset? _offlineDate = null;

        private static Location _loc = null;
        public static void Reload()
        {
            _loc = AgentHelper.GetLocalMeshLocation("erza");
        }

        private static void RunCheck()
        {
            while (!_stop)
            {
                try
                {
                    DoCheck();
                }
                catch(Exception ex)
                {

                }

                Thread.Sleep(900000);
            }
        }

        internal static void DoCheck()
        {
            if (_loc == null)
                Reload();

            if (_loc != null)
            {
                new WeatherProviders.MeteoFranceWeatherHazards().Run(Graph.Common.TaskContextKind.Agent, "erza", _loc);              
                new WeatherProviders.MetNoWeather().Run(Graph.Common.TaskContextKind.Agent, "erza", _loc);              
            }
        }
    }
}
