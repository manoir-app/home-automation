using Home.Common.Messages;
using Home.Common.Model;
using Home.Graph.Common;
using Home.Graph.Server.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Home.Graph.Server.Controllers
{
    [Route("v1.0/users"), Authorize(Roles = "Admin,Agent,Device,User")]
    [ApiController]
    public partial class UsersController : ControllerBase
    {
        private readonly IHubContext<UsersHub> _usersContext = null;
        private readonly IHubContext<AdminToolsHub> _hubContext;
        private readonly IHubContext<AppAndDeviceHub> _appContext;

        public UsersController(IHubContext<AdminToolsHub> hubContext, IHubContext<AppAndDeviceHub> appContext, IHubContext<UsersHub> usersContext)
        {
            _hubContext = hubContext;
            _appContext = appContext;
            _usersContext = usersContext;

        }

        private string GetCurrentUserId()
        {
            var userFromContext = User;
            return GetCurrentUserId(userFromContext);
        }

        public static string GetCurrentUserId(ClaimsPrincipal userFromContext)
        {
            if (userFromContext.IsInRole("Admin") || userFromContext.IsInRole("User"))
            {
                var clm = userFromContext.Claims.Where(c => c.Type.Equals(ClaimTypes.NameIdentifier)).FirstOrDefault();
                if (clm != null)
                    return clm.Value;
            }

            if (userFromContext.IsInRole("Device"))
            {
                var t = (from z in userFromContext.Claims
                         where z.Type.Equals("AssociatedUserId", System.StringComparison.InvariantCultureIgnoreCase)
                         select z).FirstOrDefault();
                if (t != null)
                {
                    return t.Value;
                }
            }

            return null;
        }



        [Route("main"), HttpGet]
        public List<User> GetMainUsers()
        {
            var collection = MongoDbHelper.GetClient<User>();
            var lst = collection.Find(x => x.IsMain);
            return lst.ToList();
        }

        [Route("all"), HttpGet]
        public List<User> GetAllUsers()
        {
            var collection = MongoDbHelper.GetClient<User>();
            var lst = collection.Find(x => true);
            return lst.ToList();
        }

        [Route("all/{userName}"), HttpGet]
        public User GetUser(string userName)
        {
            if (userName == null)
                return null;
            userName = userName.ToLowerInvariant();
            var collection = MongoDbHelper.GetClient<User>();
            var usr = collection.Find(x => x.Id == userName).FirstOrDefault();
            return usr;
        }

        [Route("mains/{userName}"), HttpDelete]
        public bool DeleteMainUser(string userName)
        {
            if (userName == null)
                return true;
            userName = userName.ToLowerInvariant();

            var collection = MongoDbHelper.GetClient<User>();
            var lst = collection.Find(x => x.IsMain).ToList();

            if (lst.Count == 1 && lst[0].Id == userName)
                return false;

            var res = collection.DeleteOne(x => x.Id == userName && x.IsMain);
            if (res.DeletedCount == 1)
            {
                MessagingHelper.PushToLocalAgent(new UserChangeMessage(UserChangeMessage.DeletedTopic, userName));
                return true;
            }
            else
                return false;
        }

        [Route("all/{userName}"), HttpDelete]
        public bool DeleteOtherUser(string userName)
        {
            if (userName == null)
                return true;
            userName = userName.ToLowerInvariant();

            var collection = MongoDbHelper.GetClient<User>();
            var exists = collection.Find(x => !x.IsMain
                    && x.Id == userName).FirstOrDefault();

            if (exists == null)
                return true;

            collection.DeleteOne(x => x.Id == userName);

            if (exists.IsGuest)
                MessagingHelper.PushToLocalAgent(new UserChangeMessage(UserChangeMessage.GuestDeletedTopic, userName));
            else
                MessagingHelper.PushToLocalAgent(new UserChangeMessage(UserChangeMessage.DeletedTopic, userName));

            return true;

        }

        [Route("guests"), HttpGet]
        public List<User> GetGuestUsers()
        {
            var collection = MongoDbHelper.GetClient<User>();
            var lst = collection.Find(x => x.IsGuest);
            return lst.ToList();
        }

        [Route("guests/upsert"), HttpPost]
        public User UpsertGuestUser([FromBody] User usrToUpsert)
        {
            string userName = GetGuestUserId(usrToUpsert);
            if (userName == null)
                return null;
            userName = userName.ToLowerInvariant();
            var collection = MongoDbHelper.GetClient<User>();
            var usr = collection.Find(x => x.Id == userName).FirstOrDefault();
            if (usr != null)
            {
                if (!usr.IsGuest)
                    return null;

                var arrayUpdate = Builders<User>.Update
                  .Set("CommonName", usrToUpsert.CommonName)
                  .Set("FirstName", usrToUpsert.FirstName)
                  .Set("MainEmail", usrToUpsert.MainEmail)
                  .Set("MainPhoneNumber", usrToUpsert.MainPhoneNumber)
                  .Set("Name", usrToUpsert.Name)
                    ;

                // les champs updatables
                collection.UpdateOne(x => x.Id == userName, arrayUpdate);

                MessagingHelper.PushToLocalAgent(new UserChangeMessage(UserChangeMessage.GuestUpdatedTopic, userName));

                return collection.Find(x => x.Id == userName).FirstOrDefault();
            }
            else
            {
                usrToUpsert.IsGuest = true;
                usrToUpsert.DeleteAfter = DateTimeOffset.Now.AddDays(1);
                usrToUpsert.IsMain = false;
                usrToUpsert.Id = userName;
                collection.InsertOne(usrToUpsert);

                MessagingHelper.PushToLocalAgent(new UserChangeMessage(UserChangeMessage.GuestCreatedTopic, userName));

                return collection.Find(x => x.Id == userName).FirstOrDefault();
            }
        }

        public string GetGuestUserId(User usrToUpsert)
        {
            string userName = usrToUpsert.Id;
            if (usrToUpsert == null)
                userName = SanitizeGuestName(usrToUpsert.Name, usrToUpsert.FirstName);
            if (string.IsNullOrEmpty(userName) && usrToUpsert.MainEmail != null)
                userName = SanitizeGuestName(usrToUpsert.MainEmail);

            if (string.IsNullOrEmpty(userName))
                userName = Guid.NewGuid().ToString("N");
            return userName;
        }

        private string SanitizeGuestName(string name, string firstName = null)
        {
            StringBuilder blr = new StringBuilder();
            if (name != null)
                blr.Append(Sanitize(name));
            if (firstName != null)
                blr.Append(Sanitize(firstName));
            return blr.ToString();
        }

        private string Sanitize(string name)
        {
            StringBuilder blr = new StringBuilder();
            foreach (char c in name)
            {
                if (c >= '0' && c <= '9')
                    blr.Append(c);
                if (c >= 'a' && c <= 'z')
                    blr.Append(c);
                if (c >= 'A' && c <= 'Z')
                    blr.Append(c);
            }
            return blr.ToString();
        }

        [Route("all/{userName}"), HttpPost]
        public User PostUser(string userName, [FromBody] User usrToUpsert, string pwd = null /* pour les tests */)
        {
            if (userName == null)
                return null;
            userName = userName.ToLowerInvariant();
            var collection = MongoDbHelper.GetClient<User>();
            var usr = collection.Find(x => x.Id == userName).FirstOrDefault();
            if (usr != null)
            {
                var arrayUpdate = Builders<User>.Update
              .Set("CommonName", usrToUpsert.CommonName)
              .Set("FirstName", usrToUpsert.FirstName)
              .Set("MainEmail", usrToUpsert.MainEmail)
              .Set("MainPhoneNumber", usrToUpsert.MainPhoneNumber)
              .Set("Name", usrToUpsert.Name)
              .Set("SsmlTaggedName", usrToUpsert.SsmlTaggedName)
              ;

                if (pwd != null)
                {
                    pwd += userName;

                    byte[] hashedBytes;

                    var blr = new StringBuilder();

                    using (SHA256 hash = SHA256Managed.Create())
                    {
                        Encoding enc = Encoding.UTF8;
                        hashedBytes = hash.ComputeHash(enc.GetBytes(pwd));
                    }

                    foreach (var b in hashedBytes)
                        blr.Append(b.ToString("x2"));
                    pwd = blr.ToString();

                    arrayUpdate = arrayUpdate.Set("HashedPassword", pwd); // juste le temps des premiers tests :)
                }

                // les champs updatables
                collection.UpdateOne(x => x.Id == userName, arrayUpdate);

                MessagingHelper.PushToLocalAgent(new UserChangeMessage(UserChangeMessage.UpdatedTopic, userName));

                return collection.Find(x => x.Id == userName).FirstOrDefault();
            }
            else
            {
                usrToUpsert.Id = userName;
                if (!string.IsNullOrEmpty(pwd))
                {
                    pwd += userName;

                    byte[] hashedBytes;

                    var blr = new StringBuilder();

                    using (SHA256 hash = SHA256Managed.Create())
                    {
                        Encoding enc = Encoding.UTF8;
                        hashedBytes = hash.ComputeHash(enc.GetBytes(pwd));
                    }

                    foreach (var b in hashedBytes)
                        blr.Append(b.ToString("x2"));
                    pwd = blr.ToString();
                    usrToUpsert.HashedPassword = pwd;
                }
                collection.InsertOne(usrToUpsert);

                MessagingHelper.PushToLocalAgent(new UserChangeMessage(UserChangeMessage.CreatedTopic, userName));

                return collection.Find(x => x.Id == userName).FirstOrDefault();
            }
        }


        [Route("all/{userName}/avatar/set"), HttpPost]
        public User UpdateAllImages(string userName, [FromBody] UserImageData avatar)
        {
            if (userName == null)
                return null;
            userName = userName.ToLowerInvariant();
            var collection = MongoDbHelper.GetClient<User>();
            var usr = collection.Find(x => x.Id == userName).FirstOrDefault();
            if (usr != null)
            {
                var arrayUpdate = Builders<User>.Update
                    .Set("Avatar", avatar);
                collection.UpdateOne(x => x.Id == userName, arrayUpdate);

                MessagingHelper.PushToLocalAgent(new UserChangeMessage(UserChangeMessage.UpdatedTopic, userName));

                return collection.Find(x => x.Id == userName).FirstOrDefault();
            }
            else
            {
                return null;
            }
        }
        
        public class UserID
        {
            public string Id { get; set; }
            public string Name { get; set; }
        }
        [Route("me/identity"), HttpGet]
        public UserID GetMyUsername()
        {
            var usr = this.User;

            if (usr.IsInRole("User") || usr.IsInRole("Admin"))
            {
                var userId = User.Claims.Where(c => c.Type.Equals(ClaimTypes.NameIdentifier)).FirstOrDefault();
                var userName = User.Claims.Where(c => c.Type.Equals(ClaimTypes.Name)).FirstOrDefault();
                return new UserID() { Id = userId?.Value, Name = userName?.Value };
            }

            return null;
        }


        [Route("me/is-admin"), HttpGet]
        public bool IsAdmin()
        {
            var usr = this.User;

            if (usr.IsInRole("Admin"))
                return true;

            return false;
        }

        [Route("all/{userName}/setmain/{main:bool}"), HttpGet]
        public User ChangeUserIsMain(string userName, bool main)
        {
            if (userName == null)
                return null;
            userName = userName.ToLowerInvariant();
            var collection = MongoDbHelper.GetClient<User>();
            var usr = collection.Find(x => x.Id == userName).FirstOrDefault();
            if (usr != null)
            {
                if (usr.IsGuest)
                    throw new InvalidOperationException("User is a guest !");

                var arrayUpdate = Builders<User>.Update
                    .Set("IsMain", main);
                collection.UpdateOne(x => x.Id == userName, arrayUpdate);
                MessagingHelper.PushToLocalAgent(new UserChangeMessage(UserChangeMessage.UpdatedTopic, userName));

                usr.IsMain = main;
                return usr;
            }
            return null;
        }
    }
}