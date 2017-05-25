// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Microsoft.R.Editor.RData.Tokens {
    public enum BlockContentType {
        Latex,
        R,
        Verbatim
    };

    public static class RdBlockContentType {
        public static string[] Keywords = {
            @"\code",
            @"\dontshow",
            @"\donttest",
            @"\examples",
            @"\Sexpr",
            @"\testonly",
            @"\usage",
        };

        public static string[] VerbatimKeywords = {
            @"\alias",
            @"\deqn",
            @"\dontrun",
            @"\env",
            @"\eqn",
            @"\href",
            @"\kbd",
            @"\newcommand",
            @"\option",
            @"\out",
            @"\preformatted",
            @"\RdOpts",
            @"\Rdversion",
            @"\renewcommand",
            @"\samp",
            @"\special",
            @"\synopsis",
            @"\testonly",
            @"\url",
            @"\verb",
         };

        public static BlockContentType GetBlockContentType(string keyword) {
            int index = Array.BinarySearch(Keywords, keyword);
            if (index >= 0) {
                return BlockContentType.R;
            }

            index = Array.BinarySearch(VerbatimKeywords, keyword);
            if (index >= 0) {
                return BlockContentType.Verbatim;
            }

            return BlockContentType.Latex;
        }
    }
}
