// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Odachi.AspNetCore.Authentication.Basic;

namespace Microsoft.R.Host.Broker {
    public static class BasicSignInContextExtensions {
        public static bool IsSignInRequired(this BasicSignInContext context) {
            string path = context.HttpContext.Request.Path.ToString();
            if (path.Equals("/info/load") || path.Equals("/info/about")) {
                return false;
            }
            return true;
        }
    }
}
