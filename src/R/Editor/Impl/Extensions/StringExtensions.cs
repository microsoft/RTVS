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

        public static string RemoveBackticks(this string name) {
            if (!string.IsNullOrEmpty(name) && name.Length >= 2 && name[0] == '`' && name[name.Length - 1] == '`') {
                return name.Substring(1, name.Length - 2);
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

        /// <summary>
        /// Retrieves text between given position and the nearest preceding whitespace.
        /// </summary>
        public static string TextBeforePosition(this string s, int position) {
            var i = position - 1;
            for (; i >= 0 && !char.IsWhiteSpace(s[i]); i--) { }
            return s.Substring(i + 1, position - i - 1);
        }
    }
}
