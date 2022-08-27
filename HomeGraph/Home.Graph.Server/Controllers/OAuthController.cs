using Home.Common;
using Home.Common.Model;
using Home.Graph.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Home.Graph.Server.Controllers
{
    [Route("oauth")]
    [ApiController]
    public partial class OAuthController : ControllerBase
    {

    }
}
