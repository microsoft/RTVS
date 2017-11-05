// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.R.Host.Broker.Services;
using Odachi.AspNetCore.Authentication.Basic;

namespace Microsoft.R.Host.Broker.Security {
    public class SecurityManager {
        private readonly SecurityOptions _options;
        private readonly IPlatformAuthenticationService _authenticationService;

        public SecurityManager(IPlatformAuthenticationService authenticationService, IOptions<SecurityOptions> options) {
            _authenticationService = authenticationService;
            _options = options.Value;
        }

        public async Task SignInAsync(BasicSignInContext context) {
            if (context.IsSignInRequired()) {
                context.Principal = _options.Secret != null 
                    ? SignInUsingSecret(context) 
                    : await _authenticationService.SignInAsync(context.Username, context.Password, context.Scheme.Name);
            } else {
                var claims = new[] { new Claim(ClaimTypes.Anonymous, "") };
                var claimsIdentity = new ClaimsIdentity(claims, context.Scheme.Name);
                context.Principal = new ClaimsPrincipal(claimsIdentity);
            }
        }

        private ClaimsPrincipal SignInUsingSecret(BasicSignInContext context) {
            if (_options.Secret != context.Password) {
                return null;
            }

            var claims = new[] {
                new Claim(ClaimTypes.Name, context.Username),
                new Claim(Claims.RUser, string.Empty)
            };

            var identity = new ClaimsIdentity(claims, context.Scheme.Name);
            return new ClaimsPrincipal(identity);
        }
    }
}
