// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading;
using Microsoft.R.Host.Broker.Startup;

namespace Microsoft.R.Host.Broker.Services {
    /// <summary>
    ///  Wrapper around CommonStartup.Exit and CommonStartup.CancellationToken
    /// </summary>
    public class ExitService : IExitService {
        public void Exit() => CommonStartup.Exit();
        public CancellationToken CancellationToken => CommonStartup.CancellationToken;
    }
}