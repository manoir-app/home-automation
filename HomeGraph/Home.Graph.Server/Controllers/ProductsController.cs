using Home.Graph.Server.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace Home.Graph.Server.Controllers
{
    [Route("v1.0/products"), Authorize(Roles = "Device,Admin,User,Agent")]
    [ApiController]
    public partial class ProductsController : ControllerBase
    {
        private readonly IHubContext<AdminToolsHub> _adminContext;
        private readonly IHubContext<AppAndDeviceHub> _appContext;
        private readonly IHubContext<SystemHub> _sysContext;

        public ProductsController(IHubContext<AdminToolsHub> adminContext, IHubContext<AppAndDeviceHub> appContext, IHubContext<SystemHub> sysContext)
        {
            _adminContext = adminContext;
            _appContext = appContext;
            _sysContext = sysContext;
        }

    }
}
