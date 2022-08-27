using Home.Common;
using Microsoft.AspNetCore.Mvc;
using System;

namespace Home.Graph.Server.Controllers
{
    // ------------------------------------------------
    //  ! Attention, partie uniquement publique du redirecteur
    // ------------------------------------------------
    partial class DataRedirectionController : ControllerBase
    {
        [Route("")]
        public IActionResult RootRedirect()
        {
            return Redirect(HomeServerHelper.GetLocalGraphUrl());
        }

        [Route("me")]
        public IActionResult MeRedirect()
        {
            return Redirect(HomeServerHelper.GetLocalGraphUrl("me"));
        }

    }
}
