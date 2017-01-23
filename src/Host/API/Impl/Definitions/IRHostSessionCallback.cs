// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core.Shell;

namespace Microsoft.R.Host.Client {
    /// <summary>
    /// Implemented by the application that uses Microsoft.R.Host.Client.API
    /// Provides facilities to respond to the R engine requests, if any.
    /// Stub class <see cref="RHostSessionCallback"/> provides basic implementation.
    /// </summary>
    public interface IRHostSessionCallback {
        /// <summary>
        /// Displays error message in the application-specific UI
        /// </summary>
        Task ShowErrorMessageAsync(string message, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Displays user prompt with specified buttons in the application-specific UI
        /// </summary>
        Task<MessageButtons> ShowMessageAsync(string message, MessageButtons buttons, CancellationToken cancellationToken = default(CancellationToken));
    }
}
