// From https://github.com/Kukkimonsuta/Odachi/tree/master/src/Odachi.AspNetCore.Authentication.Basic

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

namespace Odachi.AspNetCore.Authentication.Basic {
    internal class BasicHandler : AuthenticationHandler<BasicOptions> {
        public const string RequestHeaderPrefix = "Basic ";

        public BasicHandler(IOptionsMonitor<BasicOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
            : base(options, logger, encoder, clock) {
        }

        /// <summary>
        /// The handler calls methods on the events which give the application control at certain points where processing is occurring.
        /// If it is not provided a default instance is supplied which does nothing when the methods are called.
        /// </summary>
        protected new BasicEvents Events {
            get { return (BasicEvents)base.Events; }
            set { base.Events = value; }
        }

        protected override Task<object> CreateEventsAsync() => Task.FromResult<object>(new BasicEvents());

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync() {
            try {
                // .NET Core does not support HTTP auth on sockets
                if (Uri.TryCreate(CurrentUri, UriKind.Absolute, out var uri)) {
                    if (uri.IsLoopback && !Request.IsHttps) {
                        var t = CreateTicket("RUser", string.Empty);
                        return AuthenticateResult.Success(t);
                    }
                }

                // retrieve authorization header
                string authorization = Request.Headers[HeaderNames.Authorization];

                if (string.IsNullOrEmpty(authorization)) {
                    return AuthenticateResult.NoResult();
                }

                if (!authorization.StartsWith(RequestHeaderPrefix, StringComparison.OrdinalIgnoreCase)) {
                    return AuthenticateResult.NoResult();
                }

                // retrieve credentials from header
                var encodedCredentials = authorization.Substring(RequestHeaderPrefix.Length);
                var decodedCredentials = default(string);
                try {
                    decodedCredentials = Encoding.UTF8.GetString(Convert.FromBase64String(encodedCredentials));
                } catch (Exception) {
                    return AuthenticateResult.Fail("Invalid basic authentication header encoding.");
                }

                var index = decodedCredentials.IndexOf(':');
                if (index == -1) {
                    return AuthenticateResult.Fail("Invalid basic authentication header format.");
                }

                var username = decodedCredentials.Substring(0, index);
                var password = decodedCredentials.Substring(index + 1);

                // invoke sign in event
                var signInContext = new BasicSignInContext(Context, Scheme, Options) {
                    Username = username,
                    Password = password,
                };
                await Events.SignIn(signInContext);

                if (signInContext.Result != null) {
                    return signInContext.Result;
                }

                // allow sign in event to modify received credentials
                username = signInContext.Username;
                password = signInContext.Password;

                // verify credentials against options
                BasicCredential credentials = null;
                for (var i = 0; i < Options.Credentials.Length; i++) {
                    var currentCredentials = Options.Credentials[i];

                    if (currentCredentials.Username == username && currentCredentials.Password == password) {
                        credentials = currentCredentials;
                        break;
                    }
                }
                if (credentials == null) {
                    return AuthenticateResult.Fail("Invalid basic authentication credentials.");
                }

                var claims = new Claim[credentials.Claims.Length + 1];
                claims[0] = new Claim(ClaimsIdentity.DefaultNameClaimType, credentials.Username);
                for (var i = 0; i < credentials.Claims.Length; i++) {
                    var currentClaim = credentials.Claims[i];

                    claims[i + 1] = new Claim(currentClaim.Type, currentClaim.Value);
                }

                var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, Scheme.Name));
                var ticket = new AuthenticationTicket(principal, new AuthenticationProperties(), Scheme.Name);

                return AuthenticateResult.Success(ticket);
            } catch (Exception ex) {
                var authenticationFailedContext = new AuthenticationFailedContext(Context, Scheme, Options) {
                    Exception = ex,
                };

                await Events.AuthenticationFailed(authenticationFailedContext);
                if (authenticationFailedContext.Result != null) {
                    return authenticationFailedContext.Result;
                }

                throw;
            }
        }

        protected override Task HandleChallengeAsync(AuthenticationProperties properties) {
            Response.StatusCode = 401;
            Response.Headers.Append(HeaderNames.WWWAuthenticate, $"Basic realm=\"{Options.Realm}\"");

            return Task.CompletedTask;
        }

        private AuthenticationTicket CreateTicket(string username, string password) {
            List<Claim> claims;
            var credentials = Options.Credentials.FirstOrDefault(c => c.Username == username && c.Password == password);
            if (credentials != null) {
                claims = credentials.Claims.Select(c => new Claim(c.Type, c.Value)).ToList();
                if (claims.All(c => c.Type != ClaimTypes.Name)) {
                    claims.Add(new Claim(ClaimTypes.Name, username));
                }
            } else {
                claims = new List<Claim> {
                    new Claim(ClaimTypes.Name, username),
                    new Claim("RUser", "")
                };
            }

            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, Scheme.Name));
            return new AuthenticationTicket(principal, new AuthenticationProperties(), Scheme.Name);
        }
    }
}