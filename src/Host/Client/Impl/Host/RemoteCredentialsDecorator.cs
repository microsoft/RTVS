// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core.Disposables;
using Microsoft.Common.Core.Security;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.Threading;
using Microsoft.R.Host.Client.BrokerServices;

namespace Microsoft.R.Host.Client.Host {
    internal class RemoteCredentialsDecorator : ICredentialsDecorator {
        private readonly ICoreShell _coreShell;
        private volatile Credentials _credentials;
        private readonly AsyncReaderWriterLock _lock;
        private readonly string _authority;
        private readonly string _workspaceName;

        public RemoteCredentialsDecorator(string credentialAuthority, string workspaceName, ICoreShell coreShell) {
            _coreShell = coreShell;
            _authority = credentialAuthority;
            _lock = new AsyncReaderWriterLock();
            _workspaceName = workspaceName;
        }

        public NetworkCredential GetCredential(Uri uri, string authType) {
            var credentials = _credentials;
            return credentials != null ? new NetworkCredential(credentials.UserName, credentials.Password) : new NetworkCredential();
        }

        public async Task<IDisposable> LockCredentialsAsync(CancellationToken cancellationToken = default(CancellationToken)) {
            // If there is already a LockCredentialsAsync request for which there hasn't been a validation yet, wait until it completes.
            // This can happen when two sessions are being created concurrently, and we don't want to pop the credential prompt twice -
            // the first prompt should be validated and saved, and then the same credentials will be reused for the second session.
            var token = await _lock.WriterLockAsync(cancellationToken);

            await _coreShell.SwitchToAsync(cancellationToken);

            try {
                var credentials = _credentials ?? _coreShell.Security().GetUserCredentials(_authority, _workspaceName);
                _credentials = credentials;
            } catch (Exception) {
                token.Dispose();
                throw;
            }

            return Disposable.Create(() => {
                token.Dispose();
            });
        }

        public void InvalidateCredentials() {
            _credentials = null;
            _coreShell.Security().DeleteCredentials(_authority);
        }
    }
}