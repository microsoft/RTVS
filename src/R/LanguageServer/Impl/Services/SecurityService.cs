// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Common.Core.Security;

namespace Microsoft.R.LanguageServer.Services {
    internal sealed class SecurityService: ISecurityService {
        public Credentials GetUserCredentials(string authority, string workspaceName) {
            throw new NotImplementedException();
        }

        public bool ValidateX509Certificate(X509Certificate certificate, string message) {
            throw new NotImplementedException();
        }

        public bool DeleteUserCredentials(string authority) {
            throw new NotImplementedException();
        }

        public void DeleteCredentials(string authority) {
            throw new NotImplementedException();
        }

        public string GetUserName(string authority) {
            throw new NotImplementedException();
        }
    }
}
