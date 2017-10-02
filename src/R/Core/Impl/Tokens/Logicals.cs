// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Microsoft.R.Core.Tokens {
    public static class Logicals {
        public static bool IsLogical(string candidate)
            => Array.BinarySearch(LogicalsList, candidate) >= 0; // R is case sensitive language

        // must be sorted
        public static string[] LogicalsList { get; } = new [] {
            "F",
            "FALSE",
            "T",
            "TRUE",
        };
    }
}
