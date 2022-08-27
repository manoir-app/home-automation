using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Plex.Api;
using Plex.Api.Api;
using Plex.Api.Models;
using Plex.Api.Models.Friends;
using Plex.Api.Models.Server;
using Plex.Api.Models.Status;
using System;
using System.Collections.Generic;
using System.Text;

namespace Home.Agents.Sarah.Devices.Media
{

    
    public class PlexServer 
    {
        public void Test()
        {
            var cli = new ClientOptions()
            {
                Product = "API_UnitTests",
                DeviceName = "API_UnitTests",
                ClientId = "MyClientId",
                Platform = "Web",
                Version = "v1"
            };


            IPlexClient plexApi = new PlexClient(cli, new ApiService(new PlexRequestsHttpClient(), NullLogger<ApiService>.Instance));
            
            var user = plexApi
                .SignIn("*****", "******").Result;


            var srv = "http://crimson:32400";
            // Get Servers
            //var servers = plexApi.GetServers(user.AuthenticationToken).Result;
            var allLibs = plexApi.GetLibraries(user.AuthenticationToken, srv).Result;
            foreach(var libDir in allLibs.MediaContainer.Directory)
            {
                var colls = plexApi.GetMetadataForLibrary(user.AuthenticationToken, srv, libDir.Key).Result;
                foreach(var col in colls.MediaContainer.Metadata)
                {
                    Console.WriteLine($"{col.Title} : {col.Rating}");
                }
            }
        }
    }
}
