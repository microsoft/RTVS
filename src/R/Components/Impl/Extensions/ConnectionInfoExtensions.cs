// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.R.Components.ConnectionManager;

namespace Microsoft.R.Components {
    public static class ConnectionInfoExtensions {
        public static string ToCredentialAuthority(this IConnectionInfo connectionInfo) {
            return $"RTVS:{connectionInfo.Name}";
        }
    }
}
