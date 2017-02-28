// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Security.Cryptography.X509Certificates;

namespace Microsoft.Common.Core.Security {
    public interface ISecurityService {
        Credentials GetUserCredentials(string authority, string workspaceName);
        bool ValidateX509Certificate(X509Certificate certificate, string message);
        bool DeleteUserCredentials(string authority);
        void DeleteCredentials(string authority);
        string GetUserName(string authority);
    }
}
