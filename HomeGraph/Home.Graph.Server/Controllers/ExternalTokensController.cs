using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Home.Graph.Common;
using Home.Common.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Security.Claims;
using Home.Common.Messages;

namespace Home.Graph.Server.Controllers
{
    [Route("v1.0/security/tokens")]
    [ApiController]
    public class ExternalTokensController : ControllerBase
    {
        public class TokenForAdminPage
        {
            public string User { get; set; }
            public string TokenType { get; set; }
            public string ExpiresAt { get; set; }
        }

        [Route("self/foradmin")]
        public List<TokenForAdminPage> ForAdmin()
        {
            var user = User.Claims.FirstOrDefault(x => x.Type.Equals(ClaimTypes.NameIdentifier))?.Value;

            var collection = MongoDbHelper.GetClient<ExternalToken>();
            var lst = collection.Find(x => x.UserName== "system" || x.UserName==user).ToList();


            List<TokenForAdminPage> ret = (from z in lst
                                           select new TokenForAdminPage()
                                           {
                                               User = z.UserName,
                                               TokenType = z.TokenType,
                                           }).ToList();

            return ret;
        }

        [Route("all"), HttpGet]
        public List<ExternalToken> GetAll()
        {
            var collection = MongoDbHelper.GetClient<ExternalToken>();
            var lst = collection.Find(x => true).ToList();
            return lst;
        }

        [Route("{user}/{type}"), HttpGet]
        public List<ExternalToken> Get(string user, string type)
        {
            if (user != null)
                user = user.ToLowerInvariant();

            var collection = MongoDbHelper.GetClient<ExternalToken>();
            var lst = collection.Find(x => 
                x.TokenType.Equals(type) && x.UserName.Equals(user)).ToList();

            switch(type)
            {
                case "azuremgmt":
                case "azuread":
                    for (int i=0;i<lst.Count;i++)
                    {
                        var t = lst[i];
                        if (t.ExpiresAt.HasValue
                            && t.ExpiresAt.Value.AddMinutes(3) < DateTimeOffset.Now)
                        {
                            lst[i] = OAuthController.RefreshMicrosoftToken(user, type, t);
                        }
                    }
                    break;
            }

            return lst;
        }


        [Route("{user}/{type}"), HttpPut]
        public ExternalToken Put(string user, string type, [FromBody] ExternalToken token)
        {
            if (user != null)
                user = user.ToLowerInvariant();

            var collection = MongoDbHelper.GetClient<ExternalToken>();
            if (token.Id == null)
                token.Id = user + "_" + type;

            token.UserName = user;
            token.TokenType = type;

            var exists = collection.Find(x =>
                x.TokenType.Equals(type) && x.UserName.Equals(user)).FirstOrDefault();
            if (exists == null)
            {
                collection.InsertOne(token);
                MessagingHelper.PushToLocalAgent(new NewExternalTokenMessage()
                {
                    UserId = user,
                    TokenType = type
                });
            }
            else
                collection.ReplaceOne(x => x.TokenType.Equals(type) && x.UserName.Equals(user), token);

            return collection.Find(x =>
                x.TokenType.Equals(type) && x.UserName.Equals(user)).FirstOrDefault();
        }

        [Route("{user}/{type}"), HttpDelete]
        public bool Delete(string user, string type)
        {
            if (user != null)
                user = user.ToLowerInvariant();

            var collection = MongoDbHelper.GetClient<ExternalToken>();
            var exists = collection.Find(x =>
                x.TokenType.Equals(type) && x.UserName.Equals(user)).FirstOrDefault();
            if (exists != null)
            {
                var res = collection.DeleteOne(x =>
                x.TokenType.Equals(type) && x.UserName.Equals(user));

                if(res.IsAcknowledged && res.DeletedCount > 0)
                {
                    MessagingHelper.PushToLocalAgent(new ExternalTokenDeletedMessage()
                    {
                        UserId = user,
                        TokenType = type
                    });
                }

                return true;
            }
            else
                return false;
        }

    }
}
