using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Home.Common
{
    public static class FileCacheHelper
    {
        public static string GetLocalFilename(string type, string folder, string file)
        {
            var path = GetLocalFolder(type, folder);
            if (string.IsNullOrEmpty(path))
                return null;

            return Path.Combine(path, file);
        }

        public static string GetRootPath()
        {
            var pth = Environment.GetEnvironmentVariable("FILE_CACHE_FOLDER");

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                if (Directory.Exists("/home-automation/"))
                {
                    pth = Path.Combine("/home-automation/", "files/cache");
                    if (!Directory.Exists(pth))
                        Directory.CreateDirectory(pth);
                    return pth;
                }

            }
            if (pth == null)
                pth = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "home-automation");
            if (!Directory.Exists(pth))
                Directory.CreateDirectory(pth);
            return pth;
        }

        public static string GetLocalFolder(string type, string folder)
        {
            var pth = GetRootPath();
            pth = Path.Combine(pth, type, folder);
            if (!Directory.Exists(pth))
                Directory.CreateDirectory(pth);
            return pth;
        }

        public static string GetContentType(string localFile)
        {
            string ext = Path.GetExtension(localFile);
            switch(ext.ToLowerInvariant())
            {
                case ".png":
                    return "image/png";
                case ".jpg":
                case ".jpeg":
                    return "image/jpg";
            }

            return "application/octet-stream";
        }
    }

}
