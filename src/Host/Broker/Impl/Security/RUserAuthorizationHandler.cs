// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace Microsoft.R.Host.Broker.Security {
    public class RUserAuthorizationHandler : AuthorizationHandler<RUserAuthorizationRequirement> {
        private readonly IOptions<SecurityOptions> _securityOptions;

        public RUserAuthorizationHandler(IOptions<SecurityOptions> securityOptions) {
            _securityOptions = securityOptions;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, RUserAuthorizationRequirement requirement) {
            if (!context.User.IsInRole(_securityOptions.Value.AllowedGroup)) {
                return Task.CompletedTask;
            }

            context.Succeed(requirement);
            return Task.CompletedTask;
        }
    }
}
