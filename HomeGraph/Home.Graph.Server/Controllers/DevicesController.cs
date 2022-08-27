using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Home.Common.Model;
using Home.Graph.Common;
using Home.Graph.Server.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Driver;

namespace Home.Graph.Server.Controllers
{
    [Route("v1.0/devices"), Authorize(Roles = "Device,Admin,Agent")]
    [ApiController]
    public partial class DevicesController : ControllerBase
    {
        private readonly IHubContext<AdminToolsHub> _adminContext;
        private readonly IHubContext<AppAndDeviceHub> _appContext;
        private readonly IHubContext<SystemHub> _sysContext;

        public DevicesController(IHubContext<AdminToolsHub> adminContext, IHubContext<AppAndDeviceHub> appContext, IHubContext<SystemHub> sysContext)
        {
            _adminContext = adminContext;
            _appContext = appContext;
            _sysContext = sysContext;   
        }


    }
}
