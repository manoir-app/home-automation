using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using Home.Graph.Common;
using Home.Common.Model;

namespace Home.Graph.Server.Controllers
{
    [ApiController]
    [Route("v1.0/locations")]
    public class LocationsController : ControllerBase
    {
        [HttpGet]
        public List<Location> GetAll()
        {
            var collection = MongoDbHelper.GetClient<Location>();
            var lst = collection.Find(x => true).ToList();
            return lst;
        }

        [Route("{id:guid}"), HttpGet]
        public Location Get(string id)
        {
            if (id == null)
                return null;

            id = id.ToLowerInvariant();

            var collection = MongoDbHelper.GetClient<Location>();
            var lst = collection.Find(x => x.Id == id).FirstOrDefault();
            return lst;
        }

        [HttpPost]
        public Location Update([FromBody] Location loc)
        {
            if (loc == null)
                return null;

            var collection = MongoDbHelper.GetClient<Location>();

            if (loc.Zones != null)
            {
                foreach (var z in loc.Zones)
                {
                    if(z.Id == null)
                        z.Id = Guid.NewGuid().ToString("D").ToUpperInvariant();
                    z.Id = z.Id.ToUpperInvariant();
                    if(z.Rooms!=null)
                    {
                        foreach(var r in z.Rooms)
                        {
                            if(r.Id == null)
                                r.Id = Guid.NewGuid().ToString("D").ToUpperInvariant();
                            r.Id = r.Id.ToUpperInvariant();
                        }
                    }
                }
            }

            if (loc.Id == null)
            {
                loc.Id = Guid.NewGuid().ToString("d").ToLowerInvariant();
                collection.InsertOne(loc);
            }
            else
            {
                var exists = collection.Find(x => x.Id == loc.Id).FirstOrDefault();
                if(exists != null)
                    collection.ReplaceOne(x => x.Id == loc.Id, loc);
                else
                    collection.InsertOne(loc);
            }

            var lst = collection.Find(x => x.Id == loc.Id).FirstOrDefault();
            return lst;
        }

    }
}