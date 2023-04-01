using Home.Agents.Clara.Mails;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Home.Agents.Clara
{
    internal static class MailServiceThread
    {
        public static bool _stop = false;
        public static void Start()
        {
            var t = new Thread(() => MailServiceThread.Run());
            t.Name = "Schedule Thread";
            t.Start();
        }

        public static void Stop()
        {
            _stop = true;
        }

        private static DateTimeOffset _lastRefresh = DateTimeOffset.MinValue;

        private static void Run()
        {
            while (!_stop)
            {
                if (Math.Abs((DateTimeOffset.Now - _lastRefresh).TotalMinutes) > 2)
                {
                    MailHelper.CheckForNewMails();
                }
                Thread.Sleep(250);
            }
        }


    }
}
