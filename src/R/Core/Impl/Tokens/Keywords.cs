// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Microsoft.R.Core.Tokens {
    public static class Keywords {
        public static bool IsKeyword(string candidate) 
            => Array.BinarySearch(KeywordList, candidate) >= 0;

        public static string[] KeywordList { get; } = {
            "break",
            "else",
            "for",
            "function",
            "if",
            "in",
            "next",
            "repeat",
            "while",
        };
    }
}
