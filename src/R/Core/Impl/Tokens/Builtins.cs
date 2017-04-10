// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Microsoft.R.Core.Tokens {
    public static class Builtins {
        public static bool IsBuiltin(string candidate) {
            // R is case sensitive language
            return Array.BinarySearch<string>(_builtins, candidate) >= 0;
        }

        public static string[] BuiltinList {
            get { return _builtins; }
        }

        internal static string[] _builtins = {
            "library",
            "require",
            "return",
            "switch",
            "typeof",
        };
    }
}
