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

        [Route("me/interests")]
        public List<UserInterestInfo> GetUserInterests()
        {
            var user = GetCurrentUserId();
            if (user != null)
                return GetUserInterests(user);
            return null;
        }


        [Route("all/{user}/interests"), HttpGet()]
        public List<UserInterestInfo> GetUserInterests(string user)
        {
            if (user == null)
                return null;

            user = user.ToLowerInvariant();

            var coll = MongoDbHelper.GetClient<UserInterestInfo>();
            return coll.Find(x => x.UserId == user).ToList();
        }

        [Route("me/interests"), HttpPost()]
        public UserInterestInfo UpsertInterest([FromBody] UserInterestInfo t)
        {
            var user = GetCurrentUserId();
            if (user != null)
                return UpsertInterest(user, t);
            return null;
        }

        [Route("all/{user}/interests"), HttpPost()]
        public UserInterestInfo UpsertInterest(string user, [FromBody] UserInterestInfo t)
        {
            if (user == null)
                return null;

            user = user.ToLowerInvariant();

            var coll = MongoDbHelper.GetClient<UserInterestInfo>();
            t.UserId = user;
            if (t.Id == null)
            {
                t.Id = Guid.NewGuid().ToString("N");
                coll.InsertOne(t);
            }
            else
            {
                coll.ReplaceOne(x => x.Id == t.Id, t);
            }
            return GetUserInterest(user, t.Id);
        }

        [Route("me/interests/{interestId}"), HttpGet()]
        public UserInterestInfo GetUserInterest(string interestId)
        {
            var user = GetCurrentUserId();
            if (user != null)
                return GetUserInterest(user, interestId);
            return null;
        }

        [Route("all/{user}/interests/{interestId}"), HttpGet()]
        public UserInterestInfo GetUserInterest(string user, string interestId)
        {
            if (user == null)
                return null;

            user = user.ToLowerInvariant();

            var coll = MongoDbHelper.GetClient<UserInterestInfo>();
            return coll.Find(x => x.Id == interestId && x.UserId==user).FirstOrDefault();
        }

        [Route("me/triggers/{interestId}"), HttpDelete()]
        public bool DeleteUserInterest(string interestId)
        {
            var user = GetCurrentUserId();
            if (user != null)
                return DeleteUserInterest(user, interestId);
            return false;
        }

        [Route("{user}/interests/{interestId}"), HttpDelete()]
        public bool DeleteUserInterest(string user, string interestId)
        {
            if (user == null)
                return false;

            user = user.ToLowerInvariant();

            var coll = MongoDbHelper.GetClient<UserInterestInfo>();
            var obj = coll.Find(x => x.Id == interestId && x.UserId==user).FirstOrDefault();
            if (obj == null)
                return false;

            var res = coll.DeleteOne(x => x.Id == interestId && x.UserId == user);
            if (res.IsAcknowledged)
                return true;

            return false;
        }
    }
}
