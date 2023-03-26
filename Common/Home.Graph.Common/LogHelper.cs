using Home.Common;
using Home.Common.Model;
using Serilog;
using System;
using System.Net;
using System.Collections.Generic;
using Serilog.Sinks.Grafana.Loki;
using Serilog.Context;

namespace Home.Graph.Common
{
    public static class LogHelper
    {


        static ILogger _log = null;
        static LogHelper()
        {
            string port = Environment.GetEnvironmentVariable("LOKI_SERVICE_PORT");
            if (port == null)
                port = "3100";
            string ip = Environment.GetEnvironmentVariable("LOKI_SERVICE_HOST");
            
            if (port != null && ip != null)
            {
                _log = new LoggerConfiguration()
                      //.MinimumLevel.Debug()
                      .Enrich.FromLogContext()
                      //.Enrich.With<HttpContextEnricher>()
                      .WriteTo.GrafanaLoki(
                            $"http://{ip}:{port}",
                            propertiesAsLabels: new string[] { "SourceKind", "SourceId" }
                      )
                      .CreateLogger();
            }
        }

        public static void Log(LogData l)
        {
            Console.WriteLine(l.Message);

            if (_log == null)
                return;

            try
            {
                using (LogContext.PushProperty("SourceKind", l.Source))
                using (LogContext.PushProperty("SourceId", l.SourceId))
                using (LogContext.PushProperty("Picture", l.ImageUrl))
                    _log.Information(l.Message);
            }
            catch
            {
                // il s'agit de log, tant pis si on arrive
                // pas à le stocker.
            }
        }

        public static void Log(string source, string sourceId, string message)
        {
            LogData ld = new LogData()
            {
                Source = source,
                SourceId = sourceId,
                ImageUrl = GetImageUrlForSource(source, sourceId),
                Message = message
            };
            Log(ld);
        }

        public static void Log(string source, string sourceId, string urlImage, string message)
        {
            LogData ld = new LogData()
            {
                Source = source,
                SourceId = sourceId,
                ImageUrl = urlImage,
                Message = message
            };
            Log(ld);
        }

        public static string GetImageUrl(string imageCode)
        {
            return HomeServerHelper.GetPublicGraphUrl($"/v1.0/services/files/common/images/{imageCode}.png");
        }

        public static string GetImageUrlForSource(string source, string sourceId)
        {
            return GetImageUrl(GetImageNameForSource(source, sourceId));
        }


        public static string GetImageNameForSource(string source, string sourceId)
        {
            if (source != null)
            {
                switch (source.ToLowerInvariant())
                {
                    case "mesh":
                        return "log-home";
                    case "user":
                        if (!string.IsNullOrEmpty(sourceId))
                        {
                            switch (sourceId.ToLowerInvariant())
                            {
                                case "presence":
                                    return "log-home-user";
                            }
                        }
                        return "log-home";
                    case "agents":
                    case "agent":
                        if (!string.IsNullOrEmpty(sourceId))
                        {
                            switch (sourceId.ToLowerInvariant())
                            {
                                case "agents-gaia":
                                case "gaia":
                                    return "log-stats";
                                case "erza":
                                    return "log-home";
                                case "sarah":
                                    return "log-home";
                            }
                        }
                        break;
                }
            }
            return "log-default";
        }
    }
}
