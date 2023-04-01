using Home.Common;
using Home.Common.Model;
using Home.Graph.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Home.Graph.Server.Controllers
{
    public partial class OAuthController : ControllerBase
    {
        private static Dictionary<string, Guid> _twitchStates = new Dictionary<string, Guid>();

        [Route("twitch/geturl"), HttpGet, Authorize(Roles = "User, Admin")]
        public string GetTwitchCallUrl(string userId = null)
        {
            if (userId == null)
            {
                if (User.IsInRole("Admin") || User.IsInRole("User"))
                {
                    var clm = User.Claims.Where(c => c.Type.Equals(ClaimTypes.NameIdentifier)).FirstOrDefault();
                    if (clm != null)
                        userId = clm.Value;
                }
            }

            var clientid = ConfigurationSettingsHelper.GetTwitchClientId();
            var req = this.HttpContext.Request;
            string urlRet = req.Host.Host.Equals("localhost", StringComparison.InvariantCultureIgnoreCase) ?
                "https://localhost:5001/oauth/twitch/return" :
                $"https://{req.Host}/oauth/twitch/return";


            if (_twitchStates.ContainsKey(userId))
                _twitchStates.Remove(userId);

            Guid g = Guid.NewGuid();
            _twitchStates.Add(userId, g);

            string scopes = string.Join(' ',
                "user:read:broadcast",
                "user:edit:broadcast");

            return $"https://id.twitch.tv/oauth2/authorize?client_id={clientid}" +
                $"&redirect_uri={urlRet}" +
                $"&state={g.ToString("N")}_{userId}" +
                $"&response_type=code&scope={System.Web.HttpUtility.UrlEncode(scopes)}";
        }


        public class TwitchOauthTokenResult
        {
            public string access_token { get; set; }
            public string refresh_token { get; set; }
            public int expires_in { get; set; }
            public string[] scope { get; set; }
            public string token_type { get; set; }
        }


        [Route("twitch/return"), HttpGet, Authorize(Roles = "User, Admin")]
        public string HandleTwitchReturn(string code, string scope, string state)
        {
            var req = this.HttpContext.Request;

            string urlRet = req.Host.Host.Equals("localhost", StringComparison.InvariantCultureIgnoreCase) ?
               "https://localhost:5001/oauth/twitch/return" :
               $"https://{req.Host}/oauth/twitch/return";

            var clientid = ConfigurationSettingsHelper.GetTwitchClientId();
            var clientsecret = ConfigurationSettingsHelper.GetTwitchClientSecret();
            var urlTw = $"https://id.twitch.tv/oauth2/token"
                + $"?client_id={clientid}"
                + $"&client_secret={clientsecret}"
                + $"&code={code}"
                + $"&grant_type=authorization_code"
                + $"&redirect_uri={urlRet}";

            bool okState = false;
            string user = null;
            var parts = state.Split(new char[] { '_' });
            if (parts.Length == 2)
            {
                user = parts[1];
                Guid g;
                if (_twitchStates.TryGetValue(parts[1], out g))
                {
                    if (g.ToString("N").Equals(parts[0], StringComparison.InvariantCultureIgnoreCase))
                    {
                        okState = true;
                    }
                }
            }

            if (!okState)
                throw new ApplicationException("Données invalides");

            using (var cli = new WebClient())
            {
                string data = cli.UploadString(urlTw, "POST", "");
                var token = JsonConvert.DeserializeObject<TwitchOauthTokenResult>(data);
                if (token != null)
                {
                    var tk = new ExternalTokensController().Put(user, "twitch", new Home.Common.Model.ExternalToken()
                    {
                        ExpiresAt = DateTimeOffset.Now.AddSeconds(-60).AddSeconds(token.expires_in),
                        RefreshToken = token.refresh_token,
                        Token = token.access_token,
                        UserName = user,
                        TokenType = token.token_type
                    });
                    return tk.Id;
                }
            }


            return "none";
        }

        [Route("twitch/token/me"), HttpGet, Authorize(Roles = "Admin,User")]
        public string GetTwitchAccessToken()
        {
            if (User.IsInRole("Admin") || User.IsInRole("User"))
            {
                var usr = User;
                return GetTwitchAccessToken(UsersController.GetCurrentUserId(User));
            }

            return null;
        }

        [Route("twitch/token/{user}"), HttpGet
            , Authorize(Roles = "Agent,Device")]
        public string GetTwitchAccessToken(string user)
        {
            var collection = MongoDbHelper.GetClient<ExternalToken>();
            var lst = collection.Find(x =>
                x.TokenType.Equals("twitch") && x.UserName.Equals(user)).FirstOrDefault();

            if (lst == null)
                return null;

            if (lst.ExpiresAt.HasValue
                && lst.ExpiresAt.Value.AddMinutes(3) < DateTimeOffset.Now)
            {
                var clientid = ConfigurationSettingsHelper.GetTwitchClientId();
                var clientsecret = ConfigurationSettingsHelper.GetTwitchClientSecret();
                var urlTw = $"https://id.twitch.tv/oauth2/token"
                    + $"?client_id={clientid}"
                    + $"&client_secret={clientsecret}"
                    + $"&refresh_token={lst.RefreshToken}"
                    + $"&grant_type=refresh_token";

                try
                {
                    using (var cli = new WebClient())
                    {
                        string data = cli.UploadString(urlTw, "POST", "");
                        var token = JsonConvert.DeserializeObject<TwitchOauthTokenResult>(data);
                        if (token != null)
                        {
                            if (token.expires_in < 0)
                                token.expires_in = 3600;
                            var tk = new ExternalTokensController().Put(user, "twitch", new Home.Common.Model.ExternalToken()
                            {
                                ExpiresAt = DateTimeOffset.Now.AddSeconds(-60).AddSeconds(token.expires_in),
                                RefreshToken = token.refresh_token,
                                Token = token.access_token,
                                UserName = user,
                                TokenType = token.token_type
                            });
                            return token.access_token;
                        }
                        else
                            return null;
                    }
                }
                catch (WebException ex)
                {
                    var httpResp = ex.Response as HttpWebResponse;
                    if (httpResp.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        collection.DeleteOne(x => x.Id == lst.Id);
                    }
                    return null;
                }
            }

            else
                return lst.Token;


        }



    }
}
