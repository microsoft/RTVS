// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Disposables;
using Microsoft.Common.Core.Security;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Threading;
using Microsoft.R.Host.Client.BrokerServices;

namespace Microsoft.R.Host.Client.Host {
    internal class RemoteCredentialsDecorator : ICredentialsDecorator {
        private readonly IServiceContainer _services;
        private volatile Credentials _credentials;
        private readonly AsyncReaderWriterLock _lock;
        private readonly string _authority;
        private readonly string _workspaceName;

        public RemoteCredentialsDecorator(string credentialAuthority, string workspaceName, IServiceContainer services) {
            _services = services;
            _authority = credentialAuthority;
            _lock = new AsyncReaderWriterLock();
            _workspaceName = workspaceName;
        }

        public NetworkCredential GetCredential(Uri uri, string authType) {
            var credentials = _credentials;
            return credentials != null ? new NetworkCredential(credentials.UserName, credentials.Password.ToUnsecureString()) : new NetworkCredential();
        }

        public async Task<IDisposable> LockCredentialsAsync(CancellationToken cancellationToken = default(CancellationToken)) {
            // If there is already a LockCredentialsAsync request for which there hasn't been a validation yet, wait until it completes.
            // This can happen when two sessions are being created concurrently, and we don't want to pop the credential prompt twice -
            // the first prompt should be validated and saved, and then the same credentials will be reused for the second session.
            var token = await _lock.WriterLockAsync(cancellationToken);

            try {
                var credentials = _credentials ?? await _services.Security().GetUserCredentialsAsync(_authority, _workspaceName, cancellationToken);
                _credentials = credentials;
            } catch (Exception ex) when (!ex.IsCriticalException() && !(ex is OperationCanceledException)) {
                // TODO: provide better error message
                //_services.GetService<IConsole>().WriteErrorLine(Invariant($"{Microsoft.Common.Core.Resources.Error_CredReadFailed} {ex.Message}"));
                token.Dispose();
                return Disposable.Empty;
            }

            return Disposable.Create(() => {
                token.Dispose();
            });
        }

        public void InvalidateCredentials() {
            _credentials = null;
            try {
                _services.Security().DeleteCredentials(_authority);
            } catch(Exception ex) when (!ex.IsCriticalException()) {
                // TODO: provide better error message
                //_console.WriteErrorLine(Invariant($"{Common.Core.Resources.Error_CredWriteFailed} {ex.Message}"));
            }
        }
    }
}