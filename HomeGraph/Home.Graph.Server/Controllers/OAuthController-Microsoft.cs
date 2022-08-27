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
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Home.Graph.Server.Controllers
{
    public partial class OAuthController : ControllerBase
    {
        private static Dictionary<string, Guid> _states = new Dictionary<string, Guid>();

        [Route("microsoft/geturl"), HttpGet, Authorize(Roles = "User, Admin")]
        public string GetMicrosoftCallUrl(string userId = null, string resource = "graph")
        {
            if (string.IsNullOrEmpty(resource))
                resource = "graph";

            string scopes = "", resourceUri = ""; 


            switch (resource)
            {
                case "graph":
                    resourceUri = "";
                    scopes = string.Join(' ',
                       "User.Read",
                       "Calendars.ReadWrite.Shared",
                       "Device.Command",
                       "Domain.ReadWriteAll",
                       "Family.Read",
                       "Files.ReadWrite.AppFolder",
                       "Notifications.ReadWrite.CreatedByApp",
                       "Presence.Read",
                       "Tasks.ReadWrite.Shared",
                       "UserTimelineActivity.Write.CreatedByApp",
                       "UserNotification.ReadWrite.CreatedByApp",
                       "offline_access",
                       "open_id");
                    break;
                case "azuremanagement":
                case "azuremgmt":
                    resource = "azuremgmt";
                    scopes = "";
                    resourceUri = "https://management.core.windows.net/";
                    break;
            }

            if (userId == null)
            {
                if (User.IsInRole("Admin") || User.IsInRole("User"))
                {
                    var clm = User.Claims.Where(c => c.Type.Equals(ClaimTypes.NameIdentifier)).FirstOrDefault();
                    if (clm != null)
                        userId = clm.Value;
                }
            }

            var clientid = ConfigurationSettingsHelper.GetAzureAdClientId();
            var req = this.HttpContext.Request;
            string urlRet = req.Host.Host.Equals("localhost", StringComparison.InvariantCultureIgnoreCase) ?
                "http://localhost:51237/oauth/microsoft/return" :
                $"https://{req.Host}/oauth/microsoft/return";


            if (_states.ContainsKey(userId))
                _states.Remove(userId);

            Guid g = Guid.NewGuid();
            _states.Add(userId, g);



            StringBuilder blr = new StringBuilder();
            blr.Append("https://login.windows.net/common/oauth2/authorize?client_id=");
            blr.Append(clientid);
            blr.Append("&response_type=code&redirect_uri=");
            blr.Append(urlRet);
            blr.Append("&state=");
            blr.Append(g.ToString("N"));
            blr.Append("_");
            blr.Append(userId);
            blr.Append("_");
            blr.Append(resource);

            if (!string.IsNullOrEmpty(scopes))
            {
                blr.Append("&scope=");
                blr.Append(scopes.Replace(" ", "%20"));
            }
            if(!string.IsNullOrEmpty(resourceUri))
            {
                blr.Append("&resource=");
                blr.Append(resourceUri);
            }

            return blr.ToString();
        }


        internal class AzureTokenResponse
        {
            public string token_type { get; set; }
            public int expires_in { get; set; }
            public string scope { get; set; }
            public string access_token { get; set; }
            public string refresh_token { get; set; }
            public string id_token { get; set; }
        }


        [Route("microsoft/return"), HttpGet, Authorize(Roles = "User, Admin")]
        public string HandleMicrosoftReturn(string code = null, string state = null, string error = null)
        {
            if (error != null)
            {
                return "none";
            }
            if (code == null)
            {
                return "none";
            }

            var req = this.HttpContext.Request;

            string urlRet = req.Host.Host.Equals("localhost", StringComparison.InvariantCultureIgnoreCase) ?
               "http://localhost:51237/oauth/microsoft/return" :
               $"https://{req.Host}/oauth/microsoft/return";

            var clientid = ConfigurationSettingsHelper.GetAzureAdClientId();
            var clientsecret = ConfigurationSettingsHelper.GetAzureAdClientSecret();

            bool okState = false;
            string user = null;
            var parts = state.Split(new char[] { '_' });
            string resourceUri = "https://graph.microsoft.com/";
            string tokentype = "azuread";

            if (parts.Length >= 2)
            {
                user = parts[1];
                Guid g;
                if (_states.TryGetValue(parts[1], out g))
                {
                    if (g.ToString("N").Equals(parts[0], StringComparison.InvariantCultureIgnoreCase))
                    {
                        okState = true;
                    }
                }
            }
            if(parts.Length>=3)
            {
                switch(parts[2].ToLowerInvariant())
                {
                    case "azuremgmt":
                        resourceUri = "https://management.core.windows.net/";
                        tokentype = "azuremgmt";
                        break;
                }
            }

            if (!okState)
                throw new ApplicationException("Données invalides");

            var token = GetMicrosoftTokenFromCode(code, urlRet, resourceUri, clientid, clientsecret);

            if (token != null)
            {
                var tk = new ExternalTokensController().Put(user, tokentype, new Home.Common.Model.ExternalToken()
                {
                    ExpiresAt = DateTimeOffset.Now.AddSeconds(-60).AddSeconds(token.expires_in),
                    RefreshToken = token.refresh_token,
                    Token = token.access_token,
                    UserName = user,
                    TokenType = token.token_type
                });
                return tk.Id;
            }


            return "none";
        }



        private static AzureTokenResponse GetMicrosoftTokenFromCode(string code,
            string calledUrl, string resourceUri, string clientid, string clientSecret)
        {

            try
            {
                HttpWebRequest rq = HttpWebRequest.Create("https://login.windows.net/common/oauth2/token") as HttpWebRequest;
                rq.Method = "POST";
                rq.ContentType = "application/x-www-form-urlencoded";
                StringBuilder blr = new StringBuilder();
                blr.Append("client_id=");
                blr.Append(clientid);
                blr.Append("&client_secret=");
                blr.Append(HttpUtility.UrlEncode(clientSecret));
                blr.Append("&redirect_uri=");
                blr.Append(calledUrl);
                blr.Append("&code=");
                blr.Append(code);
                blr.Append("&grant_type=authorization_code");
                blr.Append("&resource=");
                blr.Append(HttpUtility.UrlEncode(resourceUri));
                string st = blr.ToString();
                rq.ContentLength = st.Length;

                StreamWriter stOut = new StreamWriter(rq.GetRequestStream(), System.Text.Encoding.ASCII);
                stOut.Write(st);
                stOut.Close();

                StreamReader stIn = new StreamReader(rq.GetResponse().GetResponseStream());
                st = stIn.ReadToEnd();
                stIn.Close();

                var tk = JsonConvert.DeserializeObject<AzureTokenResponse>(st);
                return tk;
            }
            catch (WebException ex)
            {
                try
                {
                    string err = ex.Message;
                    using (var srs = new StreamReader(ex.Response.GetResponseStream()))
                    {
                        err = srs.ReadToEnd();
                    }
                }
                catch
                {
                }
            }
            catch (Exception ex)
            {
            }

            return null;
        }


        [Route("microsoft/token/me"), HttpGet, Authorize(Roles = "Admin,User")]
        public string GetMicrosoftAcessToken(string type="azuread")
        {
            if (User.IsInRole("Admin") || User.IsInRole("User"))
            {
                var usr = User;
                return GetMicrosoftAccessToken(UsersController.GetCurrentUserId(User));
            }

            return null;
        }

        [Route("microsoft/token/{user}"), HttpGet
            , Authorize(Roles = "Agent,Device")]
        public string GetMicrosoftAccessToken(string user, string type="azuread")
        {
            var collection = MongoDbHelper.GetClient<ExternalToken>();
            var lst = collection.Find(x =>
                x.TokenType.Equals(type) && x.UserName.Equals(user)).FirstOrDefault();

            if (lst == null)
                return null;

            if (lst.ExpiresAt.HasValue
                && lst.ExpiresAt.Value.AddMinutes(3) < DateTimeOffset.Now)
            {
                return RefreshMicrosoftToken(user, type, lst)?.Token;
            }

            return lst.Token;
        }

        internal static ExternalToken RefreshMicrosoftToken(string user, string type, ExternalToken lst)
        {
            var clientid = ConfigurationSettingsHelper.GetAzureAdClientId();
            var clientsecret = ConfigurationSettingsHelper.GetAzureAdClientSecret();
            string resourceUri = "https://graph.microsoft.com/";
            switch (type)
            {
                case "azuremgmt":
                    resourceUri = "https://management.core.windows.net/";
                    break;
            }

            var token = RefreshMicrosoftToken(clientid, clientsecret, lst.RefreshToken, resourceUri);
            if (token != null)
            {
                if (token.expires_in < 0)
                    token.expires_in = 3600;
                var tk = new ExternalTokensController().Put(user, type, new Home.Common.Model.ExternalToken()
                {
                    ExpiresAt = DateTimeOffset.Now.AddSeconds(-60).AddSeconds(token.expires_in),
                    RefreshToken = token.refresh_token,
                    Token = token.access_token,
                    UserName = user,
                    TokenType = token.token_type
                });
                return tk;
            }
            else
                return null;
        }

        internal static AzureTokenResponse RefreshMicrosoftToken(string clientid, string clientsecret, string refreshtoken, string resource)
        {

            try
            {
                HttpWebRequest rq = HttpWebRequest.Create("https://login.windows.net/common/oauth2/token") as HttpWebRequest;
                rq.Method = "POST";
                rq.ContentType = "application/x-www-form-urlencoded";
                StringBuilder blr = new StringBuilder();
                blr.Append("client_id=");
                blr.Append(clientid);
                blr.Append("&client_secret=");
                blr.Append(HttpUtility.UrlEncode(clientsecret));
                blr.Append("&refresh_token=");
                blr.Append(refreshtoken);
                blr.Append("&grant_type=refresh_token");
                blr.Append("&resource=");
                blr.Append(resource);
                string st = blr.ToString();
                rq.ContentLength = st.Length;

                StreamWriter stOut = new StreamWriter(rq.GetRequestStream(), System.Text.Encoding.ASCII);
                stOut.Write(st);
                stOut.Close();

                StreamReader stIn = new StreamReader(rq.GetResponse().GetResponseStream());
                st = stIn.ReadToEnd();
                stIn.Close();

                var tk = JsonConvert.DeserializeObject<AzureTokenResponse>(st);
                return tk;
            }
            catch (WebException ex)
            {
                try
                {
                    string err = ex.Message;
                    using (var srs = new StreamReader(ex.Response.GetResponseStream()))
                    {
                        err = srs.ReadToEnd();
                    }
                }
                catch
                {
                }
            }
            catch (Exception ex)
            {
            }

            return null;
        }

    }
}
