using Home.Common.Model;
using Home.Graph.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using Home.Common.Messages;
using Home.Common;

namespace Home.Graph.Server.Controllers
{
    partial class AutomationMeshController
    {
        [Route("local/logs")]
        public List<LogData> GetLogs(string source = null, string sourceId = null, int durationInMinutes = 120, int maxCount = 250)
        {
            return new List<LogData>();
        }
    }
}
