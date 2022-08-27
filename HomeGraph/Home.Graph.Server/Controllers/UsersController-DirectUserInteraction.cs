using Home.Common;
using Home.Common.Messages;
using Home.Common.Model;
using Home.Graph.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Home.Graph.Server.Controllers
{
    partial class UsersController
    {
        [Route("interactions/greetings/fromdevice"), HttpGet]
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
                Console.WriteLine("Pas de user associé au device");
                return null;
            }
            Console.WriteLine($"Device is associated with {dev.AssignatedUserId}, getting greetings");
            return DevicesController.GetGreetingsForUserOnDevice(dev.AssignatedUserId);
        }


        [Route("interactions/greetings/all/{userName}"), HttpGet]
        public GreetingsMessageResponse GetGreetingsForUser(string userName)
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

        [Route("interactions/greetings/all/{userName}"), HttpPost]
        public bool PushNewGreetings(string userName, [FromBody] GreetingsMessageResponse item)
        {
            if (userName == null)
                return false;

            userName = userName.ToLowerInvariant();

            var collection = MongoDbHelper.GetClient<User>();
            var usr = collection.Find(x => x.Id == userName).FirstOrDefault();
            if (usr == null)
                return false;

            // on va push sur tous les devices mobiles qui correspondent

            var ctl = MongoDbHelper.GetClient<Device>();
            var devs = ctl.Find(x => x.AssignatedUserId!=null 
                && x.AssignatedUserId.Equals(userName) 
                && x.MeshId.Equals("local")
                && x.DeviceKind == Device.DeviceKindMobileDevice).ToList();

            try
            {
                Console.WriteLine($"Found {devs?.Count} devices for user " + userName + " => dispatching notifications");
            }
            catch
            {

            }

            Dictionary<string, string> datas = new Dictionary<string, string>();
            datas.Add("messageType", "greetings_change");
            datas.Add("messageData", JsonConvert.SerializeObject(item));

            if(devs!=null && devs.Count>0)
                MessagingHelper.PushToMobileApp(usr.Id, datas);
          
            return true;
        }
    }
}
