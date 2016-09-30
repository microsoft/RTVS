// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using System.Text;
using Microsoft.Common.Core;

namespace Microsoft.R.Host.Client {
    public static class RStringExtensions {
        public static string EnsureLineBreak(this string s) => s.Length > 0 && s[s.Length - 1] == '\n' ? s : s + "\n";

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

        public static string ToRPath(this string s) {
            return s.Replace("\\", "/");
        }

        public static string FromRPath(this string s) {
            return s.Replace("/", "\\");
        }

        public static string ProjectRelativePathToRemoteProjectPath(this string path, string remoteRoot, string projectName) {
            if (string.IsNullOrWhiteSpace(projectName)) {
                return ($"{remoteRoot}/{path}")?.ToRPath();
            } else {
                return ($"{remoteRoot}/{projectName}/{path}")?.ToRPath();
            }
        }

        /// <summary>
        /// Convert R string that comes encoded into &lt;U+ABCD&gt; into Unicode
        /// characters so user can see actual language symbols rather than 
        /// the character codes. Trims trailing '| __truncated__' that R tends 
        /// to append at the end.
        /// </summary>
        public static string ConvertCharacterCodes(this string s) {
            int t = s.IndexOfOrdinal("\"| __truncated__");
            if (t >= 0) {
                s = s.Substring(0, t);
            }

            if (s.IndexOfOrdinal("<U+") < 0) {
                // Nothing to convert
                return s;
            }

            char[] converted = new char[s.Length];
            int j = 0;
            for (int i = 0; i < s.Length;) {
                if (i <= s.Length - 8 &&
                    s[i] == '<' && s[i + 1] == 'U' && s[i + 2] == '+' && s[i + 7] == '>') {
                    int code = s.SubstringToHex(i + 3, 4);
                    if (code > 0 && code < 65535) {
                        converted[j++] = Convert.ToChar(code);
                        i += 8;
                        continue;
                    }
                }
                converted[j++] = s[i++];
            }
            return new string(converted, 0, j);
        }
    }
}
