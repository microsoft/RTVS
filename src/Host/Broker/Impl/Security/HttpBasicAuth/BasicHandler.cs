// From https://github.com/Kukkimonsuta/Odachi/tree/master/src/Odachi.AspNetCore.Authentication.Basic

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Http.Features.Authentication;
using Microsoft.AspNetCore.Http.Features;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Security.Claims;
using Microsoft.R.Host.Broker.Security;

namespace Odachi.AspNetCore.Authentication.Basic {
    internal class BasicHandler : AuthenticationHandler<BasicOptions> {
        public const string RequestHeader = "Authorization";
        public const string ChallengeHeader = "WWW-Authenticate";
        public const string RequestHeaderPrefix = "Basic ";

        public BasicHandler() {
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync() {
            try {
                var feat = Context.Features.OfType<HttpRequestFeature>();

                if (Uri.TryCreate(CurrentUri, UriKind.Absolute, out var uri)) {
                    if (uri.IsLoopback && !Request.IsHttps) {
                        var t = CreateTicket(Claims.RUser, string.Empty);
                        return AuthenticateResult.Success(t);
                    }
                }

                var headers = Request.Headers[RequestHeader];
                if (headers.Count <= 0) {
                    return AuthenticateResult.Fail("No authorization header.");
                }

                var header = headers.FirstOrDefault(h => h.StartsWith(RequestHeaderPrefix, StringComparison.OrdinalIgnoreCase));
                if (header == null) {
                    return AuthenticateResult.Fail("Not basic authentication header.");
                }

                var encoded = header.Substring(RequestHeaderPrefix.Length);
                string decoded;
                try {
                    decoded = Encoding.UTF8.GetString(Convert.FromBase64String(encoded));
                } catch (Exception) {
                    return AuthenticateResult.Fail("Invalid basic authentication header encoding.");
                }

                var index = decoded.IndexOf(':');
                if (index == -1) {
                    return AuthenticateResult.Fail("Invalid basic authentication header format.");
                }

                var username = decoded.Substring(0, index);
                var password = decoded.Substring(index + 1);

                var signInContext = new BasicSignInContext(Context, Options, username, password);
                await Options.Events.SignIn(signInContext);

                if (signInContext.HandledResponse) {
                    return signInContext.Ticket != null
                        ? AuthenticateResult.Success(signInContext.Ticket)
                        : AuthenticateResult.Fail("Invalid basic authentication credentials.");
                }

                if (signInContext.Skipped) {
                    return AuthenticateResult.Success(null);
                }

                var ticket = CreateTicket(username, password);
                if (ticket == null) {
                    return AuthenticateResult.Fail("Invalid basic authentication credentials.");
                }

                return AuthenticateResult.Success(ticket);
            } catch (Exception ex) {
                var exceptionContext = new BasicExceptionContext(Context, Options, ex);

                await Options.Events.Exception(exceptionContext);

                if (exceptionContext.HandledResponse) {
                    return AuthenticateResult.Success(exceptionContext.Ticket);
                }

                if (exceptionContext.Skipped) {
                    return AuthenticateResult.Success(null);
                }

                throw;
            }
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
                    new Claim(Claims.RUser, "")
                };
            }

            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, Options.AuthenticationScheme));
            return new AuthenticationTicket(principal, new AuthenticationProperties(), Options.AuthenticationScheme);
        }

        protected override Task<bool> HandleUnauthorizedAsync(ChallengeContext context) {
            Response.StatusCode = 401;
            Response.Headers[ChallengeHeader] = "Basic realm=\"" + Options.Realm + "\"";
            return Task.FromResult(false);
        }

        protected override Task HandleSignOutAsync(SignOutContext context) {
            throw new NotSupportedException();
        }

        protected override Task HandleSignInAsync(SignInContext context) {
            throw new NotSupportedException();
        }
    }
}