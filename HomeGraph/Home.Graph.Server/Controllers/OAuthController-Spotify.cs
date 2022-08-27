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
        private static Dictionary<string, Guid> _spotifyStates = new Dictionary<string, Guid>();

        [Route("spotify/geturl"), HttpGet, Authorize(Roles = "User, Admin")]
        public string GetSpotifyCallUrl(string userId = null)
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

            var clientid = ConfigurationSettingsHelper.GetSpotifyClientId();
            var req = this.HttpContext.Request;
            string urlRet = req.Host.Host.Equals("localhost", StringComparison.InvariantCultureIgnoreCase) ?
                "http://localhost:51237/oauth/spotify/return" :
                $"https://{req.Host}/oauth/spotify/return";


            if (_spotifyStates.ContainsKey(userId))
                _spotifyStates.Remove(userId);

            Guid g = Guid.NewGuid();
            _spotifyStates.Add(userId, g);

            string scopes = string.Join(' ',
                "user-library-read",
                "user-read-currently-playing",
                "user-follow-read",
                "user-modify-playback-state",
                "user-read-recently-played",
                "user-read-playback-position",
                "user-library-modify",
                "user-read-playback-state",
                "playlist-modify-private",
                "playlist-read-private");

            return $"https://accounts.spotify.com/authorize?client_id={clientid}" +
                $"&redirect_uri={urlRet}" +
                $"&state={g.ToString("N")}_{userId}" +
                $"&response_type=code&scope={System.Web.HttpUtility.UrlEncode(scopes)}";
        }


        public class SpotifyOauthTokenResult
        {
            public string access_token { get; set; }
            public string refresh_token { get; set; }
            public int expires_in { get; set; }
            public string scope { get; set; }
            public string token_type { get; set; }
        }


        [Route("spotify/return"), HttpGet, Authorize(Roles = "User, Admin")]
        public string HandleSpotifyReturn(string code, string scope, string state)
        {
            var req = this.HttpContext.Request;

            string urlRet = req.Host.Host.Equals("localhost", StringComparison.InvariantCultureIgnoreCase) ?
               "http://localhost:51237/oauth/spotify/return" :
               $"https://{req.Host}/oauth/spotify/return";

            var clientid = ConfigurationSettingsHelper.GetSpotifyClientId();
            var clientsecret = ConfigurationSettingsHelper.GetSpotifyClientSecret();
            var urlTw = $"https://accounts.spotify.com/api/token";
            var data = "code={code}"
                + $"&grant_type=authorization_code"
                + $"&redirect_uri={Uri.EscapeDataString(urlRet)}";

            bool okState = false;
            string user = null;
            var parts = state.Split(new char[] { '_' });
            if (parts.Length == 2)
            {
                user = parts[1];
                Guid g;
                if (_spotifyStates.TryGetValue(parts[1], out g))
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
                cli.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                data = cli.UploadString(urlTw, "POST", data);
                var token = JsonConvert.DeserializeObject<SpotifyOauthTokenResult>(data);
                if (token != null)
                {
                    var tk = new ExternalTokensController().Put(user, "spotify", new Home.Common.Model.ExternalToken()
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

        [Route("spotify/token/me"), HttpGet, Authorize(Roles = "Admin,User")]
        public string GetspotifyAccessToken()
        {
            if (User.IsInRole("Admin") || User.IsInRole("User"))
            {
                var usr = User;
                return GetSpotifyAccessToken(UsersController.GetCurrentUserId(User));
            }

            return null;
        }

        [Route("spotify/token/{user}"), HttpGet
            , Authorize(Roles = "Agent,Device")]
        public string GetSpotifyAccessToken(string user)
        {
            var collection = MongoDbHelper.GetClient<ExternalToken>();
            var lst = collection.Find(x =>
                x.TokenType.Equals("spotify") && x.UserName.Equals(user)).FirstOrDefault();

            if (lst == null)
                return null;

            if (lst.ExpiresAt.HasValue
                && lst.ExpiresAt.Value.AddMinutes(3) < DateTimeOffset.Now)
            {
                var clientid = ConfigurationSettingsHelper.GetSpotifyClientId();
                var clientsecret = ConfigurationSettingsHelper.GetSpotifyClientSecret();
                var urlTw = $"https://id.spotify.tv/oauth2/token"
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
                        var token = JsonConvert.DeserializeObject<SpotifyOauthTokenResult>(data);
                        if (token != null)
                        {
                            if (token.expires_in < 0)
                                token.expires_in = 3600;
                            var tk = new ExternalTokensController().Put(user, "spotify", new Home.Common.Model.ExternalToken()
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
