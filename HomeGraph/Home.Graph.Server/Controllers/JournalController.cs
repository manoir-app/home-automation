using Home.Common.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System.Collections.Generic;
using System;
using Microsoft.AspNetCore.Authorization;

namespace Home.Graph.Server.Controllers
{
    [Route("v1.0/journal"), Authorize(Roles = "Agent,Device,User")]
    [ApiController]
    public partial class JournalController : ControllerBase
    {
       
    }
}
