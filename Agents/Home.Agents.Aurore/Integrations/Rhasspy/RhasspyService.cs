using Home.Common;
using Home.Common.Model;
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
    internal static partial class RhasspyService
    {
        private static string _mainRhasspyUrl = "http://192.168.2.128:12101/";


        public static void Start()
        {
            var t = new Thread(() => Run());
            t.Name = "Rhasspy";
            t.Start();
        }

        public static void Stop()
        {
            _stop = true;
        }


        public static bool _stop = false;

        public static void Run()
        {
            while(!_stop)
            {
                Thread.Sleep(100);
                if (DateTime.Now.Hour > 2)
                    continue;
                if (_lastUpdate >= DateTime.Today)
                    continue;

                FullRefresh();

                _lastUpdate = DateTime.Now;
            }
        }


        public static void FullRefresh()
        {
            ClearCache();
            SetupCache();
            RefreshSlots();
            RefreshSentences();
            Retrain();
        }

        private static void Retrain()
        {
            using (var cli = new WebClient())
            {
                cli.BaseAddress = _mainRhasspyUrl;
                cli.Headers.Add("Accept", "text/plain");
                cli.UploadString("/api/train", "POST", "");
            }
        }

      

       
    }
}
