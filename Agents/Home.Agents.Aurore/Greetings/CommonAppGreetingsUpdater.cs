using Home.Common;
using Home.Common.Model;
using Home.Journal.Common.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace Home.Agents.Aurore.Greetings
{
    internal partial class CommonAppGreetingsUpdater
    {
        public static void Start()
        {
            var t = new Thread(() => CommonAppGreetingsUpdater.Run());
            t.Name = "GreetingsUpdater";
            t.Start();
        }

        public static void Stop()
        {
            _stop = true;
        }


        public static bool _stop = false;
        public static DateTimeOffset _nextJournalUpdate = DateTime.MinValue;

        private static void Run()
        {
            while (!_stop)
            {
                if (DateTime.Now >= _nextJournalUpdate)
                    UpdateJournal();
                Thread.Sleep(500);
            }
        }
    }
}
