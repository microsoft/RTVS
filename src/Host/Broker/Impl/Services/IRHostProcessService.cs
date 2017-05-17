// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Security.Claims;
using Microsoft.Common.Core.OS;
using Microsoft.R.Host.Broker.Interpreters;

namespace Microsoft.R.Host.Broker.Services {
    public interface IRHostProcessService {
        IProcess StartHost(Interpreter interpreter, string profilePath, string userName, ClaimsPrincipal principal, string commandLine);
    }
}