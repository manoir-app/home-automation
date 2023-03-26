using Home.Common.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System.Collections.Generic;
using System;
using Home.Graph.Common;

namespace Home.Graph.Server.Controllers
{
    [Route("v1.0/home/circuits"), Authorize(Roles = "Admin,User,Agent,Device")]
    [ApiController]
    public class HomeCircuitController : ControllerBase
    {
        #region HomeCircuits
        [HttpGet]
        public List<HomeCircuit> GetHomeCircuits()
        {
            var collection = MongoDbHelper.GetClient<HomeCircuit>();
            var lst = collection.Find(x => true).ToList();
            return lst;
        }

        [Route("{circuitId}"), HttpGet]
        public HomeCircuit GetHomeCircuit(string circuitId)
        {
            var collection = MongoDbHelper.GetClient<HomeCircuit>();
            var lst = collection.Find(x => x.Id == circuitId).FirstOrDefault();
            return lst;
        }

        [HttpPost]
        public HomeCircuit UpsertHomeCircuit([FromBody] HomeCircuit account)
        {
            if (account.Id == null)
                account.Id = Guid.NewGuid().ToString("D").ToLowerInvariant();

            var collection = MongoDbHelper.GetClient<HomeCircuit>();
            var lst = collection.Find(x => x.Id == account.Id).FirstOrDefault();

            if (lst == null)
            {
                collection.InsertOne(account);
            }
            else
            {
                collection.ReplaceOne(x => x.Id == lst.Id, account);
            }

            lst = collection.Find(x => x.Id == account.Id).FirstOrDefault();
            return lst;
        }

        [Route("{circuitId}"), HttpDelete]
        public bool DeleteHomeCircuit(string circuitId)
        {
            var collection = MongoDbHelper.GetClient<HomeCircuit>();
            var lst = collection.Find(x => x.Id == circuitId).FirstOrDefault();

            if (lst == null)
                return true;

            collection.DeleteOne(x => x.Id == lst.Id);
            return true;
        }
        #endregion
    }
}
