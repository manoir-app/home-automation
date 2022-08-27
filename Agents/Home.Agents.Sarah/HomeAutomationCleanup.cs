using Home.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Home.Agents.Sarah
{
    public class HomeAutomationCleanup
    {


        public static void Start()
        {
            var t = new Thread(() => HomeAutomationCleanup.Run());
            t.Name = "Cleanup";
            t.Start();
        }


        public static void Stop()
        {
            _stop = true;
        }

        public static bool _stop = false;

        private static void Run()
        {
            DateTime last = DateTime.Now;
            while (!_stop)
            {
                try
                {
                    Thread.Sleep(100);

                    if (last.AddMinutes(5) < DateTime.Now)
                    {
                        last = DateTime.Now;
                        using (var cli = new MainApiAgentWebClient("sarah"))
                        {
                            var isDone = cli.DownloadData<bool>("v1.0/devices/discovered/clearolds");
                            Console.WriteLine("Old discovered device cleaned");
                        }
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }

    }
}
