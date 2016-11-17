// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Microsoft.Common.Core {
    public static class UriExtensions {
        public static string ToCredentialAuthority(this Uri uri) {
            return new UriBuilder { Scheme = uri.Scheme, Host = uri.Host, Port = uri.Port }.ToString();
        }
    }
}
