using Home.Common.Model;
using Home.Graph.Common;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System;
using System.Collections.Generic;

namespace Home.Graph.Server.Controllers
{
    partial class EntitiesController : ControllerBase
    {
        [Route("weather")]
        public List<Entity> GetWeatherAsEntities()
        {
            var entities = new List<Entity>();

            var tmp = new AutomationMeshController(_adminContext, _appContext).Get("local");
            if (tmp != null && tmp.LocationInfo != null)
                entities.Add(new Entity(tmp.LocationInfo));

            return entities;
        }

        [Route("devices")]
        public List<Entity> GetDevicesAsEntities(string deviceKind = null)
        {
            var entities = new List<Entity>();

            var devs = new DevicesController(_adminContext, _appContext, _sysContext).FindDevice(kind: deviceKind);
            foreach (var dev in devs)
            {
                if (dev != null)
                    entities.Add(new Entity(dev));
            }

            return entities;
        }


        [Route("all")]
        public List<Entity> GetEntities(string kind = null)
        {
            var coll = MongoDbHelper.GetClient<Entity>();
            List<Entity> ret = new List<Entity>();

            var fl = Builders<Entity>.Filter;
            var lesFiltres = fl.Empty;

            if (kind != null)
                lesFiltres &= fl.Eq("EntityKind", kind);

            ret = coll.Find<Entity>(lesFiltres).ToList();

            string deviceKind = null;
            if (kind != null && kind.StartsWith("manoirapp:device/"))
            {
                deviceKind = kind.Substring(17);
            }

            if (kind == null || kind.Equals("device", StringComparison.InvariantCultureIgnoreCase)
                || deviceKind != null)
            {
                var devs = new DevicesController(_adminContext, _appContext, _sysContext).FindDevice(kind: deviceKind);
                foreach (var dev in devs)
                {
                    if (dev != null)
                        ret.Add(new Entity(dev));
                }
            }

            if (kind == null || kind.Equals("weather", StringComparison.InvariantCultureIgnoreCase))
            {
                var we = GetWeatherAsEntities();
                ret.AddRange(we);
            }

            return ret;
        }

        [Route("all"), HttpPost]
        public Entity UpsertEntity(Entity entity)
        {
            if (entity.Id == null)
                entity.Id = Guid.NewGuid().ToString("D").ToLowerInvariant();

            entity.LastUpdate = DateTimeOffset.Now;

            string kind = entity.EntityKind;

            if (kind == null)
                throw new InvalidOperationException();

            if (kind.StartsWith("manoirapp:device/"))
                throw new InvalidOperationException();

            var collection = MongoDbHelper.GetClient<Entity>();
            var lst = collection.Find(x => x.Id == entity.Id).FirstOrDefault();

            if (lst == null)
            {
                collection.InsertOne(entity);
            }
            else
            {
                collection.ReplaceOne(x => x.Id == lst.Id, entity);
            }

            lst = collection.Find(x => x.Id == entity.Id).FirstOrDefault();
            return lst;
        }

        [Route("all/{entityId}"), HttpDelete]
        public bool DeleteEntity(string entityId)
        {
            var collection = MongoDbHelper.GetClient<Entity>();
            var lst = collection.Find(x => x.Id == entityId).FirstOrDefault();

            if (lst == null)
                return true;

            collection.DeleteOne(x => x.Id == lst.Id);
            return true;
        }

        [Route("all/{entityId}"), HttpGet]
        public Entity GetEntity(string entityId)
        {
            entityId = entityId.ToLowerInvariant();

            var collection = MongoDbHelper.GetClient<Entity>();
            var lst = collection.Find(x => x.Id == entityId).FirstOrDefault();
            return lst;
        }
    }
}
