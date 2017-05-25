// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using System.Text;
using Microsoft.Common.Core;
using static System.FormattableString;

namespace Microsoft.R.Host.Client {
    public static class RStringExtensions {
        public static string EnsureLineBreak(this string s) => s.Length > 0 && s[s.Length - 1] == '\n' ? s : s + "\n";

        public static string ToRStringLiteral(this string s, char quote = '"', string nullValue = "NULL") {
            Debug.Assert(quote == '"' || quote == '\'');

            if (s == null) {
                return nullValue;
            }

            return quote +
                s.Replace("\\", "\\\\")
                .Replace("" + quote, "\\" + quote)
                .Replace("\n", "\\n")
                .Replace("\r", "\\r")
                .Replace("\t", "\\t")
                .Replace("\b", "\\b")
                .Replace("\a", "\\a")
                .Replace("\f", "\\f")
                .Replace("\v", "\\v") +
                quote;
        }

        public static string ToRBooleanLiteral(this bool b) =>
            b ? "TRUE" : "FALSE";

        public static string FromRStringLiteral(this string s) {
            if (s.Length < 2) {
                throw new FormatException("Not a quoted R string literal");
            }

            var quote = s[0];
            if (s[s.Length - 1] != quote) {
                throw new FormatException("Mismatching quotes");
            }

            var sb = new StringBuilder();
            var escape = false;
            for (var i = 1; i < s.Length - 1; ++i) {
                var c = s[i];
                if (escape) {
                    // https://stat.ethz.ch/R-manual/R-devel/library/base/html/Quotes.html
                    switch (c) {
                        case 'n':
                            sb.Append("\n");
                            break;
                        case 'r':
                            sb.Append("\r");
                            break;
                        case 't':
                            sb.Append("\t");
                            break;
                        case 'b':
                            sb.Append("\b");
                            break;
                        case 'a':
                            sb.Append("\a");
                            break;
                        case 'f':
                            sb.Append("\f");
                            break;
                        case 'v':
                            sb.Append("\v");
                            break;
                        case 'x':
                            // 1 to 2 hex digits, lowercase or uppercase
                            if (i < s.Length - 1) {
                                int val;
                                if (HexCharToDecimal(s[i + 1], out val)) {
                                    i++;
                                    if (i < s.Length - 1) {
                                        int nextVal;
                                        if (HexCharToDecimal(s[i + 1], out nextVal)) {
                                            i++;
                                            val = (val << 4) | nextVal;
                                        }
                                    }

                                    sb.Append(Char.ConvertFromUtf32(val));
                                } else {
                                    throw new FormatException("Expected hex character");
                                }
                            } else {
                                throw new FormatException("Unexpected end of string");
                            }
                            break;
                        case '\\':
                            sb.Append(c);
                            break;
                        case '"':
                            sb.Append(c);
                            break;
                        case '\'':
                            sb.Append(c);
                            break;
                        default:
                            if (c >= '0' && c <= '7') {
                                // 1 to 3 octal digits
                                var val = c - '0';
                                if (i < s.Length - 1) {
                                    var next = s[i + 1];
                                    if (next >= '0' && next <= '7') {
                                        i++;
                                        val = (val << 3) | (next - '0');
                                        if (i < s.Length - 1) {
                                            next = s[i + 1];
                                            if (next >= '0' && next <= '7') {
                                                i++;
                                                val = (val << 3) | (next - '0');
                                            }
                                        }
                                    }
                                }

                                sb.Append(char.ConvertFromUtf32(val));
                                break;
                            }

                            throw new FormatException("Unrecognized escape sequence");
                    }
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

        private static bool HexCharToDecimal(char c, out int val) {
            if (c >= 'a' && c <= 'f') {
                val = c - 'a' + 10;
                return true;
            } else if (c >= 'A' && c <= 'F') {
                val = c - 'A' + 10;
                return true;
            } else if (c >= '0' && c <= '9') {
                val = c - '0';
                return true;
            }
            val = -1;
            return false;
        }

        /// <summary>
        /// Converts fancy quotes specified in the Unicode control range specifically,
        /// 0x91-0x94 to visible quotes. These are 'respectively are left/right single 
        /// quotes and left/right double quotes.
        /// Example: https://everythingfonts.com/unicode/0x0091.
        /// </summary>
        public static string ToUnicodeQuotes(this string s) {
            var sb = new StringBuilder(s.Length);
            for (var i = 0; i < s.Length; i++) {
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

        public static string ToRPath(this string s) => s.Replace("\\", "/");
        public static string FromRPath(this string s) => s.Replace("/", "\\");

        public static string ProjectRelativePathToRemoteProjectPath(this string path, string remoteRoot, string projectName) {
            if (string.IsNullOrWhiteSpace(projectName)) {
                return Invariant($"{remoteRoot}/{path}")?.ToRPath().Replace("//", "/");
            } else {
                return Invariant($"{remoteRoot}/{projectName}/{path}")?.ToRPath().Replace("//", "/");
            }
        }

        /// <summary>
        /// Convert R string that comes encoded into &lt;U+ABCD&gt; into Unicode
        /// characters so user can see actual language symbols rather than 
        /// the character codes. Trims trailing '| __truncated__' that R tends 
        /// to append at the end.
        /// </summary>
        public static string ConvertCharacterCodes(this string s) {
            var t = s.IndexOfOrdinal("\"| __truncated__");
            if (t >= 0) {
                s = s.Substring(0, t);
            }

            if (s.IndexOfOrdinal("<U+") < 0) {
                // Nothing to convert
                return s;
            }

            var converted = new char[s.Length];
            var j = 0;
            for (var i = 0; i < s.Length;) {
                if (i <= s.Length - 8 &&
                    s[i] == '<' && s[i + 1] == 'U' && s[i + 2] == '+' && s[i + 7] == '>') {
                    var code = s.SubstringToHex(i + 3, 4);
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
