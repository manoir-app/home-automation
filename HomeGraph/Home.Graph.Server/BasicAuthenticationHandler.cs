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
    public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public BasicAuthenticationHandler(
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

        private static List<Agent> _allAgents = new List<Agent>();

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            Agent user = null;
            try
            {
                var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
                var credentialBytes = Convert.FromBase64String(authHeader.Parameter);
                var credentials = Encoding.UTF8.GetString(credentialBytes).Split(new[] { ':' }, 2);
                var username = credentials[0];
                var password = credentials[1];

                var passwordToGet = LocalDebugHelper.GetApiKey();
                if(passwordToGet == null)
                    return Task.FromResult(AuthenticateResult.Fail("Error Occured.Authorization failed - NO API KEY."));

                if (!password.Equals(passwordToGet))
                    return Task.FromResult(AuthenticateResult.Fail("Error Occured.Authorization failed."));

                bool justloaded = false;
                var coll = MongoDbHelper.GetClient<Agent>();
                if (_allAgents == null)
                {
                    _allAgents = coll.Find(x => x.AgentMesh == "local").ToList();
                    justloaded = true;
                }
                user = (from z in _allAgents
                        where z.AgentName.Equals(username)
                        select z).FirstOrDefault();
                if (user == null && !justloaded)
                {
                    _allAgents = coll.Find(x => x.AgentMesh == "local").ToList();
                    justloaded = true;
                }
                user = (from z in _allAgents
                        where z.AgentName.Equals(username)
                        select z).FirstOrDefault();
            }
            catch
            {
                return Task.FromResult(AuthenticateResult.Fail("Error Occured.Authorization failed."));
            }

            if (user == null)
                return Task.FromResult(AuthenticateResult.Fail("Invalid Credentials"));

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.AgentName),
                new Claim(ClaimTypes.Role, "Agent"),
            };

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);

            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
