using Home.Common.Model;
using Home.Graph.Common;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Claims;
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

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            Device user = null;
            try
            {
                var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
                var credentialBytes = Convert.FromBase64String(authHeader.Parameter);
                var credentials = Encoding.UTF8.GetString(credentialBytes).Split(new[] { ':' }, 2);
                var username = credentials[0];
                var password = credentials[1];

                var passwordToGet = Environment.GetEnvironmentVariable("HOMEAUTOMATION_APIKEY");
#if DEBUG
                passwordToGet = "12345678";
#endif
                if (!password.Equals(passwordToGet))
                    return Task.FromResult(AuthenticateResult.Fail("Error Occured.Authorization failed."));

                var ctl = MongoDbHelper.GetClient<Device>();
                user = ctl.Find(x => x.Id.Equals(username) && x.MeshId.Equals("local")).FirstOrDefault();
                if(user == null)
                    return Task.FromResult(AuthenticateResult.Fail("Device not found."));

            }
            catch
            {
                return Task.FromResult(AuthenticateResult.Fail("Error Occured.Authorization failed."));
            }

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
