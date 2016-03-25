using System;
using System.Globalization;

namespace Microsoft.R.Core.Tokens {
    public static class Number {
        private const NumberStyles _hexStyle = NumberStyles.HexNumber | NumberStyles.AllowHexSpecifier;
        private const NumberStyles _integerStyle = NumberStyles.Integer;
        private const NumberStyles _doubleStyle = NumberStyles.Number | NumberStyles.AllowExponent;

        public static bool TryParse(string text, out double doubleResult, bool allowLSuffix = true) {
            doubleResult = Double.NaN;
            int sign = 1;

            if (text.Length == 0) {
                return false;
            }
            if (allowLSuffix && text[text.Length - 1] == 'L') {
                text = text.Substring(0, text.Length - 1);
            }

            if (text.Length == 0) {
                return false;
            }
            if (text[0] == '-' || text[0] == '+') {
                text = text.Substring(1);
                sign = text[0] == '-' ? -1 : 1;
            }

            int intResult = 0;
            if (Int32.TryParse(text, _integerStyle, CultureInfo.InvariantCulture, out intResult)) {
                doubleResult = sign * intResult;
            } else {
                if (Double.TryParse(text, _doubleStyle, CultureInfo.InvariantCulture, out doubleResult)) {
                    doubleResult = sign * doubleResult;
                } else if (Int32.TryParse(text, _hexStyle, CultureInfo.InvariantCulture, out intResult)) {
                    doubleResult = sign * intResult;
                }
            }
            return doubleResult != Double.NaN;
        }
    }
}
