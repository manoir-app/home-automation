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

namespace Home.Graph.Server.Controllers
{
    public partial class OAuthController : ControllerBase
    {
        private static Dictionary<string, Guid> _zohoStates = new Dictionary<string, Guid>();

        [Route("zoho/geturl"), HttpGet, Authorize(Roles = "User, Admin")]
        public string GetZohoCallUrl(string userId = null)
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

            var clientid = ConfigurationSettingsHelper.GetZohoClientId();
            var req = this.HttpContext.Request;
            string urlRet = req.Host.Host.Equals("localhost", StringComparison.InvariantCultureIgnoreCase) ?
                "http://localhost:51237/oauth/zoho/return" :
                $"https://{req.Host}/oauth/zoho/return";


            if (_zohoStates.ContainsKey(userId))
                _zohoStates.Remove(userId);

            Guid g = Guid.NewGuid();
            _zohoStates.Add(userId, g);

            string scopes = string.Join(' ',
                "MDMOnDemand.MDMInventory.CREATE",
                "MDMOnDemand.MDMInventory.READ");

            return $"https://accounts.zoho.com/oauth/v2/auth?client_id={clientid}" +
                $"&redirect_uri={urlRet}" +
                $"&access_type=offline&state={g.ToString("N")}_{userId}" +
                $"&response_type=code&scope={System.Web.HttpUtility.UrlEncode(scopes)}";
        }


        public class ZohoOauthTokenResult
        {
            public string access_token { get; set; }
            public string refresh_token { get; set; }
            public int expires_in { get; set; }
        }


        [Route("zoho/return"), HttpGet, Authorize(Roles = "User, Admin")]
        public string HandleZohoReturn(string code, string scope, string state)
        {
            var req = this.HttpContext.Request;

            string urlRet = req.Host.Host.Equals("localhost", StringComparison.InvariantCultureIgnoreCase) ?
               "http://localhost:51237/oauth/zoho/return" :
               $"https://{req.Host}/oauth/zoho/return";

            var clientid = ConfigurationSettingsHelper.GetZohoClientId();
            var clientsecret = ConfigurationSettingsHelper.GetZohoClientSecret();
            var urlTw = $"https://accounts.zoho.com/oauth/v2/token"
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
                if (_zohoStates.TryGetValue(parts[1], out g))
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
                var token = JsonConvert.DeserializeObject<ZohoOauthTokenResult>(data);
                if (token != null)
                {
                    var tk = new ExternalTokensController().Put(user, "zoho", new Home.Common.Model.ExternalToken()
                    {
                        ExpiresAt = DateTimeOffset.Now.AddSeconds(-60).AddSeconds(token.expires_in),
                        RefreshToken = token.refresh_token,
                        Token = token.access_token,
                        UserName = user,
                        TokenType = "zoho"
                    });
                    return tk.Id;
                }
            }


            return "none";
        }

        [Route("zoho/token/me"), HttpGet, Authorize(Roles = "Admin,User")]
        public string GetZohoAccessToken()
        {
            if (User.IsInRole("Admin") || User.IsInRole("User"))
            {
                var usr = User;
                return GetZohoAccessToken(UsersController.GetCurrentUserId(User));
            }

            return null;
        }

        [Route("zoho/token/{user}"), HttpGet
            , Authorize(Roles = "Agent,Device")]
        public string GetZohoAccessToken(string user)
        {
            var collection = MongoDbHelper.GetClient<ExternalToken>();
            var lst = collection.Find(x =>
                x.TokenType.Equals("zoho") && x.UserName.Equals(user)).FirstOrDefault();

            if (lst == null)
                return null;

            if (lst.ExpiresAt.HasValue
                && lst.ExpiresAt.Value.AddMinutes(3) < DateTimeOffset.Now)
            {
                var req = this.HttpContext.Request;
                string urlRet = req.Host.Host.Equals("localhost", StringComparison.InvariantCultureIgnoreCase) ?
                       "http://localhost:51237/oauth/zoho/return" :
                       $"https://{req.Host}/oauth/zoho/return";

                var clientid = ConfigurationSettingsHelper.GetZohoClientId();
                var clientsecret = ConfigurationSettingsHelper.GetZohoClientSecret();
                var urlTw = $"https://accounts.zoho.com/oauth/v2/token"
                    + $"?client_id={clientid}"
                    + $"&client_secret={clientsecret}"
                    + $"&refresh_token={lst.RefreshToken}"
                    + $"&redirect_uri={urlRet}"
                    + $"&grant_type=refresh_token";

                try
                {
                    using (var cli = new WebClient())
                    {
                        string data = cli.UploadString(urlTw, "POST", "");
                        var token = JsonConvert.DeserializeObject<ZohoOauthTokenResult>(data);
                        if (token != null)
                        {
                            if (token.expires_in < 0)
                                token.expires_in = 3600;
                            var tk = new ExternalTokensController().Put(user, "zoho", new Home.Common.Model.ExternalToken()
                            {
                                ExpiresAt = DateTimeOffset.Now.AddSeconds(-60).AddSeconds(token.expires_in),
                                RefreshToken = string.IsNullOrEmpty(token.refresh_token) ? lst.RefreshToken : token.refresh_token,
                                Token = token.access_token,
                                UserName = user,
                                TokenType = "zoho"
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
