// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using System.Text;

namespace Microsoft.R.Host.Client {
    public static class RStringExtensions {
        public static string ToRStringLiteral(this string s, char quote = '"', string nullValue = "NULL") {
            Debug.Assert(quote == '"' || quote == '\'');

            if (s == null) {
                return nullValue;
            }

            return quote + s.Replace("\\", "\\\\").Replace("" + quote, "\\" + quote) + quote;
        }

        public static string FromRStringLiteral(this string s) {
            if (s.Length < 2) {
                throw new FormatException("Not a quoted R string literal");
            }

            char quote = s[0];
            if (s[s.Length - 1] != quote) {
                throw new FormatException("Mismatching quotes");
            }

            var sb = new StringBuilder();
            bool escape = false;
            for (int i = 1; i < s.Length - 1; ++i) {
                char c = s[i];
                if (escape) {
                    sb.Append(c);
                    escape = false;
                } else {
                    if (c == quote) {
                        throw new FormatException("Unescaped embedded quote");
                    } else if (c == '\\') {
                        escape = true;
                    } else {
                        sb.Append(c);
                    }
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Converts fancy quotes specified in the Unicode control range specifically,
        /// 0x91-0x94 to visible quotes. These are 'respectively are left/right single 
        /// quotes and left/right double quotes.
        /// Example: https://everythingfonts.com/unicode/0x0091.
        /// </summary>
        public static string ToUnicodeQuotes(this string s) {
            var sb = new StringBuilder(s.Length);
            for (int i = 0; i < s.Length; i++) {
                char ch;
                switch (s[i]) {
                    case (char)0x0091:
                        ch = (char)0x2018; // Left single quote
                        break;
                    case (char)0x0092:
                        ch = (char)0x2019; // Right single quote
                        break;
                    case (char)0x0093:
                        ch = (char)0x201C; // Left double quote
                        break;
                    case (char)0x0094:
                        ch = (char)0x201D; // Right double quote
                        break;
                    default:
                        ch = s[i];
                        break;
                }
                sb.Append(ch);
            }
            return sb.ToString();
        }

        public static string ToRName(this string s) {
            if (s.Length > 0) {
                if (s[0] != '`') {
                    s = "`" + s;
                }
                if (s[s.Length - 1] != '`') {
                    s = s + "`";
                }
            }
            return s;
        }
    }
}
