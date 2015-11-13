using System;

namespace Microsoft.Common.Core {
    public static class StringExtensions {
        public static bool EqualsIgnoreCase(this string s, string other) {
            return string.Compare(s, other, StringComparison.OrdinalIgnoreCase) == 0;
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
    }
}
