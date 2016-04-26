// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.R.Host.Client;

namespace Microsoft.R.Debugger {
    /// <summary>
    /// A service that manages debug session instances for R sessions.
    /// </summary>
    public interface IDebugSessionProvider {
        /// <summary>
        /// Returns the debug session associated with the given R session. Creates the session if it doesn't exist.
        /// </summary>
        Task<DebugSession> GetDebugSessionAsync(IRSession session, CancellationToken cancellationToken = default(CancellationToken));
    }
}
