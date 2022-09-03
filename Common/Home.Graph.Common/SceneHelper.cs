using Home.Common.Model;
using MongoDB.Driver;
using System.Linq;
using System.Collections.Generic;
using System;

namespace Home.Graph.Common
{
    public static class SceneHelper
    {
        public static List<SceneGroup> GetGroups(bool onlyRemote)
        {
            var sort = Builders<SceneGroup>.Sort.Ascending("Order");
            var collection = MongoDbHelper.GetClient<SceneGroup>();
            if (!onlyRemote)
            {
                return collection.Find(x => true).Sort(sort).ToList();
                // (onlyRemote ? (x.VisibleInRemote == true) : true) tout seul lance une exception
                // donc on le fait à la classique avec le if externalisé...
            }
            else
            {
                return collection.Find(x => x.VisibleInRemote == true).Sort(sort).ToList();
            }
        }

        public static SceneGroup GetGroup(string id, bool onlyRemote)
        {
            var collection = MongoDbHelper.GetClient<SceneGroup>();

            if (!onlyRemote)
            {
                return collection.Find(x => x.Id.Equals(id)).FirstOrDefault();
                // (onlyRemote ? (x.VisibleInRemote == true) : true) tout seul lance une exception
                // donc on le fait à la classique avec le if externalisé...
            }
            else
            {
                return collection.Find(x => x.VisibleInRemote == true && x.Id.Equals(id)).FirstOrDefault();
            }
        }

        public static List<Scene> GetScenes(string groupid, bool onlyRemote)
        {
            var sort = Builders<Scene>.Sort.Ascending("Order");
            var grp = GetGroup(groupid, onlyRemote);
            if (grp == null)
                return new List<Scene>();
            var collection = MongoDbHelper.GetClient<Scene>();
            if (!onlyRemote)
            {
                return collection.Find(x => x.GroupId == groupid).Sort(sort).ToList();
            }
            else
            {
                return collection.Find(x => x.GroupId == groupid
                && x.VisibleInRemote == true).Sort(sort).ToList();

            }
        }

        public static Scene GetScene(string groupid, string sceneId, bool onlyRemote)
        {
            var grp = GetGroup(groupid, onlyRemote);
            if (grp == null)
                return null;
            var collection = MongoDbHelper.GetClient<Scene>();
            if (!onlyRemote)
            {
                return collection.Find(x => x.GroupId == groupid
                && x.Id == sceneId).FirstOrDefault();
            }
            else
            {
                return collection.Find(x => x.GroupId == groupid
                && x.Id == sceneId
                && x.VisibleInRemote == true).FirstOrDefault();
            }
        }

        public static SceneGroup DeleteActiveSceneForGroup(string groupId, string scene, bool onlyRemote)
        {
            var collection = MongoDbHelper.GetClient<SceneGroup>();
            var group = GetGroup(groupId, onlyRemote);
            var sc = GetScene(groupId, scene, onlyRemote);

            // on ne peut retirer qu'une scene "active en remote"
            if (group != null && sc!=null)
            {
                var scenes = (from z in @group.CurrentActiveScenes
                              where !z.Equals(scene, StringComparison.InvariantCultureIgnoreCase)
                              select z).ToList();
                collection.UpdateOne(x => x.Id == groupId,
                    Builders<SceneGroup>.Update
                    .Set("CurrentActiveScenes", scenes));
            }
            var lst = collection.Find(x => x.Id == groupId).FirstOrDefault();
            return lst;
        }


        public static SceneGroup UpdateActiveSceneForGroup(string id, List<string> scenes, bool onlyRemote)
        {
            Console.WriteLine($"Updating Active Scenes for {id} : {string.Join(",", scenes)}");
            var gp = GetGroup(id, onlyRemote);
            if (gp == null)
                return null;

            // si on est en remote, on n'ajoute que des scenes
            // qui sont remotable, par contre on laisse bien
            // les scenes déjà actives 
            if (onlyRemote)
            {
                if (gp.CurrentActiveScenes == null)
                    gp.CurrentActiveScenes = new List<string>();

                var newScenes = (from z in scenes
                                 where !gp.CurrentActiveScenes.Contains(z)
                                 select z);
                foreach(var newScene in newScenes)
                {
                    var sc = GetScene(id, newScene, onlyRemote);
                    if (sc == null)
                        scenes.Remove(newScene);
                }
            }

            Console.WriteLine($"Updating Active Scenes for {id} (after removing non remote): {string.Join(",", scenes)}");

            try
            {
                var removed = (from z in gp.CurrentActiveScenes
                               where !scenes.Contains(z)
                               select z).ToList();

                var allScenes = GetScenes(gp.Id, false);

                foreach (var sc in removed)
                {
                    var laSc = (from z in allScenes
                                where z.Id.Equals(sc, StringComparison.InvariantCultureIgnoreCase)
                                select z).FirstOrDefault();
                    MqttHelper.PublishSceneGroupStatus(gp.Id, gp.Label, DateTimeOffset.Now, laSc.Id, laSc.Label, false);
                }

                foreach (var sc in scenes)
                {
                    var laSc = (from z in allScenes
                                where z.Id.Equals(sc, StringComparison.InvariantCultureIgnoreCase)
                                select z).FirstOrDefault();
                    MqttHelper.PublishSceneGroupStatus(gp.Id, gp.Label, DateTimeOffset.Now, laSc.Id, laSc.Label, true);
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
            }

            var collection = MongoDbHelper.GetClient<SceneGroup>();
            collection.UpdateOne(x => x.Id == id,
                Builders<SceneGroup>.Update
                .Set("CurrentActiveScenes", scenes));
            var lst = collection.Find(x => x.Id == id).FirstOrDefault();
            return lst;
        }

    }
}
