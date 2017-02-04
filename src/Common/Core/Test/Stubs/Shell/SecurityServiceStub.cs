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
        public ConcurrentQueue<string> DeleteCredentialsCalls { get; } = new ConcurrentQueue<string>();
        public ConcurrentQueue<string> ReadSavedCredentialsCalls { get; } = new ConcurrentQueue<string>();
        public ConcurrentQueue<Tuple<Credentials, string>> SaveCredentialsCalls { get; } = new ConcurrentQueue<Tuple<Credentials, string>>();

        public Func<string, string, CancellationToken, Credentials> GetUserCredentialsHandler { get; set; } =
            (authority, workspaceName, cancellationToken) => { throw new NotImplementedException(); };
        public Func<string, Credentials> ReadSavedCredentialsHandler { get; set; } = authority => { throw new NotImplementedException(); };
        public Action<Credentials, string> SaveCredentialsHandler { get; set; } = (credentials, authority) => { throw new NotImplementedException(); };
        public Func<X509Certificate, string, bool> ValidateX509CertificateHandler { get; set; } = (deviceId, ct) => true;
        public Func<string, bool> DeleteUserCredentialsHandler { get; set; } = authority => true;
        public Action<string> DeleteCredentialsHandler { get; set; } = authority => {};

        public Credentials GetUserCredentials(string authority, string workspaceName, CancellationToken cancellationToken = new CancellationToken()) {
            GetUserCredentialsAsyncCalls.Enqueue(new Tuple<string, string, CancellationToken>(authority, workspaceName, cancellationToken));
            var handler = GetUserCredentialsHandler;
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
        
        public Credentials ReadSavedCredentials(string authority) {
            DeleteUserCredentialsCalls.Enqueue(authority);
            var handler = ReadSavedCredentialsHandler;
            if (handler != null) {
                return handler(authority);
            }

            throw new NotImplementedException();
        }

        public void Save(Credentials credentials, string authority) {
            SaveCredentialsCalls.Enqueue(new Tuple<Credentials, string>(credentials, authority));
            var handler = SaveCredentialsHandler;
            if (handler != null) {
                handler(credentials, authority);
            } else {
                throw new NotImplementedException();
            }
        }

        public void DeleteCredentials(string authority) {
            DeleteCredentialsCalls.Enqueue(authority);
            var handler = DeleteCredentialsHandler;
            if (handler != null) {
                handler(authority);
            } else {
                throw new NotImplementedException();
            }
        }
    }
}