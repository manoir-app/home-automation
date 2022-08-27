using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Home.Common.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Home.Common;

namespace Home.Graph.Public.Controllers
{
    [Route("v1.0/services/files")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        [Route("{type}/{folder}/{folder2}/{folder3}/{folder4}/{file}"), HttpGet]
        public IActionResult GetFile(string type, string folder, string folder2, string folder3, string folder4, string file)
        {
            return GetFile(type, folder + "/" + folder2 + "/" + folder3 + "/" + folder4, file);
        }

        [Route("{type}/{folder}/{folder2}/{folder3}/{file}"), HttpGet]
        public IActionResult GetFile(string type, string folder, string folder2, string folder3, string file)
        {
            return GetFile(type, folder + "/" + folder2 + "/" + folder3, file);
        }

        [Route("{type}/{folder}/{folder2}/{file}"), HttpGet]
        public IActionResult GetFile(string type, string folder, string folder2, string file)
        {
            return GetFile(type, folder + "/" + folder2, file);
        }

        // en dur pour l'instant
        private static string[] _allowedRoots =
        {
            "users/avatars/",
            "devices/images/",
            "home-automation/images/",
            "common/images/"
        };

        [Route("{type}/{folder}/{file}"), HttpGet]
        public IActionResult GetFile(string type, string folder, string file)
        {
            string url = type + "/" + folder + "/";
            url = url.ToLowerInvariant();
            bool found = false;
            foreach (var r in _allowedRoots)
            {
                if (url.StartsWith(r, StringComparison.InvariantCultureIgnoreCase))
                {
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                Console.WriteLine("Access denied to " + url);
                return new NotFoundResult();
            }

            string localFile = FileCacheHelper.GetLocalFilename(type, folder, file);
            if (!System.IO.File.Exists(localFile))
            {
                Console.WriteLine("File not found " + localFile);
                return new NotFoundResult();
            }

            string contentType = FileCacheHelper.GetContentType(localFile);

            var t = this.PhysicalFile(localFile, contentType);
            return t;
        }
    }
}
