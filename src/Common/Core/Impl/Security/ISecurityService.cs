// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Common.Core.Security {
    public interface ISecurityService {
        Task<Credentials> GetUserCredentialsAsync(string authority, string workspaceName, CancellationToken cancellationToken);
        (string username, SecureString password) ReadUserCredentials(string authority);
        bool ValidateX509Certificate(X509Certificate certificate, string message);
        void SaveUserCredentials(string authority, string userName, SecureString password, bool save);
        bool DeleteUserCredentials(string authority);
        void DeleteCredentials(string authority);
    }
}
