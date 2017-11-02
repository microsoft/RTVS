// From https://github.com/Kukkimonsuta/Odachi/tree/master/src/Odachi.AspNetCore.Authentication.Basic

using System;
using System.Linq;
using System.Net;
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
                // .NET Core as of 2.0 does not support HTTP auth on sockets
                // This is alternative solution to anonymous access in SessionController.GetPipe()
                // but it only works for local connections (which may be OK for VSC but it won't work
                // in VSC with remote or Docker containers.

                //if (Uri.TryCreate(CurrentUri, UriKind.Absolute, out var uri)) {
                //    if (uri.IsLoopback && !Request.IsHttps) {
                //        var claims = new[] {
                //            new Claim(ClaimTypes.Name, "RTVS"),
                //            new Claim(Claims.RUser, string.Empty),
                //        };
                //        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, BasicDefaults.AuthenticationScheme));
                //        var t = new AuthenticationTicket(principal, new AuthenticationProperties(), Scheme.Name);
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
    }
}