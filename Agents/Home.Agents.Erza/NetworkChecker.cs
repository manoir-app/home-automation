using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;

namespace Home.Agents.Erza
{
    public static partial class NetworkChecker
    {
        

        public static void Start()
        {
            var t = new Thread(() => NetworkChecker.Do());
            t.Name = "Wan Checker";
            t.Start();
        }

        private static bool _stop = false;

        public static void Stop()
        {
            _stop = true;
        }

        private static void Do()
        {
            while (!_stop)
            {
                for (int i = 0; i < 60 && !_stop; i++)
                    Thread.Sleep(500);
                if (_stop)
                    return;

                try
                {
                    RunOfflineCheck();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }

                try
                {
                    RunPublicServerCheck();
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }
    }
}
