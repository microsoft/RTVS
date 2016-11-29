// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using Odachi.AspNetCore.Authentication.Basic;

namespace Microsoft.R.Host.Broker {
    public static class BasicSignInContextExtensions {

        static readonly HashSet<string> _skipSignInPaths = new HashSet<string>() { "/info/load", "/info/about" };
        public static bool IsSignInRequired(this BasicSignInContext context) {
            string path = context.HttpContext.Request.Path.ToString();
            if (_skipSignInPaths.Contains(path)) {
                return false;
            }
            return true;
        }
    }
}
