using Microsoft.Extensions.Configuration;
using Home.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.AspNetCore.Mvc.ApplicationParts;

namespace Home.Agents.Alexandra
{
    class Program
    {
        internal static bool _stop = false;

        public static IHost WebHost = null;
        public static Thread tWebHost = null;

        static void Main(string[] args)
        {
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;

            AgentHelper.WriteStartupMessage("Alexandra", typeof(Program).Assembly);

            AgentHelper.SetupReporting("alexandra");
            AgentHelper.SetupLocaleFromServer("alexandra");
            AgentHelper.ReportStart("alexandra", "amazon", "alexa", "integration");
            
            AlexandraMessageHandler.Start();
            NewsBriefingService.Start();
            StartWebThread();

            while (!_stop)
            {
                Thread.Sleep(500);
                AgentHelper.Ping("alexandra");
            }
            AlexandraMessageHandler.Stop();
            NewsBriefingService.Stop();
            StopWebThread();
        }

        public static IHostBuilder CreateHostBuilder()
        {
            bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

            var t = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseUrls($"http://*:53000");
                    webBuilder.UseStartup<Startup>();
                });
            return t;
        }

        public static void StartWebThread()
        {
            if (WebHost != null)
                return;
            Thread t = new Thread(() =>
            {
                try
                {
                    WebHost = CreateHostBuilder().Build();
                    WebHost.Run();
                }
                catch
                {

                }
            });
            tWebHost = t;
            t.Start();
        }

        public static void StopWebThread()
        {
            if (tWebHost != null)
                tWebHost.Interrupt();
        }

        public class Startup
        {
            public Startup(IConfiguration configuration)
            {
                Configuration = configuration;
            }

            public IConfiguration Configuration { get; }

            // This method gets called by the runtime. Use this method to add services to the container.
            public void ConfigureServices(IServiceCollection services)
            {
                services.AddControllers();
            }

            // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
            public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
            {
                if (env.IsDevelopment())
                {
                    app.UseDeveloperExceptionPage();
                }

                app.UseRouting();

                app.UseDefaultFiles();
                app.UseStaticFiles();
                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                });
            }
        }


    }
}
   