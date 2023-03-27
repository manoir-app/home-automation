using Home.Common.Model;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Org.BouncyCastle.OpenSsl;
using System;
using System.Collections.Generic;
using System.Net;

namespace Home.Graph.Server.Controllers
{
    partial class AutomationMeshController
    {

        public class GetLogResponseStream
        {
            public string status { get; set; }
            public Data data { get; set; }
        }

        public class Data
        {
            public string resultType { get; set; }
            public StreamResult[] result { get; set; }
            public Stats stats { get; set; }
        }

        public class Stats
        {
            public Summary summary { get; set; }
            public Querier querier { get; set; }
            public Ingester ingester { get; set; }
            public Cache cache { get; set; }
        }

        public class Summary
        {
            public int bytesProcessedPerSecond { get; set; }
            public int linesProcessedPerSecond { get; set; }
            public int totalBytesProcessed { get; set; }
            public int totalLinesProcessed { get; set; }
            public float execTime { get; set; }
            public float queueTime { get; set; }
            public int subqueries { get; set; }
            public int totalEntriesReturned { get; set; }
        }

        public class Querier
        {
            public Store store { get; set; }
        }

        public class Store
        {
            public int totalChunksRef { get; set; }
            public int totalChunksDownloaded { get; set; }
            public int chunksDownloadTime { get; set; }
            public Chunk chunk { get; set; }
        }

        public class Chunk
        {
            public int headChunkBytes { get; set; }
            public int headChunkLines { get; set; }
            public int decompressedBytes { get; set; }
            public int decompressedLines { get; set; }
            public int compressedBytes { get; set; }
            public int totalDuplicates { get; set; }
        }

        public class Ingester
        {
            public int totalReached { get; set; }
            public int totalChunksMatched { get; set; }
            public int totalBatches { get; set; }
            public int totalLinesSent { get; set; }
            public Store store { get; set; }
        }

        public class Cache
        {
            public CacheChunk chunk { get; set; }
            public Index index { get; set; }
            public Result result { get; set; }
        }

        public class CacheChunk
        {
            public int entriesFound { get; set; }
            public int entriesRequested { get; set; }
            public int entriesStored { get; set; }
            public int bytesReceived { get; set; }
            public int bytesSent { get; set; }
            public int requests { get; set; }
        }

        public class Index
        {
            public int entriesFound { get; set; }
            public int entriesRequested { get; set; }
            public int entriesStored { get; set; }
            public int bytesReceived { get; set; }
            public int bytesSent { get; set; }
            public int requests { get; set; }
        }

        public class Result
        {
            public int entriesFound { get; set; }
            public int entriesRequested { get; set; }
            public int entriesStored { get; set; }
            public int bytesReceived { get; set; }
            public int bytesSent { get; set; }
            public int requests { get; set; }
        }

        public class StreamResult
        {
            public StreamResultDetail stream { get; set; }
            public string[][] values { get; set; }
        }

        public class StreamResultDetail
        {
            public string Application { get; set; }
            public string SourceId { get; set; }
            public string SourceKind { get; set; }
        }


        public class LogDataResult
        {
            public string Id { get; set; }
            public DateTimeOffset Date { get; set; }
            public string Source { get; set; }
            public string SourceId { get; set; }
            public string ImageUrl { get; set; }
            public string Message { get; set; }
            public string FormatedDate { get; set; }
        }

        [Route("local/logs")]
        public List<LogDataResult> GetLogs(string source = null, string sourceId = null, int durationInMinutes = 120, int maxCount = 250)
        {
            var ret = new List<LogDataResult>();


            using (WebClient cli = new WebClient())
            {
                long start = DateTimeOffset.Now.AddHours(-1).ToUnixTimeSeconds() * 1000000000; 
                long end = DateTimeOffset.Now.ToUnixTimeSeconds() * 1000000000;
                string port = Environment.GetEnvironmentVariable("LOKI_SERVICE_PORT");
                if (port == null)
                    port = "3100";
                string ip = Environment.GetEnvironmentVariable("LOKI_SERVICE_HOST");
                string url = $"http://{ip}:{port}/loki/api/v1/query_range?direction=BACKWARD&end={end}&limit=50&start={start}&query={{Application=%22manoir.app%22}}";

                var json = cli.DownloadString(url);
                ParseAndFillList(json, ret);
            }

            ret.Sort((a,b) => b.Date.CompareTo(a.Date));    

            return ret;
        }

        private class ParsedLogData
        {
            public string Message { get; set; }
            public string MessageTemplate { get; set; }
            public string Picture { get; set; }
            public string level { get; set; }

        }

        private void ParseAndFillList(string json, List<LogDataResult> ret)
        {
            var result = JsonConvert.DeserializeObject<GetLogResponseStream>(json);
            foreach(var resItem in result.data.result)
            {
                
                foreach (var log in resItem.values)
                {
                    LogData logData = new LogData();
                    var jsonITem = log.Length > 1 ? log[1] : null;
                    if (long.TryParse(log.Length > 0 ? log[0] : "", out long ts) && !string.IsNullOrEmpty(jsonITem))
                    {
                        DateTimeOffset dt = DateTimeOffset.FromUnixTimeSeconds(ts / 1000000000);
                        var ld = JsonConvert.DeserializeObject<ParsedLogData>(jsonITem);
                        if (ld != null)
                        {
                            ret.Add(new LogDataResult()
                            {
                                Message = ld.Message,
                                Id = ts.ToString(),
                                Date = dt,
                                ImageUrl = ld.Picture,
                                Source = resItem.stream.SourceKind,
                                SourceId = resItem.stream.SourceId,
                                FormatedDate = dt.ToString("HH:mm:ss")
                            });
                        }
                    }
                }
            }
        }
    }
}
