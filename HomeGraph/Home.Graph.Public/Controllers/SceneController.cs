using Home.Common.Model;
using Home.Graph.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Home.Graph.Public.Controllers
{
    [Route("v1.0/homeautomation/scenes"), Authorize(Roles = "Agent,Admin")]
    [ApiController]
    public class SceneController : ControllerBase
    {
        [Route("groups"), HttpGet]
        public IEnumerable<SceneGroup> GetGroups()
        {
            return SceneHelper.GetGroups(true);
        }

        [Route("groups/{id}"), HttpGet]
        public SceneGroup GetGroup(string id)
        {
            return SceneHelper.GetGroup(id, true);
        }

        [Route("groups/{groupid}/scenes"), HttpGet]
        public List<Scene> GetScenes(string groupid)
        {
            return SceneHelper.GetScenes(groupid, true);
        }

        [Route("groups/{id}/activeScenes"), HttpPost]
        public SceneGroup UpdateActiveSceneForGroup(string id, [FromBody] List<string> scenes)
        {
            return SceneHelper.UpdateActiveSceneForGroup(id, scenes, true);
        }

        [Route("groups/{id}/activeScenes"), HttpDelete]
        public SceneGroup DeleteActiveSceneForGroup(string id, string scene)
        {
            return SceneHelper.DeleteActiveSceneForGroup(id, scene, true);
        }

    }
}
