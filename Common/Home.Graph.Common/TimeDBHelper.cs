using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Home.Graph.Common
{
    public static class TimeDBHelper
    {
        private static InfluxDBClient _client = null;
        private static InfluxDBClient GetClient()
        {

            if (_client != null)
                return _client;

            var srv = Environment.GetEnvironmentVariable("INFLUXDB_SERVICE_HOST");
            var port = Environment.GetEnvironmentVariable("INFLUXDB_SERVICE_PORT");
            var passwordToGet = LocalDebugHelper.GetApiKey();


            _client = new InfluxDBClient($"http://{srv}:{port}", "manoir-app", passwordToGet);

            return _client;
        }

        public static void Trace(string name, decimal value, Dictionary<string, string> tags)
        {
            var client = GetClient();
            using (var writeApi = client.GetWriteApi())
            {
                //
                // Write by Point
                //
                var point = PointData.Measurement(name)
                    .Field("value", value)
                    .Timestamp(DateTime.UtcNow.AddSeconds(-1), WritePrecision.Ns);

                if(tags!=null)
                {
                    foreach(var k in tags.Keys)
                        point = point.Tag(k, tags[k]);
                }

                writeApi.WritePoint(point, "home-automation", "manoir-app");
            }
        }

        public static void Trace(string name, bool value, Dictionary<string, string> tags)
        {
            var client = GetClient();
            using (var writeApi = client.GetWriteApi())
            {
                //
                // Write by Point
                //
                var point = PointData.Measurement(name)
                    .Field("value", value)
                    .Timestamp(DateTime.UtcNow.AddSeconds(-1), WritePrecision.Ns);

                if (tags != null)
                {
                    foreach (var k in tags.Keys)
                        point = point.Tag(k, tags[k]);
                }

                writeApi.WritePoint(point, "home-automation", "manoir-app");
            }
        }

    }
}
