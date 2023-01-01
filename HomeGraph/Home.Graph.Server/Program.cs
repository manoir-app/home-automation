using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Home.Common;
using Home.Common.Model;
using Home.Graph.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Home.Graph.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            MqttHelper.Start();

            string cn = Environment.GetEnvironmentVariable("APPCONFIG_CNSTRING");

            if (cn != null)
            {
                Console.WriteLine("with config = " + cn);
                ConfigurationSettingsHelper.Init(cn);
            }
            else
                Console.WriteLine("without config");

            var t = Environment.GetEnvironmentVariables();
            Console.WriteLine("Env vars : ");
            foreach(var k in t.Keys)
            {
                Console.Write(k.ToString().PadRight(35,' '));
                Console.Write(" : ");
                Console.WriteLine(t[k].ToString());
            }


            try
            {
                DateTime dt = System.IO.File.GetLastWriteTime(typeof(Program).Assembly.Location);
                Console.WriteLine($"Build date : {dt.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss")}");
                Console.WriteLine("-----------------------");
            }
            catch
            {

            }

            var host = CreateHostBuilder(args).Build();
            host.Run();

            MqttHelper.Stop();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
