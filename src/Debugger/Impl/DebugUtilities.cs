using System;
using System.Diagnostics;
using System.Text;

namespace Microsoft.R.Debugger {
    public static class DebugUtilities {
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
    }
}
