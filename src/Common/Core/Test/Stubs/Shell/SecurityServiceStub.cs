// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Common.Core.Security;

namespace Microsoft.Common.Core.Test.Stubs.Shell {
    public class SecurityServiceStub : ISecurityService {
        public ConcurrentQueue<Tuple<string, string>> GetUserCredentialsCalls { get; } = new ConcurrentQueue<Tuple<string, string>>();
        public ConcurrentQueue<Tuple<X509Certificate, string>> ValidateX509CertificateCalls { get; } = new ConcurrentQueue<Tuple<X509Certificate, string>>();
        public ConcurrentQueue<string> DeleteUserCredentialsCalls { get; } = new ConcurrentQueue<string>();
        public ConcurrentQueue<string> DeleteCredentialsCalls { get; } = new ConcurrentQueue<string>();
        public ConcurrentQueue<string> GetUserNameCalls { get; } = new ConcurrentQueue<string>();

        public Func<string, string, Credentials> GetUserCredentialsHandler { get; set; } = (authority, workspaceName) => { throw new NotImplementedException(); };
        public Func<X509Certificate, string, bool> ValidateX509CertificateHandler { get; set; } = (deviceId, ct) => true;
        public Func<string, bool> DeleteUserCredentialsHandler { get; set; } = authority => true;
        public Action<string> DeleteCredentialsHandler { get; set; } = authority => {};
        public Func<string, string> GetUserNameHandler { get; set; } = authority => authority;

        public Credentials GetUserCredentials(string authority, string workspaceName) {
            GetUserCredentialsCalls.Enqueue(new Tuple<string, string>(authority, workspaceName));
            var handler = GetUserCredentialsHandler;
            if (handler != null) {
                return handler(authority, workspaceName);
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
        
        public void DeleteCredentials(string authority) {
            DeleteCredentialsCalls.Enqueue(authority);
            var handler = DeleteCredentialsHandler;
            if (handler != null) {
                handler(authority);
            } else {
                throw new NotImplementedException();
            }
        }

        public string GetUserName(string authority) {
            GetUserNameCalls.Enqueue(authority);
            var handler = GetUserNameHandler;
            if (handler != null) {
                return handler(authority);
            }

            throw new NotImplementedException();
        }
    }
}