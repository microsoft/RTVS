// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Common.Core.OS;
using System.IO;
using Microsoft.R.Host.Broker.Startup;
using System.Reflection;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using System.Security.Principal;
using Microsoft.R.Host.Broker.Security;

namespace Microsoft.R.Host.Broker.Services {
    public class LinuxAuthenticationService : IAuthenticationService {
        private readonly IProcessServices _ps;
        private readonly ILogger<IAuthenticationService> _logger;

        public LinuxAuthenticationService(ILogger<IAuthenticationService> logger, IProcessServices ps) {
            _ps = ps;
            _logger = logger;
        }

        public Task<ClaimsPrincipal> SignInAsync(string username, string password, string authenticationScheme) {
            string profileDir;
            if(Utility.AuthenticateUser(_logger, _ps, username, password, out profileDir)) {
                var identity = new GenericIdentity(username, "login");
                var principal = new ClaimsPrincipal(identity);
                var claims = new[] {
                    new Claim(Claims.RUser, ""),
                    new Claim(UnixClaims.RUsername, username),
                    new Claim(UnixClaims.RPassword, password),
                    new Claim(Claims.RUserProfileDir, profileDir)
                };

                var claimsIdentity = new ClaimsIdentity(claims, authenticationScheme);
                principal.AddIdentities(new[] { claimsIdentity });
                return Task.FromResult(principal);
            }
            return Task.FromResult<ClaimsPrincipal>(null);
        }



    }
}
