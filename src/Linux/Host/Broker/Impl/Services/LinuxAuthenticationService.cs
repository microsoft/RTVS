// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.R.Host.Broker.Services {
    class LinuxAuthenticationService : IAuthenticationService {
        public Task<ClaimsPrincipal> SignInAsync(string username, string password, string authenticationScheme) {
            throw new NotImplementedException();
        }
    }
}
