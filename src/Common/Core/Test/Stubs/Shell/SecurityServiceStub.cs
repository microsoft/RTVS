// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core.Security;

namespace Microsoft.Common.Core.Test.Stubs.Shell {
    public class SecurityServiceStub : ISecurityService {
        public ConcurrentQueue<(string, string)> GetUserCredentialsCalls { get; } = new ConcurrentQueue<(string, string)>();
        public ConcurrentQueue<(X509Certificate, string)> ValidateX509CertificateCalls { get; } = new ConcurrentQueue<(X509Certificate, string)>();
        public ConcurrentQueue<(string, string, SecureString, bool)> SaveUserCredentialsCalls { get; } = new ConcurrentQueue<(string, string, SecureString, bool)>();
        public ConcurrentQueue<string> DeleteUserCredentialsCalls { get; } = new ConcurrentQueue<string>();
        public ConcurrentQueue<string> DeleteCredentialsCalls { get; } = new ConcurrentQueue<string>();
        public ConcurrentQueue<string> GetUserNameCalls { get; } = new ConcurrentQueue<string>();

        public Func<string, string, Credentials> GetUserCredentialsHandler { get; set; } = (authority, workspaceName) => throw new NotImplementedException();
        public Func<string, (string, SecureString)> ReadUserCredentialsHandler { get; set; } = authority => (authority, authority.ToSecureString());
        public Func<X509Certificate, string, bool> ValidateX509CertificateHandler { get; set; } = (deviceId, ct) => true;
        public Action<string, string, SecureString, bool> SaveUserCredentialsHandler { get; set; } = (authority, userName, password, save) => {};
        public Func<string, bool> DeleteUserCredentialsHandler { get; set; } = authority => true;
        public Action<string> DeleteCredentialsHandler { get; set; } = authority => {};

        public Task<Credentials> GetUserCredentialsAsync(string authority, string workspaceName, CancellationToken cancellationToken) {
            GetUserCredentialsCalls.Enqueue((authority, workspaceName));
            var handler = GetUserCredentialsHandler;
            if (handler != null) {
                return Task.FromResult(handler(authority, workspaceName));
            }

            throw new NotImplementedException();
        }

        public bool ValidateX509Certificate(X509Certificate certificate, string message) {
            ValidateX509CertificateCalls.Enqueue((certificate, message));
            var handler = ValidateX509CertificateHandler;
            if (handler != null) {
                return handler(certificate, message);
            }

            throw new NotImplementedException();
        }

        public void SaveUserCredentials(string authority, string userName, SecureString password, bool save) {
            SaveUserCredentialsCalls.Enqueue((authority, userName, password, save));
            var handler = SaveUserCredentialsHandler;
            if (handler != null) {
                handler(authority, userName, password, save);
            } else {
                throw new NotImplementedException();
            }
        }

        public bool DeleteUserCredentials(string authority) {
            DeleteUserCredentialsCalls.Enqueue(authority);
            var handler = DeleteUserCredentialsHandler;
            if (handler != null) {
                return handler(authority);
            }

            throw new NotImplementedException();
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

        public (string username, SecureString password) ReadUserCredentials(string authority) {
            GetUserNameCalls.Enqueue(authority);
            var handler = ReadUserCredentialsHandler;
            if (handler != null) {
                return handler(authority);
            }

            throw new NotImplementedException();
        }
    }
}