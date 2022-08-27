using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Home.Graph.Common;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
namespace Home.Graph.Server
{
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
            services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new TimeSpanConverter());
                options.JsonSerializerOptions.Converters.Add(new TimeSpanNullableConverter());
            });
            
            services.AddSignalR(options =>
            {

            });
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "multiple";
                options.DefaultChallengeScheme = "multiple";
            })
                .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("BasicAuthentication", null)
                .AddScheme<AuthenticationSchemeOptions, DeviceTokenAuthenticationHandler>("BearerAuthentication", null)
                .AddCookie()
                .AddPolicyScheme("multiple", "Use multiple", options =>
                {
                    options.ForwardDefaultSelector = context =>
                    {
                        var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
                        if (authHeader != null)
                        {
                            if (authHeader.StartsWith("Basic "))
                                return "BasicAuthentication";
                            else if (authHeader.StartsWith("Bearer "))
                                return "BearerAuthentication";
                        }
                        return CookieAuthenticationDefaults.AuthenticationScheme;
                    };
                })
                ;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseDefaultFiles();

            app.UseStaticFiles();

            app.UseCors();

            var filesAdmin = new PhysicalFileProvider(Path.Combine(env.ContentRootPath, "adminroot"));
            app.Map("/admin", b =>
            {
                b.UseAuthentication();
                b.UseDefaultFiles();
                b.UseStaticFiles(new StaticFileOptions()
                {
                    FileProvider = filesAdmin,
                    OnPrepareResponse = (context) =>
                    {
                        var pth = context.File.PhysicalPath;
                        var ext = Path.GetExtension(pth);
                        if (string.IsNullOrEmpty(ext))
                            ext = ".";
                        else
                            ext = ext.ToLowerInvariant();
                        switch (ext)
                        {
                            case ".html":
                            case ".htm":
                            case ".aspx":
                            case ".asp":
                                context.Context.Response.Headers.Append("Cache-Control", "no-cache");
                                break;
                        }

                        if (!context.Context.User.Identity.IsAuthenticated)
                        {
                            context.Context.Response.Redirect("/login.html?return=me");
                        }
                        else
                        {
                            var clm = context.Context.User.Claims.Where(c => c.Type.Equals(ClaimTypes.Role)).FirstOrDefault();
                            if (clm == null || string.IsNullOrEmpty(clm.Value) || !clm.Value.Equals("admin", StringComparison.InvariantCultureIgnoreCase))
                            {
                                context.Context.Response.Redirect("/");
                            }
                        }
                    }
                });
            });

            var filesUser= new PhysicalFileProvider(Path.Combine(env.ContentRootPath, "userroot"));
            app.Map("/me", b =>
            {
                b.UseAuthentication();
                b.UseDefaultFiles();
                b.UseStaticFiles(new StaticFileOptions()
                {
                    FileProvider = filesUser,
                    OnPrepareResponse = (context) =>
                    {
                        var pth = context.File.PhysicalPath;
                        var ext = Path.GetExtension(pth);
                        if (string.IsNullOrEmpty(ext))
                            ext = ".";
                        else
                            ext = ext.ToLowerInvariant();
                        switch (ext)
                        {
                            case ".html":
                            case ".htm":
                            case ".aspx":
                            case ".asp":
                                context.Context.Response.Headers.Append("Cache-Control", "no-cache");
                                break;
                        }

                        if (!context.Context.User.Identity.IsAuthenticated)
                        {
                            context.Context.Response.Redirect("/login.html?return=me");
                        }
                        else
                        {
                            var clm = context.Context.User.Claims.Where(c => c.Type.Equals(ClaimTypes.Role)).FirstOrDefault();
                            if (clm == null || string.IsNullOrEmpty(clm.Value) 
                                || (!clm.Value.Equals("admin", StringComparison.InvariantCultureIgnoreCase)
                                        && !clm.Value.Equals("user", StringComparison.InvariantCultureIgnoreCase)))
                            {
                                context.Context.Response.Redirect("/");
                            }
                        }
                    }
                });
            });


            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<Hubs.SystemHub>("/hubs/1.0/system");
                endpoints.MapHub<Hubs.AppAndDeviceHub>("/hubs/1.0/appanddevices");
                endpoints.MapHub<Hubs.AdminToolsHub>("/hubs/1.0/admin");
            });
        }
    }
}
