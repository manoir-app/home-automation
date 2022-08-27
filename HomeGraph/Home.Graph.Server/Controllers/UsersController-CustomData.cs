using Home.Common.Model;
using Home.Graph.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Home.Graph.Server.Controllers
{
    partial class UsersController
    {
        [Route("me/data/custom"), HttpGet()]
        public List<UserCustomData> GetUserCustomDatas()
        {
            var user = GetCurrentUserId();
            if (user != null)
                return GetUserCustomDatas(user);
            return null;
        }


        [Route("{user}/data/custom"), HttpGet()]
        public List<UserCustomData> GetUserCustomDatas(string user)
        {
            if (user == null)
                return null;

            user = user.ToLowerInvariant();

            var coll = MongoDbHelper.GetClient<UserCustomData>();
            return coll.Find(x => x.UserId == user).ToList();
        }

        [Route("{user}/data/custom"), HttpPost]
        public UserCustomData SetUserCustomDatas(string user, [FromBody] UserCustomData t)
        {
            if (user == null)
                return null;

            if (t.Code == null)
                throw new ArgumentException();

            user = user.ToLowerInvariant();

            var coll = MongoDbHelper.GetClient<UserCustomData>();
            t.UserId = user;
            t.Code = t.Code.ToLowerInvariant();
            t.Id = user + "_" + t.Code;
            coll.ReplaceOne(x => x.UserId == user && x.Code == t.Code , t, 
                new ReplaceOptions()
                {
                    IsUpsert = true
                });


            return GetUserCustomData(user, t.Code);
        }

        [Route("{user}/data/custom/{dataCode}"), HttpGet()]
        public UserCustomData GetUserCustomData(string user, string dataCode)
        {
            if (user == null)
                return null;
            if (dataCode == null)
                return null;

            dataCode = dataCode.ToLowerInvariant();
            user = user.ToLowerInvariant();
            
            var coll = MongoDbHelper.GetClient<UserCustomData>();
            return coll.Find(x => x.UserId == user && x.Code == dataCode).FirstOrDefault();
        }

        [Route("{user}/data/custom/{dataCode}"), HttpDelete]
        public bool DeleteUserCustomData(string user, string dataCode)
        {
            if (user == null)
                return false;
            if (dataCode == null)
                return false;

            dataCode = dataCode.ToLowerInvariant();
            user = user.ToLowerInvariant();

            var coll = MongoDbHelper.GetClient<UserCustomData>();
            var res = coll.DeleteOne(x => x.UserId == user && x.Code == dataCode);
            return res.IsAcknowledged && res.DeletedCount == 1;
        }
    }
}
