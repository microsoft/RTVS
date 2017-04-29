// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics;
using System.Text;
using Microsoft.Languages.Core.Text;

namespace Microsoft.R.Editor.RData.Parser {
    internal static class RdText {
        public static string GetText(RdParseContext context) {
            string text = string.Empty;

            int startTokenIndex, endTokenIndex;
            if (RdParseUtility.GetKeywordArgumentBounds(context.Tokens, out startTokenIndex, out endTokenIndex)) {
                text = RdText.FromTokens(context, startTokenIndex, endTokenIndex);
                context.Tokens.Position = endTokenIndex;
            }

            return text.Trim();
        }

        public static string FromTokens(RdParseContext context, int startTokenIndex, int endTokenIndex) {
            Debug.Assert(startTokenIndex >= 0 && startTokenIndex < endTokenIndex);

            // Clean descripton so it only consists of plain text
            var sb = new StringBuilder();
            for (int i = startTokenIndex; i < endTokenIndex; i++) {
                var range = TextRange.FromBounds(context.Tokens[i].End, context.Tokens[i + 1].Start);
                var s = context.TextProvider.GetText(range);

                s = CleanRawRdText(s);
                sb.Append(s);
            }
            return sb.ToString();
        }

        public static string CleanRawRdText(string rawRdText) {
            var sb = new StringBuilder();

            for (int i = 0; i < rawRdText.Length; i++) {
                char ch = rawRdText[i];

                if (char.IsWhiteSpace(ch)) {
                    ch = ' ';
                } else if (ch == '\\') {
                    continue; // skip escapes
                }

                if (ch != ' ' || (sb.Length > 0 && sb[sb.Length - 1] != ' ') || sb.Length == 0) {
                    sb.Append(ch);
                }
            }

            return sb.ToString();
        }
    }
}
