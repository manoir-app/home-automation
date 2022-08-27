using Home.Common;
using Home.Common.Messages;
using Home.Common.Model;
using Home.Graph.Common;
using Home.Graph.Server.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Driver;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace Home.Graph.Server.Controllers
{
    [Route("v1.0/graphserver")]
    [ApiController]
    public class GraphToolsController : ControllerBase
    {
        private readonly IHubContext<SystemHub> _hubContext;

        public GraphToolsController(IHubContext<SystemHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public enum GraphToolCommandKind
        {
            AgentCommand,
            WebUiCommand
        }

        public class GraphToolCommand
        {
            public string InvokeWord { get; set; }

            public string Command { get; set; }

            public bool AcceptParameter { get; set; }

            public GraphToolCommandKind Kind { get; set; }

        }


        public class GraphToolCommandResult
        {
            public string InvokedWord { get; set; }
            public string Message { get; set; }
            public string UrlToRedirectTo { get; set; }
        }

        [Route("commands/search"), HttpGet]
        public List<GraphToolCommand> GetCommands(string prefix="")
        {
            return new List<GraphToolCommand>();
        }


    }
}
