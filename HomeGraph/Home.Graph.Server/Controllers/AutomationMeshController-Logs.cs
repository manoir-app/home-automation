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
            var coll = MongoDbHelper.GetClient<LogData>();

            var bldr = Builders<LogData>.Filter.Gte<DateTimeOffset?>("Date", DateTimeOffset.Now.AddMinutes(0 - Math.Abs(durationInMinutes)));

            if (!string.IsNullOrEmpty(source))
                bldr &= Builders<LogData>.Filter.Eq("Source", source);
            if (!string.IsNullOrEmpty(sourceId))
                bldr &= Builders<LogData>.Filter.Eq("SourceId", sourceId);

            var sort = Builders<LogData>.Sort.Descending("Date");


            return coll.Find(bldr).Sort(sort).Limit(maxCount).ToList();
        }
    }
}
