using Home.Common.Messages;
using Home.Common.Model;
using Home.Graph.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System.Collections.Generic;
using System.IO;

namespace Home.Graph.Server.Controllers
{
    partial class AutomationMeshController
    {
        [Route("local/location/infos/weatherhazard"), HttpPost]
        public bool SetWeatherHazard([FromBody] List<WeatherHazard> hazards)
        {
            var collection = MongoDbHelper.GetClient<AutomationMesh>();
            var lst = collection.Find(x => x.Id == "local").FirstOrDefault();
            if (lst == null)
                return false;

            var res = collection.UpdateOne(x => x.Id == "local" && x.LocationInfo != null,
                                Builders<AutomationMesh>.Update
                                .Set("LocationInfo.WeatherHazards", hazards));
            if (res.IsAcknowledged && res.ModifiedCount == 0)
            {

                lst.LocationInfo = new AutomationMeshLocationInfo()
                {
                    WeatherHazards = hazards
                };
                res = collection.UpdateOne(x => x.Id == "local" && x.LocationInfo == null,
                                Builders<AutomationMesh>.Update
                                .Set("LocationInfo", lst.LocationInfo));
            }

            return res.IsAcknowledged && res.ModifiedCount > 0;
        }

        [Route("local/location/infos/weather"), HttpPost]
        public bool SetWeather([FromBody] List<WeatherInfo> weather)
        {
            var collection = MongoDbHelper.GetClient<AutomationMesh>();
            var lst = collection.Find(x => x.Id == "local").FirstOrDefault();
            if (lst == null)
                return false;
            var res = collection.UpdateOne(x => x.Id == "local" && x.LocationInfo != null,
                                Builders<AutomationMesh>.Update
                                .Set("LocationInfo.Weather", weather));
            if (res.IsAcknowledged && res.ModifiedCount == 0)
            {

                lst.LocationInfo = new AutomationMeshLocationInfo()
                {
                    Weather = weather
                };
                res = collection.UpdateOne(x => x.Id == "local" && x.LocationInfo == null,
                                Builders<AutomationMesh>.Update
                                .Set("LocationInfo", lst.LocationInfo));

                if (res.IsAcknowledged && res.ModifiedCount > 0)
                    MessagingHelper.PushToLocalAgent(new WeatherChangeMessage(weather));

                var mesh = collection.Find(x => x.Id == "local").FirstOrDefault();
                if (mesh != null)
                    MqttHelper.PublishEntity(new Entity(mesh.LocationInfo));
            }
            else
            {
                MessagingHelper.PushToLocalAgent(new WeatherChangeMessage(weather));
            }

            return res.IsAcknowledged && res.ModifiedCount > 0;
        }
    }
}
