using Home.Common;
using Home.Common.Model;
using Home.Graph.Common;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Home.Graph.Server.Controllers
{
    partial class UsersController
    {
        public class Credential
        {
            public string login { get; set; }
            public string pwd { get; set; }
        }

        [Route("login"), AllowAnonymous(), HttpPost]
        public async Task<ActionResult> Login([FromBody] Credential creds)
        {
            User user = GetUser(creds);

            bool isAuth = user != null;

            if (!isAuth)
                return Unauthorized();

            MqttHelper.PublishUserLogin(user, "graph");

            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.Name + " " + user.FirstName),
                //new Claim(ClaimTypes.Email, user.MainEmail),
                new Claim(ClaimTypes.Role, user.IsMain ? "Admin":"User"),
            };

            var authProperties = new AuthenticationProperties()
            {
                IsPersistent = true,
                ExpiresUtc = DateTime.Now.AddDays(365)
            };

            var claimsIdentity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);
            return new OkObjectResult(user);
        }

        private static User GetUser(Credential creds)
        {
            User user;
            if (creds == null || string.IsNullOrEmpty(creds.login) || string.IsNullOrEmpty(creds.pwd))
                return null;
            string userName = creds.login.ToLowerInvariant();
            var pwd = creds.pwd;
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

            var collection = MongoDbHelper.GetClient<User>();
            user = collection.Find(x => x.Id == userName && x.HashedPassword != null && x.HashedPassword == pwd).FirstOrDefault();
            if (user != null)
                user.MinimizeData();

            return user;
        }

        [Route("logout"), Authorize(Roles = "admin,user"), HttpGet]
        public ActionResult Logout()
        {
            try
            {
                HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);
                return new OkObjectResult(true);
            }
            catch
            {
                return new BadRequestResult();
            }
        }


        public class CredentialFromDevice : Credential
        {
            public string deviceInternalName { get; set; }
            public string deviceKind { get; set; }
        }

        public class LoginFromDeviceResponse
        {
            public User User { get; set; }
            public Device Device { get; set; }
            public AutomationMesh Mesh { get; set; }
            public string DeviceApiKey { get; set; }
        }


        private class AuthenticationCookie
        {
            public string AuthKind { get; set; }
            public int ExpirationEpoch { get; set; }
            public string Token { get; set; }
        }

        [Route("login/device"), AllowAnonymous(), HttpPost]
        public LoginFromDeviceResponse LoginOnDevice([FromBody] CredentialFromDevice creds,
            bool associateWithUser = false,
            bool addCookie = false)
        {
            var usr = GetUser(creds);
            if (usr == null)
                return null;

            var ret = new LoginFromDeviceResponse()
            {
                User = usr,
                Mesh = new AutomationMeshController(_hubContext, _appContext).Get("local")
            };

            var collDev = MongoDbHelper.GetClient<Device>();
            var itm = collDev.Find(x => x.MeshId == "local"
                    && x.DeviceInternalName != null
                    && x.DeviceInternalName.Equals(creds.deviceInternalName)).FirstOrDefault();
            if (itm == null)
            {
                itm = new Device()
                {
                    DeviceAgentId = null,
                    DeviceGivenName = creds.deviceInternalName,
                    DeviceInternalName = creds.deviceInternalName,
                    DeviceKind = creds.deviceKind,
                    Id = Guid.NewGuid().ToString("D"),
                    MeshId = "local",
                };
                if (associateWithUser)
                    itm.AssignatedUserId = usr.Id;
                collDev.InsertOne(itm);
                ret.Device = itm;
            }
            else
            {
                ret.Device = itm;
                if(string.IsNullOrEmpty(itm.AssignatedUserId) && associateWithUser)
                {
                    var up = Builders<Device>.Update.Set("AssignatedUserId", usr.Id);
                    collDev.UpdateOne(x => x.Id == itm.Id, up);
                }
            }

            var passwordToGet = LocalDebugHelper.GetApiKey();
            if (passwordToGet == null)
                throw new InvalidOperationException("Api key not set");

            MqttHelper.PublishUserLogin(usr, "device/" + ret.Device.DeviceInternalName);

            string tmp = ret.Device.Id + ":" + passwordToGet;
            tmp = Convert.ToBase64String(Encoding.ASCII.GetBytes(tmp));
            ret.DeviceApiKey = tmp;

            MessagingHelper.PushToMobileApp(itm.AssignatedUserId, new UserNotification()
            {
                Category = "ApplicationNotifs",
                Title = "Nouveau device connecté !",
                Description = "Un nouveau device mobile vient de se connecter à votre serveur maNoir. Si ce n'est pas vous, nous vous invitons à vérifier tout de suite la sécurité de votre réseau et à changer votre mot de passe",
                UserId = itm.AssignatedUserId
            });


            if(addCookie)
            {
                var cookie = new AuthenticationCookie()
                {
                    AuthKind = "PERMANENT",
                    ExpirationEpoch = int.MaxValue,
                    Token = tmp
                };

                var uri = new Uri(HomeServerHelper.GetLocalGraphUrl());

                Response.Cookies.Append("ManoirDeviceAuth", JsonConvert.SerializeObject(cookie), new CookieOptions()
                {
                    Expires = null,
                    IsEssential = true,
                    SameSite = SameSiteMode.Lax,
                    Domain = uri.Host
                });
            }

            return ret;
        }

    }

}