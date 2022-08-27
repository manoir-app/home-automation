using Home.Common;
using Microsoft.AspNetCore.Mvc;
using System;

namespace Home.Graph.Server.Controllers
{
    // ------------------------------------------------
    //  ! Attention, fichier partagé public/home graph
    // ------------------------------------------------
    [ApiController]
    public partial class DataRedirectionController : ControllerBase
    {

        public class RedirectionInfo
        {
            public bool IsPermanent { get; set; }
            public string RedirectionUrl { get; set; }
        }

        [Route("data/{entityType}/{entityId}")]
        public IActionResult GetEntityRedirection(string entityType, string entityId, bool noAutoRedirect = false)
        {
            if (string.IsNullOrEmpty(entityType)) throw new ArgumentException(entityType + " is invalid");
            if (string.IsNullOrEmpty(entityId)) throw new ArgumentException(entityId + " is invalid");
            RedirectionInfo ret;

            ret = GetRedirectionUrl(entityType, entityId);

            if (!noAutoRedirect && ret != null)
            {
                if (ret.IsPermanent)
                    return RedirectPermanent(ret.RedirectionUrl);
                else
                    return Redirect(ret.RedirectionUrl);
            }

            return new OkObjectResult(ret);
        }

        private static RedirectionInfo GetRedirectionUrl(string entityType, string entityId)
        {
            RedirectionInfo ret;
            switch (entityType)
            {
                case "user":
                    ret = new RedirectionInfo()
                    {
                        IsPermanent = true,
                        RedirectionUrl = HomeServerHelper.GetLocalGraphUrl($"me/users.htm?userId={entityId}")
                    };
                    break;
                case "container":
                    ret = new RedirectionInfo()
                    {
                        IsPermanent = true,
                        RedirectionUrl = HomeServerHelper.GetLocalGraphUrl($"me/stocks.htm?containerId={entityId}")
                    };
                    break;
                default:
                    ret = null;
                    break;
            }

            return ret;
        }

        [Route("products/container/{entityId}")]
        public IActionResult GetContainerRedirection(string entityId, bool noAutoRedirect = false)
        {
            if (string.IsNullOrEmpty(entityId)) throw new ArgumentException(entityId + " is invalid");
            RedirectionInfo ret;

            ret = GetRedirectionUrl("container", entityId);

            if (!noAutoRedirect && ret != null)
            {
                if (ret.IsPermanent)
                    return RedirectPermanent(ret.RedirectionUrl);
                else
                    return Redirect(ret.RedirectionUrl);
            }

            return new OkObjectResult(ret);
        }
    }
}
