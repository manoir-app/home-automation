using Home.Common.Model;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;

namespace Home.Graph.Common
{
    public static class UserHelper
    {
        public static List<User> GetLocalPresentUsers()
        {
            var collMesh = MongoDbHelper.GetClient<AutomationMesh>();
            var lst = collMesh.Find(x => x.Id == "local").FirstOrDefault();

            if (string.IsNullOrEmpty(lst.LocationId))
                return new List<User>();

            var collection = MongoDbHelper.GetClient<User>();
            var arrayFilters = Builders<User>.Filter.Eq("Presence.Location.LocationId", lst.LocationId);
            var usrs = collection.Find(arrayFilters).ToList();
            usrs.ForEach(x => x.ForPresence());
            return usrs;
        }
    }
}
