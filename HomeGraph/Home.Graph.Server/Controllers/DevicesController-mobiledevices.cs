using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Home.Common.Model;
using Home.Graph.Common;
using Home.Graph.Server.Hubs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace Home.Graph.Server.Controllers
{
    partial class DevicesController
    {
      

        public class DeviceApp
        {
            public string Url { get; set; }
        }

        [Route("all/{deviceId}/change-app"), HttpPost]
        public bool ChangeApp(string deviceId, [FromBody] DeviceApp app)
        {
            deviceId = deviceId.ToLowerInvariant();

            SystemHub.ChangeDeviceApp(_sysContext, deviceId, new SystemHub.DeviceAppInfo()
            {
                Url = app.Url
            });

            return true;
        }
    }
}
