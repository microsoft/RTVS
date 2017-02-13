// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core.Security;

namespace Microsoft.Common.Core.Test.Stubs.Shell {
    public class SecurityServiceStub : ISecurityService {
        public ConcurrentQueue<Tuple<string, string, CancellationToken>> GetUserCredentialsAsyncCalls { get; } = new ConcurrentQueue<Tuple<string, string, CancellationToken>>();
        public ConcurrentQueue<Tuple<X509Certificate, string>> ValidateX509CertificateCalls { get; } = new ConcurrentQueue<Tuple<X509Certificate, string>>();
        public ConcurrentQueue<string> DeleteUserCredentialsCalls { get; } = new ConcurrentQueue<string>();

        public Func<string, string, CancellationToken, Task<Credentials>> GetUserCredentialsAsyncHandler { get; set; } =
            (authority, workspaceName, cancellationToken) => { throw new NotImplementedException(); };
        public Func<X509Certificate, string, bool> ValidateX509CertificateHandler { get; set; } = (deviceId, ct) => true;
        public Func<string, bool> DeleteUserCredentialsHandler { get; set; } = authority => true;

        public Task<Credentials> GetUserCredentialsAsync(string authority, string workspaceName, CancellationToken cancellationToken = new CancellationToken()) {
            GetUserCredentialsAsyncCalls.Enqueue(new Tuple<string, string, CancellationToken>(authority, workspaceName, cancellationToken));
            var handler = GetUserCredentialsAsyncHandler;
            if (handler != null) {
                return handler(authority, workspaceName, cancellationToken);
            }

            throw new NotImplementedException();
        }

        public bool ValidateX509Certificate(X509Certificate certificate, string message) {
            ValidateX509CertificateCalls.Enqueue(new Tuple<X509Certificate, string>(certificate, message));
            var handler = ValidateX509CertificateHandler;
            if (handler != null) {
                return handler(certificate, message);
            }

            throw new NotImplementedException();
        }

        public bool DeleteUserCredentials(string authority) {
            DeleteUserCredentialsCalls.Enqueue(authority);
            var handler = DeleteUserCredentialsHandler;
            if (handler != null) {
                return handler(authority);
            }

            throw new NotImplementedException();
        }
    }
}