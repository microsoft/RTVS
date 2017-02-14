// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.R.Host.Client.BrokerServices {
    public interface ICredentialsDecorator : ICredentials {
        /// <summary>
        /// Called before issuing an authenticated HTTP request. Implementation can refresh <see cref="HttpClientHandler.Credentials"/> if necessary.
        /// </summary>
        /// <exception cref="OperationCanceledException">
        /// Retrieval of credentials was canceled by the user (for example, by clicking the "Cancel" button in the dialog).
        /// Usually, this indicates that the operation that asked for credentials should be canceled as well.
        /// </exception>
        Task<IDisposable> LockCredentialsAsync(CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Called after the request that used credentials updated by an earlier call to <see cref="LockCredentialsAsync"/> completes.
        /// </summary>
        void InvalidateCredentials();
    }
}
