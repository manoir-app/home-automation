using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Home.Graph.Public.Controllers
{
    [ApiController, AllowAnonymous()]
    [Route("v1.0/Test")]
    public class PublicTestController : ControllerBase
    {
        [HttpGet]
        public string Get()
        {
            return "test";
        }
    }
}
