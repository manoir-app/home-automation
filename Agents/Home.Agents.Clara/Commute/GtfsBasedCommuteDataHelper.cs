using Home.Common;
using MongoDB.Driver.Core.Operations;
using SharpCompress.Archives.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Home.Agents.Clara.Commute
{
    internal partial class GtfsBasedCommuteDataHelper
    {
        private static string GetLocalFolder(string idPrefix, string type)
        {
            return Path.Combine(FileCacheHelper.GetLocalFolder("transport", idPrefix), type);
        }
        public static string DownloadRealtimeFilesToFolder(string idPrefix, string[] gtfsRtFileUrls)
        {
            var localFileName = FileCacheHelper.GetLocalFilename("transport", idPrefix, "gtfs.zip");
            string localDir = Path.Combine(Path.GetDirectoryName(localFileName), "gtfs-rt");

            if (Directory.Exists(localDir))
                Directory.Delete(localDir, true);
            Directory.CreateDirectory(localDir);

            return localDir;
        }

    }
}
