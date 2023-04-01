using Home.Common;
using Home.Common.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace Home.Graph.Common
{
    partial class ConditionHelper
    {
        private bool EvaluateForScene(Condition condition)
        {
            if (condition == null)
                return true;
            if (string.IsNullOrEmpty(condition.PropertyName))
                return true;
            switch (condition.PropertyName.ToLowerInvariant())
            {
                case "isactive":
                case "is-active":
                    return CheckSceneIsActive(condition.ElementId);
                case "isinactive":
                case "isnotactive":
                case "is-no-active":
                case "is-not-active":
                case "is-inactive":
                    return ! CheckSceneIsActive(condition.ElementId);
            }
            return false;
        }

        private bool CheckSceneIsActive(string sceneId)
        {
            lock (this)
            {
                List<SceneGroup> tmp = GetSceneGroups();
                foreach (var grp in tmp)
                {
                    if(grp.CurrentActiveScenes!=null && grp.CurrentActiveScenes.Contains(sceneId))
                        return true;
                }
            }

            return false;
        }


        private List<SceneGroup> GetSceneGroups()
        {
            List<SceneGroup> tmp = null;
            if (_cache.TryGetValue("SceneGroups", out object fromCache))
                tmp = fromCache as List<SceneGroup>;
            if (tmp == null)
            {
                tmp = SceneHelper.GetGroups(false);
                _cache.Add("SceneGroups", tmp);
            }

            return tmp;
        }

    }
}
