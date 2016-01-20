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

    }
}
