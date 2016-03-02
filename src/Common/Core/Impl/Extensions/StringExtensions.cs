// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Text;

namespace Microsoft.Common.Core {
    public static class StringExtensions {
        public static bool EqualsOrdinal(this string s, string other) {
            return string.Equals(s, other, StringComparison.Ordinal);
        }
        public static bool EqualsIgnoreCase(this string s, string other) {
            return string.Equals(s, other, StringComparison.OrdinalIgnoreCase);
        }
        public static bool StartsWithIgnoreCase(this string s, string prefix) {
            return s.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
        }
        public static bool EndsWithIgnoreCase(this string s, string suffix) {
            return s.EndsWith(suffix, StringComparison.OrdinalIgnoreCase);
        }
        public static int IndexOfIgnoreCase(this string s, string searchFor) {
            return s.IndexOf(searchFor, StringComparison.OrdinalIgnoreCase);
        }
        public static int LastIndexOfIgnoreCase(this string s, string searchFor) {
            return s.LastIndexOf(searchFor, StringComparison.OrdinalIgnoreCase);
        }
        public static int LastIndexOfIgnoreCase(this string s, string searchFor, int startIndex) {
            return s.LastIndexOf(searchFor, startIndex, StringComparison.OrdinalIgnoreCase);
        }

        public static string Replace(this string s, string oldValue, string newValue, int start, int length) {
            if (string.IsNullOrEmpty(oldValue)) {
                throw new ArgumentException("oldValue can't be null or empty string", nameof(oldValue));
            }

            if (string.IsNullOrEmpty(s)) {
                return s;
            }

            if (start < 0) {
                start = 0;
            }

            if (length < 0) {
                length = 0;
            }

            return new StringBuilder(s)
                .Replace(oldValue, newValue, start, length)
                .ToString();
        }

        public static string RemoveWhiteSpaceLines(this string text) {
            if (string.IsNullOrWhiteSpace(text)) {
                return string.Empty;
            }

            var sb = new StringBuilder(text);
            var lineBreakIndex = sb.Length;
            var isWhiteSpaceOnly = true;
            for (var i = sb.Length - 1; i >= 0; i--) {
                var ch = sb[i];
                if (ch == '\r' || ch == '\n') {
                    if (ch == '\n' && i > 0 && sb[i - 1] == '\r') {
                        i--;
                    }

                    if (isWhiteSpaceOnly) {
                        sb.Remove(i, lineBreakIndex - i);
                    } else if (i == 0) {
                        var rn = sb.Length > 1 && sb[0] == '\r' && sb[1] == '\n';
                        sb.Remove(0, rn ? 2 : 1);
                        break;
                    }

                    lineBreakIndex = i;
                    isWhiteSpaceOnly = true;
                }

                isWhiteSpaceOnly = isWhiteSpaceOnly && char.IsWhiteSpace(ch);
            }

            return sb.ToString();
        }
        public static int SubstringToHex(this string s, int position, int count) {
            int mul = 1 << (4 * (count - 1));
            int result = 0;

            for (int i = 0; i < count; i++) {
                char ch = s[position + i];
                int z;
                if (ch >= '0' && ch <= '9') {
                    z = ch - '0';
                } else if (ch >= 'a' && ch <= 'f') {
                    z = ch - 'a' + 10;
                } else if (ch >= 'A' && ch <= 'F') {
                    z = ch - 'A' + 10;
                } else {
                    return -1;
                }

                result += z * mul;
                mul >>= 4;
            }
            return result;
        }
    }
}
