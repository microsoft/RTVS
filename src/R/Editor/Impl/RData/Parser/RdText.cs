// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using System.Text;
using Microsoft.Languages.Core.Text;

namespace Microsoft.R.Editor.RData.Parser {
    internal static class RdText {
        public static string GetText(RdParseContext context) {
            var text = string.Empty;

            if (RdParseUtility.GetKeywordArgumentBounds(context.Tokens, out var startTokenIndex, out var endTokenIndex)) {
                text = RdText.FromTokens(context, startTokenIndex, endTokenIndex);
                context.Tokens.Position = endTokenIndex;
            }

            text = text.Trim();
            var index = text.IndexOf(@"\href", StringComparison.Ordinal);
            if (index >= 0) {
                var openCurlyIndex = text.IndexOf('{', index);
                var closeCurlyIndex = text.LastIndexOf('}', openCurlyIndex);
                openCurlyIndex = text.IndexOf('{', closeCurlyIndex);
                closeCurlyIndex = text.LastIndexOf('}', openCurlyIndex);
                if (openCurlyIndex >= 0 && closeCurlyIndex >= 0 && openCurlyIndex < closeCurlyIndex) {
                    var name = text.Substring(openCurlyIndex + 1, closeCurlyIndex - openCurlyIndex - 1);
                    text = text.Substring(0, index) + name + text.Substring(closeCurlyIndex + 1);
                }
            }
            return text;
        }

        public static string GetHyperlinkName(RdParseContext context) {
            // \href{{http://rlang.tidyverse.org/articles/tidy-evaluation.html}{tidy evaluationframework}}
            var text = GetText(context); // yields {http://rlang.tidyverse.org/articles/tidy-evaluation.html}{tidy evaluationframework}
            var lastOpenCurlyIndex = text.LastIndexOf('{');
            var lastCloseCurlyIndex = text.LastIndexOf('}');
            if (lastOpenCurlyIndex >= 0 && lastCloseCurlyIndex >= 0 && lastOpenCurlyIndex < lastCloseCurlyIndex) {
                return text.Substring(lastOpenCurlyIndex + 1, lastCloseCurlyIndex - lastOpenCurlyIndex - 1);
            }
            return string.Empty;
        }

        public static string FromTokens(RdParseContext context, int startTokenIndex, int endTokenIndex) {
            Debug.Assert(startTokenIndex >= 0 && startTokenIndex < endTokenIndex);

            // Clean descripton so it only consists of plain text
            var sb = new StringBuilder();
            for (var i = startTokenIndex; i < endTokenIndex; i++) {
                var range = TextRange.FromBounds(context.Tokens[i].End, context.Tokens[i + 1].Start);
                var s = context.TextProvider.GetText(range);

                s = CleanRawRdText(s);
                sb.Append(s);
            }
            return sb.ToString();
        }

        public static string CleanRawRdText(string rawRdText) {
            var sb = new StringBuilder();

            foreach (var t in rawRdText) {
                var ch = t;

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
