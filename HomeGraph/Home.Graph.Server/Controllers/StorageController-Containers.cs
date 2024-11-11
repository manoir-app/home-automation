using Home.Graph.Server.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.ComponentModel;

namespace Home.Graph.Server.Controllers
{
    partial class StorageController 
    {
        private class ContaierResult : Container
        {

        }

        [Route("containers/{containerId}")]
        public IActionResult GetContainer(bool includeStorageInfo = false, bool includeContent = false)
        {
            return new NotFoundResult();
        }


    }
}
