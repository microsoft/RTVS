// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Microsoft.Common.Core.Net {
    public static class NetworkExtensions {
        public static bool IsHttps(this Uri url) => url.Scheme.EqualsIgnoreCase("https");
    }
}
