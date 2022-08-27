using Home.Graph.Server.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Home.Graph.Server.Controllers
{
    [Route("v1.0/todos"), Authorize(Roles = "User,Admin,Device,Agent")]
    [ApiController]
    public partial class TodosController : ControllerBase
    {
        private readonly IHubContext<AdminToolsHub> _hubContext;
        private readonly IHubContext<AppAndDeviceHub> _appContext;

        public TodosController(IHubContext<AdminToolsHub> hubContext, IHubContext<AppAndDeviceHub> appContext)
        {
            _hubContext = hubContext;
            _appContext = appContext;
        }
    }
}
