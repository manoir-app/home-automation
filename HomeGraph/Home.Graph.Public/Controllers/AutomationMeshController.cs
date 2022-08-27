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
    [Route("v1.0/system/mesh")]
    [ApiController]
    public class AutomationMeshController : ControllerBase
    {
        [Route("local/public/version"), HttpGet(), AllowAnonymous()]
        public string GetVersion()
        {
            var asm = typeof(AutomationMeshController).Assembly;
            var pth = new FileInfo(asm.Location);
            return pth.LastWriteTime.ToString("yy.MMdd.HHmm");
        }

        [Route("local/graph/version"), HttpGet(), AllowAnonymous()]
        public string GetGraphVersion()
        {
            string url = $"http://{GetLocalIp()}/v1.0/system/mesh/local/graph/version";

            // on demande comme si on était Gaïa
            using(var cli = new MainApiAgentWebClient("gaia"))
            {
                var s = cli.DownloadString(url);
                return s;
            }
        }


        [Route("local/graph/check"), HttpGet(), AllowAnonymous()]
        public string CheckIfExists()
        {
            return "dev.carbenay.info:home-automation:proxy";
        }

        [Route("local/graph/localip"), HttpGet(), AllowAnonymous()]
        public string GetLocalIp()
        {
            var srv = HomeServerHelper.GetLocalIP();
            return srv;
        }
    }
}
