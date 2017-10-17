// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Security.Claims;
using System.Threading.Tasks;

namespace Microsoft.R.Host.Broker.Services {
    public interface IPlatformAuthenticationService {
        Task<ClaimsPrincipal> SignInAsync(string username, string password, string authenticationScheme);
    }
}