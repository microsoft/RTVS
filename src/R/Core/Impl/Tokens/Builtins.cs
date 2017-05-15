// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Microsoft.R.Core.Tokens {
    public static class Builtins {
        public static bool IsBuiltin(string candidate) =>
            // R is case sensitive language
            Array.BinarySearch(BuiltinList, candidate) >= 0;

        public static string[] BuiltinList { get; } = new[] {
            "library",
            "require",
            "return",
            "switch",
            "typeof",
        };
    }
}
