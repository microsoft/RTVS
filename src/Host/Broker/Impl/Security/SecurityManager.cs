// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Odachi.AspNetCore.Authentication.Basic;

namespace Microsoft.R.Host.Broker.Security {
    public class SecurityManager {
        private readonly SecurityOptions _options;
        private readonly ILogger _logger;
        private readonly IAuthenticationService _authenticationService;

        public SecurityManager(IAuthenticationService authenticationService, IOptions<SecurityOptions> options, ILogger<SecurityManager> logger) {
            _authenticationService = authenticationService;
            _options = options.Value;
            _logger = logger;
        }

        public async Task SignInAsync(BasicSignInContext context) {
            ClaimsPrincipal principal;
            if (context.IsSignInRequired()) {
                principal = _options.Secret != null ? SignInUsingSecret(context) : await _authenticationService.SignIn(context.Username, context.Password, context.Options.AuthenticationScheme);
            } else {
                var claims = new[] { new Claim(ClaimTypes.Anonymous, "") };
                var claimsIdentity = new ClaimsIdentity(claims, context.Options.AuthenticationScheme);
                principal = new ClaimsPrincipal(claimsIdentity);
            }

            if (principal != null) {
                context.Ticket = new AuthenticationTicket(principal, new AuthenticationProperties(), context.Options.AuthenticationScheme);
            }

            context.HandleResponse();
        }

        private ClaimsPrincipal SignInUsingSecret(BasicSignInContext context) {
            if (_options.Secret != context.Password) {
                return null;
            }

            var claims = new[] {
                new Claim(ClaimTypes.Name, context.Username),
                new Claim(Claims.RUser, "")
            };

            var identity = new ClaimsIdentity(claims, context.Options.AuthenticationScheme);
            return new ClaimsPrincipal(identity);
        }
    }
}
