// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Languages.Core.Text;
using Microsoft.R.Core.Tokens;
using static System.FormattableString;

namespace Microsoft.R.Editor {
    public static class StringExtensions {
        public static string BacktickName(this string name) {
            if (!string.IsNullOrEmpty(name)) {
                var t = new RTokenizer();
                var tokens = t.Tokenize(name);
                if (tokens.Count > 1) {
                    return Invariant($"`{name}`");
                }
            }
            return name;
        }

        /// <summary>
        /// Locates boundaries of '{ }' block in the text.
        /// Takes into account curly brace nesting.
        /// </summary>
        /// <param name="text">Text to look into</param>
        public static ITextRange GetScopeBlockRange(this string text) {
            var start = text.IndexOf('{');
            if (start < 0) {
                return TextRange.FromBounds(0, 0);
            }

            var bc = new BraceCounter<char>('{', '}');
            var end = start;
            bc.CountBrace(text[end]);
            while (bc.Count > 0 && end < text.Length - 1) {
                end++;
                bc.CountBrace(text[end]);
            }

            return TextRange.FromBounds(start, end + 1);
        }
    }
}
