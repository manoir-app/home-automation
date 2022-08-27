using Home.Common.Messages;
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
    [Route("v1.0/pim/scheduler")]
    [ApiController]
    public class SchedulerController : ControllerBase
    {
        [Route("wakeuptime/next"), HttpGet]
        public RoutineDataWithUser[] GetWakeupTime()
        {
            var collection = MongoDbHelper.GetClient<User>();
            var lst = collection.Find(x => x.IsMain).ToList();

            var ret = new List<RoutineDataWithUser>();

            foreach(var usr in lst)
            {
                if(usr.Routine!=null)
                    ret.Add(new RoutineDataWithUser(usr));
            }

            return ret.ToArray();
        }

        [Route("wakeuptime/next"), HttpPost]
        public bool SetWakeupTime([FromBody] RoutineDataWithUser data)
        {
            var collection = MongoDbHelper.GetClient<User>();
            var usr = collection.Find(x => x.Id == data.UserId).FirstOrDefault();

            if(usr.Routine==null)
            {
                collection.UpdateOne(x => x.Id == usr.Id,
                        Builders<User>.Update
                    .Set("Routine", new RoutineData(data)));
                PimHelper.NotifyPimItemUpdate(PimItemUpdateMessage.WakeUpTimeUpdate,
                    PimItemUpdateMessage.PimItemKindScheduler, usr.Id, "");
            }
            else
            {
                collection.UpdateOne(x => x.Id == usr.Id,
                Builders<User>.Update
                .Set("Routine.NextWakeUpTime", data.NextWakeUpTime));
                PimHelper.NotifyPimItemUpdate(PimItemUpdateMessage.WakeUpTimeUpdate,
                    PimItemUpdateMessage.PimItemKindScheduler, usr.Id, "");
            }
            return true;
        }

    }
}
