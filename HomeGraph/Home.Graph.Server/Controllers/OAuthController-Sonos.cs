using Home.Common;
using Home.Common.Model;
using Home.Graph.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;

namespace Home.Graph.Server.Controllers
{
    public partial class OAuthController : ControllerBase
    {
        private static Dictionary<string, Guid> _sonosStates = new Dictionary<string, Guid>();

        [Route("sonos/geturl"), HttpGet, Authorize(Roles = "User, Admin")]
        public string GetSonosCallUrl(string userId = null)
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

            var clientid = ConfigurationSettingsHelper.GetSonosClientId();
            var req = this.HttpContext.Request;
            string urlRet = req.Host.Host.Equals("localhost", StringComparison.InvariantCultureIgnoreCase) ?
                "https://localhost:5001/oauth/sonos/return" :
                $"https://{req.Host}/oauth/sonos/return";


            if (_sonosStates.ContainsKey(userId))
                _sonosStates.Remove(userId);

            Guid g = Guid.NewGuid();
            _sonosStates.Add(userId, g);

            string scopes = string.Join(' ',
                "playback-control-all");

            return $"https://api.sonos.com/login/v3/oauth?client_id={clientid}" +
                $"&redirect_uri={urlRet}" +
                $"&state={g.ToString("N")}_{userId}" +
                $"&response_type=code&scope={System.Web.HttpUtility.UrlEncode(scopes)}";
        }


        public class SonosOauthTokenResult
        {
            public string access_token { get; set; }
            public string refresh_token { get; set; }
            public int expires_in { get; set; }
            public string scope { get; set; }
            public string token_type { get; set; }
        }


        [Route("sonos/return"), HttpGet, Authorize(Roles = "User, Admin")]
        public string HandleSonosReturn(string code, string scope, string state)
        {
            var req = this.HttpContext.Request;

            string urlRet = req.Host.Host.Equals("localhost", StringComparison.InvariantCultureIgnoreCase) ?
               "https://localhost:5001/oauth/sonos/return" :
               $"https://{req.Host}/oauth/sonos/return";

            var clientid = ConfigurationSettingsHelper.GetSonosClientId();
            var clientsecret = ConfigurationSettingsHelper.GetSonosClientSecret();
            var urlTw = $"https://api.sonos.com/login/v3/oauth/access"
                + $"?code={code}"
                + $"&grant_type=authorization_code"
                + $"&redirect_uri={urlRet}";

            bool okState = false;
            string user = null;
            var parts = state.Split(new char[] { '_' });
            if (parts.Length == 2)
            {
                user = parts[1];
                Guid g;
                if (_sonosStates.TryGetValue(parts[1], out g))
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
                string aut = clientid + ":" + clientsecret;
                aut = Convert.ToBase64String(Encoding.ASCII.GetBytes(aut));
                cli.Headers.Add("Authorization", $"Basic {aut}");
                string data = cli.UploadString(urlTw, "POST", "");
                var token = JsonConvert.DeserializeObject<SonosOauthTokenResult>(data);
                if (token != null)
                {
                    var tk = new ExternalTokensController().Put(user, "sonos", new Home.Common.Model.ExternalToken()
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

        [Route("sonos/token/me"), HttpGet, Authorize(Roles = "Admin,User")]
        public string GetsonosAccessToken()
        {
            if (User.IsInRole("Admin") || User.IsInRole("User"))
            {
                var usr = User;
                return GetSonosAccessToken(UsersController.GetCurrentUserId(User));
            }

            return null;
        }

        [Route("sonos/token/{user}"), HttpGet
            , Authorize(Roles = "Agent,Device")]
        public string GetSonosAccessToken(string user)
        {
            var collection = MongoDbHelper.GetClient<ExternalToken>();
            var lst = collection.Find(x =>
                x.TokenType.Equals("sonos") && x.UserName.Equals(user)).FirstOrDefault();

            if (lst == null)
                return null;

            if (lst.ExpiresAt.HasValue
                && lst.ExpiresAt.Value.AddMinutes(3) < DateTimeOffset.Now)
            {
                var clientid = ConfigurationSettingsHelper.GetSonosClientId();
                var clientsecret = ConfigurationSettingsHelper.GetSonosClientSecret();
                var urlTw = $"https://api.sonos.com/login/v3/oauth/access"
                    + $"refresh_token={lst.RefreshToken}"
                    + $"&grant_type=refresh_token";

                try
                {
                    using (var cli = new WebClient())
                    {
                        string aut = clientid + ":" + clientsecret;
                        aut = Convert.ToBase64String(Encoding.ASCII.GetBytes(aut));
                        cli.Headers.Add("Authorization", $"Basic {aut}");

                        string data = cli.UploadString(urlTw, "POST", "");
                        var token = JsonConvert.DeserializeObject<SonosOauthTokenResult>(data);
                        if (token != null)
                        {
                            if (token.expires_in < 0)
                                token.expires_in = 3600;
                            var tk = new ExternalTokensController().Put(user, "sonos", new Home.Common.Model.ExternalToken()
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
