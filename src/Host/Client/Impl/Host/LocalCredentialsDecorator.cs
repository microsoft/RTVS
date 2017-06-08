// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core.Disposables;
using Microsoft.R.Host.Client.BrokerServices;

namespace Microsoft.R.Host.Client.Host {
    internal class LocalCredentialsDecorator : ICredentialsDecorator {
        private static readonly NetworkCredential LocalCredentials = new NetworkCredential("RTVS", Guid.NewGuid().ToString());

        public string Password => LocalCredentials.Password;

        public Task<IDisposable> LockCredentialsAsync(CancellationToken cancellationToken = default(CancellationToken)) => Disposable.EmptyTask;

        public void InvalidateCredentials() {
            // Local broker authentication should never fail - if it does, it's a bug, and we want to surface it right away.
            const string message = "Authentication failed for local broker";
            Trace.Fail(message);
            throw new RHostDisconnectedException(message);
        }

        public NetworkCredential GetCredential(Uri uri, string authType) => LocalCredentials;
    }
}