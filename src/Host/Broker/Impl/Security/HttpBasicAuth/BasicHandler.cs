// From https://github.com/Kukkimonsuta/Odachi/tree/master/src/Odachi.AspNetCore.Authentication.Basic

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Microsoft.R.Host.Broker.Security;
using Microsoft.R.Host.Broker.Start;

namespace Odachi.AspNetCore.Authentication.Basic {
    internal class BasicHandler : AuthenticationHandler<BasicOptions> {
        public const string RequestHeaderPrefix = "Basic ";

        public BasicHandler(IOptionsMonitor<BasicOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
            : base(options, logger, encoder, clock) {
        }

        protected override Task<object> CreateEventsAsync()
            => Task.FromResult<object>(new BasicEvents { OnSignIn = ProgramBase.WebHost.Services.GetService<SecurityManager>().SignInAsync });

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync() {
            var events = Events as BasicEvents;

            try {
                // .NET Core does not support HTTP auth on sockets
                //if (Uri.TryCreate(CurrentUri, UriKind.Absolute, out var uri)) {
                //    if (uri.IsLoopback && !Request.IsHttps) {
                //        var t = CreateTicket("RUser", string.Empty);
                //        return AuthenticateResult.Success(t);
                //    }
                //}

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
                var signInContext = new BasicSignInContext(Context, Scheme, Options) {
                    Username = username,
                    Password = password,
                };

                await events.SignIn(signInContext);
                if (signInContext.Principal == null) {
                    return AuthenticateResult.Fail("Invalid basic authentication credentials.");
                }

                var ticket = new AuthenticationTicket(signInContext.Principal, new AuthenticationProperties(), Scheme.Name);

                return AuthenticateResult.Success(ticket);
            } catch (Exception ex) {
                var authenticationFailedContext = new AuthenticationFailedContext(Context, Scheme, Options) {
                    Exception = ex,
                };

                await events.AuthenticationFailed(authenticationFailedContext);
                if (authenticationFailedContext.Result != null) {
                    return authenticationFailedContext.Result;
                }

                throw;
            }
        }

        protected override Task HandleChallengeAsync(AuthenticationProperties properties) {
            Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            Response.Headers.Append(HeaderNames.WWWAuthenticate, $"Basic realm=\"{Options.Realm}\"");
            return Task.CompletedTask;
        }

        //private AuthenticationTicket CreateTicket(string username, string password) {
        //    List<Claim> claims;
        //    var credentials = Options.Credentials.FirstOrDefault(c => c.Username == username && c.Password == password);
        //    if (credentials != null) {
        //        claims = credentials.Claims.Select(c => new Claim(c.Type, c.Value)).ToList();
        //        if (claims.All(c => c.Type != ClaimTypes.Name)) {
        //            claims.Add(new Claim(ClaimTypes.Name, username));
        //        }
        //    } else {
        //        claims = new List<Claim> {
        //            new Claim(ClaimTypes.Name, username),
        //            new Claim("RUser", "")
        //        };
        //    }

        //    var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, Scheme.Name));
        //    return new AuthenticationTicket(principal, new AuthenticationProperties(), Scheme.Name);
        //}
    }
}