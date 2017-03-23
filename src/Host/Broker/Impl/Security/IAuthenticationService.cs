// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Security.Claims;
using System.Threading.Tasks;

namespace Microsoft.R.Host.Broker.Security {
    public interface IAuthenticationService {
        Task<ClaimsPrincipal> SignIn(string username, string password, string authenticationScheme);
    }
}