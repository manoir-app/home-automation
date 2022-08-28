using Home.Common;
using Home.Common.Model;
using System;

namespace Home.Graph.Common
{
    public static class LogHelper
    {
        public static void Log(LogData l)
        {
            try
            {
                var coll = MongoDbHelper.GetClient<LogData>();
                l.Id = Guid.NewGuid().ToString();
                l.Date = DateTimeOffset.Now;
                coll.InsertOne(l);
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
