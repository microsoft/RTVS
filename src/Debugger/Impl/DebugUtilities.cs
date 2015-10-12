using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.R.Debugger {
    public static class DebugUtilities {
        public static string ToRStringLiteral(this string s, char quote = '"', string nullValue = "NULL") {
            Debug.Assert(quote == '"' || quote == '\'');

            if (s == null) {
                return nullValue;
            }

            return quote + s.Replace("\\", "\\\\").Replace("" + quote, "\\" + quote) + quote;
        }
    }
}
