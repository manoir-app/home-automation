using Home.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;


namespace Home.Graph.Public.Controllers
{
    [Route("v1.0/proxy/alexa")]
    [ApiController]
    public class AlexaProxyController : ControllerBase
    {
        [Route("news-briefing")]
        public IActionResult GetNewsBriefing()
        {
            var ok = new ContentResult();
            var file = FileCacheHelper.GetLocalFilename("proxy", "alexa/news-briefing", "home.json");
            if (System.IO.File.Exists(file))
            {
                ok.StatusCode = 200;
                ok.ContentType = "application/json";
                ok.Content = System.IO.File.ReadAllText(file);
                return ok;
            }
            else
            {
                return new NotFoundResult();
            }
        }
    }
}
