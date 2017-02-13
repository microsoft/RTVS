// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core.Security;
using Microsoft.Common.Core.Threading;
using Microsoft.R.Host.Client.BrokerServices;

namespace Microsoft.R.Host.Client.Host {
    internal class RemoteCredentialsDecorator : ICredentialsDecorator {
        private readonly ISecurityService _securityService;
        private readonly IMainThread _mainThread;
        private Credentials _credentials;
        private object _credentialsLock;
        private readonly string _authority;
        private readonly string _workspaceName;

        public RemoteCredentialsDecorator(string credentialAuthority, string workspaceName, ISecurityService securityService, IMainThread mainThread) {
            _securityService = securityService;
            _mainThread = mainThread;
            _authority = credentialAuthority;
            _credentialsLock = new object();
            _workspaceName = workspaceName;
        }

        public NetworkCredential GetCredential(Uri uri, string authType) {
            lock (_credentialsLock) {
                return new NetworkCredential(_credentials?.UserName, _credentials?.Password);
            }
        }

        public async Task LockCredentialsAsync(CancellationToken cancellationToken = default(CancellationToken)) {
            // If there is already a LockCredentialsAsync request for which there hasn't been a validation yet, wait until it completes.
            // This can happen when two sessions are being created concurrently, and we don't want to pop the credential prompt twice -
            // the first prompt should be validated and saved, and then the same credentials will be reused for the second session.

            await _mainThread.SwitchToAsync(cancellationToken);

            lock (_credentialsLock) {
                _credentials = _credentials ?? _securityService.GetUserCredentials(_authority, _workspaceName, cancellationToken);
                _credentials.Save(_authority);
            }
        }

        public void InvalidateCredentials() {
            lock (_credentialsLock) {
                _credentials = null;
                SecurityUtilities.DeleteCredentials(_authority);
            }
        }
    }
}