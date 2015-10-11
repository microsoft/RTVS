using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.R.Debugger {
    internal static class DebugUtilities {
        internal static string ToRStringLiteral(this string s, char quote = '"', string nullValue = "NULL") {
            Debug.Assert(quote == '"' || quote == '\'');

            if (s == null) {
                return nullValue;
            }

            return quote + s.Replace("\\", "\\\\").Replace("" + quote, "\\" + quote) + quote;
        }
    }
}
