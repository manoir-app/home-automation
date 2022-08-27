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
    [Route("api/tests")]
    [ApiController]
    public class TestsController : ControllerBase
    {
        [Route("greetings/{userName}")]
        public GreetingsMessageResponse GetGreetings(string userName)
        {
            if (userName == null)
                return null;
            userName = userName.ToLowerInvariant();
            var collection = MongoDbHelper.GetClient<User>();
            var usr = collection.Find(x => x.Id == userName).FirstOrDefault();
            if (usr == null)
                return null;

            var msg = new GreetingsMessage(GreetingsMessage.SimpleGetGreetings)
            {
                Destination = GreetingsMessage.GreetingsDestination.UserApp,
            };
            msg.Users.Add(usr);

            return MessagingHelper.RequestFromLocalAgent<GreetingsMessageResponse>(msg);
        }

    }
}
