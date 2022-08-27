using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Home.Common.Model;
using Home.Graph.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace Home.Graph.Server.Controllers
{
    partial class DevicesController
    {
        [Route("self/associatedUser"), HttpPost]
        public bool SetAssociatedUser([FromBody] string user)
        {
            if (!User.IsInRole("Device"))
                return false;

            if (user == null)
                return false;

            user = user.ToLowerInvariant();

            var devId = User.Claims.FirstOrDefault(x => x.Type.Equals(ClaimTypes.NameIdentifier))?.Value;

            var ctl = MongoDbHelper.GetClient<Device>();
            var dev = ctl.Find(x => x.Id.Equals(devId) && x.MeshId.Equals("local")).FirstOrDefault();

            if (dev == null)
                return false;

            var usrctl = MongoDbHelper.GetClient<User>();
            var usr = usrctl.Find(x => x.Id.Equals(user)).FirstOrDefault();

            if (usr == null)
                return false;

            var filter = Builders<Device>.Filter.Eq("Id", dev.Id);
            var up = Builders<Device>.Update
                .Set("AssignatedUserId", user);

            var res = ctl.UpdateOne(filter, up);

            return res.IsModifiedCountAvailable && res.ModifiedCount == 1;
        }

        [Route("self/associatedUser"), HttpDelete]
        public bool RemoveAssociatedUser()
        {
            if (!User.IsInRole("Device"))
                return false;

            var devId = User.Claims.FirstOrDefault(x => x.Type.Equals(ClaimTypes.NameIdentifier))?.Value;

            var ctl = MongoDbHelper.GetClient<Device>();
            var dev = ctl.Find(x => x.Id.Equals(devId) && x.MeshId.Equals("local")).FirstOrDefault();

            if (dev == null)
                return false;

            var filter = Builders<Device>.Filter.Eq("Id", dev.Id);
            var up = Builders<Device>.Update
                .Set("AssignatedUserId", (string) null);

            var res = ctl.UpdateOne(filter, up);

            return res.IsModifiedCountAvailable && res.ModifiedCount == 1;
        }


        [Route("self/status"), HttpPatch]
        public bool ReplaceDataAndStatus(string status, [FromBody] List<DeviceData> datas)
        {
            if (!User.IsInRole("Device"))
                return false;

            var usr = User;
            var devId = User.Claims.FirstOrDefault(x => x.Type.Equals(ClaimTypes.NameIdentifier))?.Value;

            return ReplaceDataAndStatus(devId, status, datas);
        }

       
        [Route("self"), HttpGet]
        public Device GetSelf()
        {
            if (!User.IsInRole("Device"))
                return null;

            var usr = User;
            var devId = User.Claims.FirstOrDefault(x => x.Type.Equals(ClaimTypes.NameIdentifier))?.Value;

            var ctl = MongoDbHelper.GetClient<Device>();
            var dev = ctl.Find(x => x.Id.Equals(devId) && x.MeshId.Equals("local")).FirstOrDefault();

            if (dev == null)
                return null;

            return dev;
        }

    }
}
