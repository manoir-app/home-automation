using Home.Common.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System.Collections.Generic;
using System;
using Microsoft.AspNetCore.Authorization;
using Home.Journal.Common.Model;
using Home.Graph.Common;

namespace Home.Graph.Server.Controllers
{
    partial class JournalController 
    {
        [Route("sections/{sectionId}/properties"), HttpPost]
        public PageSection UpdateProperties(string sectionId, Dictionary<string, string> props)
        {
            var collection = MongoDbHelper.GetClient<PageSection>();

            var res = collection.UpdateOne(x => x.Id.Equals(sectionId),
              Builders<PageSection>.Update
              .Set("Properties", props));

            var lst = collection.Find(x => x.Id.Equals(sectionId)).FirstOrDefault();
            return lst;
        }
    }
}
