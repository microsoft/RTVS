// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Microsoft.R.Core.Tokens {
    public static class Logicals {
        public static string[] LogicalsList {
            get { return _logicals; }
        }

        public static bool IsLogical(string candidate) {
            // R is case sensitive language
            return Array.BinarySearch<string>(_logicals, candidate) >= 0;
        }

        // must be sorted
        internal static string[] _logicals = new string[]
        {
            "F",
            "FALSE",
            "T",
            "TRUE",
        };
    }
}
