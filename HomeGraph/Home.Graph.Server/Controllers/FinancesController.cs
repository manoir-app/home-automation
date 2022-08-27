using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Home.Graph.Server.Controllers
{
    [Route("v1.0/pim/finances"), Authorize(Roles = "Admin,User,Agent,Device")]
    [ApiController]
    public partial class FinancesController : ControllerBase
    {
        
    }
}
