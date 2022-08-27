using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Home.Common.Messages;
using Home.Common.Model;
using Home.Graph.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace Home.Graph.Server.Controllers
{
    partial class DevicesController
    {
        [Route("self/interactions/greetings"), HttpGet]
        public GreetingsMessageResponse GetGreetingsForDevice()
        {
            if (!User.IsInRole("Device"))
            {
                Console.WriteLine("Pas un device");
                return null;
            }

            var devId = User.Claims.FirstOrDefault(x => x.Type.Equals(ClaimTypes.NameIdentifier))?.Value;

            var ctl = MongoDbHelper.GetClient<Device>();
            var dev = ctl.Find(x => x.Id.Equals(devId) && x.MeshId.Equals("local")).FirstOrDefault();

            if (string.IsNullOrEmpty(dev.AssignatedUserId))
            {
                Console.WriteLine($"Device is not associated with a user, getting mesh greetings");
                return GetGreetingsForMeshOnDevice(dev.DeviceKind);

            }
            else
            {
                Console.WriteLine($"Device is associated with {dev.AssignatedUserId}, getting greetings");
                return GetGreetingsForUserOnDevice(dev.AssignatedUserId);
            }
        }

        private GreetingsMessageResponse GetGreetingsForMeshOnDevice(string deviceKind)
        {
            var msg = new GreetingsMessage(GreetingsMessage.SimpleGetGreetings)
            {
                Destination = GreetingsMessage.GreetingsDestination.Screen,
            };
            return MessagingHelper.RequestFromLocalAgent<GreetingsMessageResponse>(msg);
        }

        internal static GreetingsMessageResponse GetGreetingsForUserOnDevice(string userId)
        {
            var collection = MongoDbHelper.GetClient<User>();

            var usr = collection.Find(x => x.Id == userId).FirstOrDefault();
            if (usr == null)
            {
                Console.WriteLine("User (" + userId + ") pas trouvé");
                return null;
            }

            var msg = new GreetingsMessage(GreetingsMessage.SimpleGetGreetings)
            {
                Destination = GreetingsMessage.GreetingsDestination.UserApp,
            };
            msg.Users.Add(usr);
            return MessagingHelper.RequestFromLocalAgent<GreetingsMessageResponse>(msg);
        }

    }
}
