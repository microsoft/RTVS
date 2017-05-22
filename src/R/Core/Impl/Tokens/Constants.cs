// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Microsoft.R.Core.Tokens {
    public static class Constants {
        public static bool IsConstant(string candidate) => Array.BinarySearch(ConstantsList, candidate) >= 0;
        public static string[] ConstantsList { get; } = {
                 // must be sorted
                "Inf",
                "letters",
                "month.abb",
                "month.name",
                "NA",
                "NA_character_",
                "NA_complex_",
                "NA_integer_",
                "NA_real_",
                "NaN",
                "NULL",
                "pi"
            };
    }
}
