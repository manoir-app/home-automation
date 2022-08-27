using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Home.Graph.Server.Controllers
{
    [Route("v1.0/settings"), Authorize(Roles = "Device,Admin,User,Agent")]
    [ApiController]
    public partial class GlobalSettingsController : ControllerBase
    {
    }
}
