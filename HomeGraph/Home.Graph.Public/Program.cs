using Home.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Home.Graph.Public
{
    public class Program
    {
        public static void Main(string[] args)
        {

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
            foreach (var k in t.Keys)
            {
                Console.Write(k.ToString().PadRight(35, ' '));
                Console.Write(" : ");
                Console.WriteLine(t[k].ToString());
            }

            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
