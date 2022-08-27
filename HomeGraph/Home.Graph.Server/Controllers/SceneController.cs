using Home.Common;
using Home.Common.Messages;
using Home.Common.Model;
using Home.Graph.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Home.Graph.Server.Controllers
{
    [Route("v1.0/homeautomation/scenes"), Authorize(Roles = "Agent,Admin,Device")]
    [ApiController]
    public class SceneController : ControllerBase
    {
        private class SceneGroupWithScenes : SceneGroup
        {
            public SceneGroupWithScenes()
            {
                Scenes = new List<Scene>();
            }

            public List<Scene> Scenes { get; set; }
        }

        [Route("groups"), HttpGet]
        public IEnumerable<SceneGroup> GetGroups()
        {
            return SceneHelper.GetGroups(false);
        }

        [Route("groups/{id}"), HttpGet]
        public SceneGroup GetGroup(string id)
        {
           return SceneHelper.GetGroup(id, false);
        }


        [Route("groups/{id}/activeScenes"), HttpPost]
        public SceneGroup UpdateActiveSceneForGroup(string id, [FromBody] List<string> scenes)
        {
            return SceneHelper.UpdateActiveSceneForGroup(id, scenes, false);
        }

        [Route("groups/{id}/activeScenes"), HttpDelete]
        public SceneGroup DeleteActiveSceneForGroup(string id, string scene)
        {
            return SceneHelper.DeleteActiveSceneForGroup(id, scene, false);
        }

        [Route("clearall"), HttpGet]
        public bool DeleteAll()
        {
            var group = MongoDbHelper.GetClient<SceneGroup>();
            group.DeleteMany(x => true);
            var scene = MongoDbHelper.GetClient<Scene>();
            scene.DeleteMany(x => true);

            return true;
        }


        [Route("groups/{groupId}"), HttpDelete]
        public bool DeleteGroup(string groupId)
        {
            var collection = MongoDbHelper.GetClient<SceneGroup>();
            var lst = collection.Find(x => x.Id == groupId).FirstOrDefault();

            if (lst == null)
                return true;

            var items = MongoDbHelper.GetClient<Scene>();
            var count = items.DeleteMany(x => x.GroupId == groupId);

            collection.DeleteOne(x => x.Id == lst.Id);

            MessagingHelper.PushToLocalAgent(new ScenarioContentChangedMessage()
            {
                SceneGroupId = groupId
            });

            return true;
        }

        [Route("groups"), HttpPost]
        public SceneGroup UpsertGroup([FromBody] SceneGroup group)
        {
            if (group.Id == null)
                group.Id = Guid.NewGuid().ToString("D").ToLowerInvariant();

            var collection = MongoDbHelper.GetClient<SceneGroup>();
            var lst = collection.Find(x => x.Id == group.Id).FirstOrDefault();

            if (lst == null)
            {
                collection.InsertOne(group);
            }
            else
            {
                collection.ReplaceOne(x => x.Id == lst.Id, group);
            }

            lst = collection.Find(x => x.Id == group.Id).FirstOrDefault();

            MessagingHelper.PushToLocalAgent(new ScenarioContentChangedMessage()
            {
                SceneGroupId = group.Id
            }); ;

            return lst;
        }

        [Route("groups/{groupid}/scenes"), HttpGet]
        public List<Scene> GetScenes(string groupid)
        {
            return SceneHelper.GetScenes(groupid, false);
        }

        [Route("scenes"), HttpGet]
        public List<Scene> GetScenes()
        {
            var collection = MongoDbHelper.GetClient<Scene>();
            var lst = collection.Find(x => true).ToList();
            return lst;
        }

        [Route("scenes/{id}"), HttpGet]
        public Scene GetScene(string id)
        {
            var collection = MongoDbHelper.GetClient<Scene>();
            var lst = collection.Find(x => x.Id.Equals(id)).FirstOrDefault();
            return lst;
        }

        [Route("scenes"), HttpPost]
        public Scene UpsertScene([FromBody] Scene scene)
        {
            if (scene.Id == null)
                scene.Id = Guid.NewGuid().ToString("D").ToLowerInvariant();

            var collection = MongoDbHelper.GetClient<Scene>();
            var lst = collection.Find(x => x.Id == scene.Id).FirstOrDefault();

            if (lst == null)
            {
                collection.InsertOne(scene);
            }
            else
            {
                collection.ReplaceOne(x => x.Id == lst.Id, scene);
            }

            lst = collection.Find(x => x.Id == scene.Id).FirstOrDefault();

            MessagingHelper.PushToLocalAgent(new ScenarioContentChangedMessage()
            {
                SceneGroupId = scene.GroupId,
                SceneId = scene.Id
            });


            return lst;
        }

        [Route("scene/{id}"), HttpDelete]
        public bool DeleteScene(string sceneId)
        {
            var collection = MongoDbHelper.GetClient<Scene>();
            var lst = collection.Find(x => x.Id == sceneId).FirstOrDefault();

            if (lst == null)
                return true;

            var items = MongoDbHelper.GetClient<SceneGroup>();
            var group = items.Find(x => x.Id == lst.GroupId).FirstOrDefault();
            if(group!=null)
            {
                if(group.CurrentActiveScenes.Contains(sceneId))
                {
                    group.CurrentActiveScenes.Remove(sceneId);
                    items.UpdateOne(x => x.Id == group.Id,
                       Builders<SceneGroup>.Update
                       .Set("CurrentActiveScenes", group.CurrentActiveScenes));
                }
            }

            collection.DeleteOne(x => x.Id == lst.Id);

            MessagingHelper.PushToLocalAgent(new ScenarioContentChangedMessage()
            {
                SceneGroupId = group?.Id,
                SceneId = sceneId
            });

            return true;
        }

        [Route("scene/{id}/images/{imagecode}"), Route("scenes/{id}/images/{imagecode}"), HttpDelete]
        public Scene DeleteImage(string id, string imagecode)
        {
            id = id.ToLowerInvariant();
            var collection = MongoDbHelper.GetClient<Scene>();
            var t = collection.Find(x => x.Id == id).FirstOrDefault();


            if (t != null)
            {
                var pth = FileCacheHelper.GetLocalFilename("home-automation", $"images/scenes/{id}", imagecode + ".png");

                if (System.IO.File.Exists(pth))
                    System.IO.File.Delete(pth);

                if (t.Images.ContainsKey(imagecode))
                    t.Images.Remove(imagecode);

                collection.UpdateOne(x => x.Id == id,
                     Builders<Scene>.Update.Set("Images", t.Images));
            }

            return t;
        }


        [Route("scene/{id}/images/{imagecode}"), Route("scenes/{id}/images/{imagecode}"), HttpPost]
        public Scene UpsertImage(string id, string imagecode)
        {
            id = id.ToLowerInvariant();
            var collection = MongoDbHelper.GetClient<Scene>();
            var t = collection.Find(x => x.Id == id).FirstOrDefault();


            if (t != null)
            {
                var pth = FileCacheHelper.GetLocalFilename("home-automation", $"images/scenes/{id}", imagecode + ".dat");
                using (var st = System.IO.File.Create(pth))
                    this.HttpContext.Request.BodyReader.CopyToAsync(st).Wait();

                try
                {
                    var fmt = Image.DetectFormat(pth);
                    if (fmt.DefaultMimeType.StartsWith("image/"))
                    {
                        switch (fmt.DefaultMimeType)
                        {
                            case "image/png":
                                System.IO.File.Move(pth, System.IO.Path.ChangeExtension(pth, ".png"));
                                pth = System.IO.Path.ChangeExtension(pth, ".png");
                                break;
                            default:
                                var img = Image.Load(pth);
                                img.SaveAsPng(System.IO.Path.ChangeExtension(pth, ".png"));
                                System.IO.File.Delete(pth);
                                pth = System.IO.Path.ChangeExtension(pth, ".png");
                                break;
                        }
                    }
                    else
                    {
                        System.IO.File.Delete(pth);
                        BadRequest("Invalid file format");
                    }
                }
                catch (Exception ex)
                {
                    if (System.IO.File.Exists(pth))
                        System.IO.File.Delete(pth);
                    Console.WriteLine(ex);
                    BadRequest("Invalid file format : " + ex.Message);
                }

                string pubUrl = $"https://public.anzin.carbenay.manoir.app/v1.0/services/files/home-automation/images/scenes/{id}/{imagecode}.png"; ;
                if (t.Images.ContainsKey(imagecode))
                    t.Images[imagecode] = pubUrl;
                else
                    t.Images.Add(imagecode, pubUrl);

                collection.UpdateOne(x => x.Id == id,
                                    Builders<Scene>.Update.Set("Images", t.Images));
            }

            return t;

        }

    }
}
