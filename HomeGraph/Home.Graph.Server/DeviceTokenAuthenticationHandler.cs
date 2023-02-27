using Home.Common.Model;
using Home.Graph.Common;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Home.Graph.Server
{
    public class DeviceTokenAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public DeviceTokenAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
        }

        protected override Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            return base.HandleChallengeAsync(properties);
        }

        private class AuthenticationCookie
        {
            public string Token { get; set; }
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            AuthenticationHeaderValue authHeader = null;
            try
            {
                string auth = Request.Headers["Authorization"];
                if (!string.IsNullOrEmpty(auth))
                {
                    authHeader = AuthenticationHeaderValue.Parse(auth);
                    if (authHeader != null)
                    {
                        switch (authHeader.Scheme.ToLowerInvariant())
                        {
                            case "bearer":
                                return AuthDevice(authHeader);
                            case "signature": // smartthings à priori
                                return AuthSignature(authHeader);
                        }
                    }
                }

                if(Request.Cookies.TryGetValue("ManoirDeviceAuth", out string cookie))
                {
                    var tmp = JsonConvert.DeserializeObject<AuthenticationCookie>(cookie);
                    if (tmp != null && !string.IsNullOrEmpty(tmp.Token))
                        return AuthDevice(tmp.Token);
                }

            }
            catch (Exception ex)
            {
                return Task.FromResult(AuthenticateResult.Fail($"Error Occured ({ex.Message}). Authorization failed."));
            }

            return Task.FromResult(AuthenticateResult.Fail("No valid scheme.Authorization failed."));

        }

        private Task<AuthenticateResult> AuthSignature(AuthenticationHeaderValue authHeader)
        {
            var principal = new GenericPrincipal(new GenericIdentity("smarthings"), new string[] { "Integration" });

            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }

        private Task<AuthenticateResult> AuthDevice(AuthenticationHeaderValue authHeader)
        {
            var credentialBytes = Convert.FromBase64String(authHeader.Parameter);
            var credentialToken = Encoding.UTF8.GetString(credentialBytes);
            return AuthDevice(credentialToken);
        }

        private Task<AuthenticateResult> AuthDevice(string credentialToken)
        {
            Device user = null;
            var credentials = credentialToken.Split(new[] { ':' }, 2);
            if (credentials.Length != 2)
                return Task.FromResult(AuthenticateResult.Fail("Error Occured.Authorization failed - NO API KEY."));

            var username = credentials[0];
            var password = credentials[1];

            var passwordToGet = LocalDebugHelper.GetApiKey();
            if (passwordToGet == null)
                return Task.FromResult(AuthenticateResult.Fail("Error Occured.Authorization failed - NO API KEY."));

            if (!password.Equals(passwordToGet))
                return Task.FromResult(AuthenticateResult.Fail("Error Occured.Authorization failed."));

            var ctl = MongoDbHelper.GetClient<Device>();
            user = ctl.Find(x => x.Id.Equals(username) && x.MeshId.Equals("local")).FirstOrDefault();
            if (user == null)
                return Task.FromResult(AuthenticateResult.Fail("Device not found."));

            if (user == null)
                return Task.FromResult(AuthenticateResult.Fail("Invalid Credentials"));

            var claims = new List<Claim>(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.DeviceGivenName),
                new Claim(ClaimTypes.Role, "Device"),
            });

            if (user.AssignatedUserId != null)
                claims.Add(new Claim("AssociatedUserId", user.AssignatedUserId));

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);

            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
