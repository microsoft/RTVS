// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using System.Globalization;
using Microsoft.Languages.Core.Text;

namespace Microsoft.Languages.Core.Tokens {
    public static class NumberTokenizer {
        // public static object CharacterSteam { get; private set; }

        public static int HandleNumber(CharacterStream cs) {
            int start = cs.Position;

            if (cs.CurrentChar == '-' || cs.CurrentChar == '+') {
                cs.MoveToNextChar();
            }

            if (cs.CurrentChar == '0' && cs.NextChar == 'x') {
                cs.Advance(2);
                return HandleHex(cs, start);
            }

            if (cs.CurrentChar == 'x' && CharacterStream.IsHex(cs.NextChar)) {
                cs.MoveToNextChar();
                return HandleHex(cs, start);
            }

            int integerPartStart = cs.Position;
            int integerPartLength = 0;
            int fractionPartLength = 0;
            bool isDouble = false;

            // collect decimals (there may be none like in .1e+20
            while (cs.IsDecimal()) {
                cs.MoveToNextChar();
                integerPartLength++;
            }

            if (cs.CurrentChar == '.') {
                isDouble = true;

                // float/double
                cs.MoveToNextChar();

                // If we've seen don we need to collect factional part of any
                while (cs.IsDecimal()) {
                    cs.MoveToNextChar();
                    fractionPartLength++;
                }
            }

            if (integerPartLength + fractionPartLength == 0) {
                return 0; // +e or +.e is not a number and neither is lonely + or -
            }

            int numberLength;
            if (cs.CurrentChar == 'e' || cs.CurrentChar == 'E') {
                isDouble = true;
                numberLength = HandleExponent(cs, start);
            } else {
                numberLength = cs.Position - start;
            }

            // Verify double format
            if (isDouble && !IsValidDouble(cs, start, cs.Position)) {
                numberLength = 0;
            }

            if (numberLength > 0) {
                // skip over trailing 'L' if any
                if (cs.CurrentChar == 'L') {
                    cs.MoveToNextChar();
                    numberLength++;
                }
            }

            return numberLength;
        }

        private static bool IsValidDouble(CharacterStream cs, int start, int end) {
            int len = end - start;
            string s = cs.GetSubstringAt(start, len);
            double n;
            return Double.TryParse(s, NumberStyles.Number | NumberStyles.AllowExponent, CultureInfo.InvariantCulture, out n);
        }

        internal static int HandleHex(CharacterStream cs, int start) {
            while (CharacterStream.IsHex(cs.CurrentChar)) {
                cs.MoveToNextChar();
            }

            // TODO: handle C99 floating point hex syntax like 0x1.1p-2
            if (cs.CurrentChar == 'L') {
                cs.MoveToNextChar();
            }

            return cs.Position - start;
        }

        internal static int HandleExponent(CharacterStream cs, int start) {
            Debug.Assert(cs.CurrentChar == 'E' || cs.CurrentChar == 'e');

            bool hasSign = false;

            cs.MoveToNextChar();
            if (cs.IsWhiteSpace() || cs.IsEndOfStream()) {
                // 0.1E or 1e
                return 0;
            }

            if (cs.CurrentChar == '-' || cs.CurrentChar == '+') {
                hasSign = true;
                cs.MoveToNextChar();
            }

            int digitsStart = cs.Position;

            // collect decimals
            while (cs.IsDecimal()) {
                cs.MoveToNextChar();
            }

            if (hasSign && digitsStart == cs.Position) {
                return 0; // NaN like 1.0E-
            }

            // Technically if letter or braces follows this is not 
            // a number but we'll leave it alone for now.

            // TODO: This code is not language specific and yet it currently
            // handles complex 'i' as well as R-specific 'L' suffix.
            // Ideally this needs to be extended in a way so language-specific
            // tokenizer can specify options or control number format.
            if (char.IsLetter(cs.CurrentChar) && cs.CurrentChar != 'i' && cs.CurrentChar != 'L') {
                return 0;
            }

            return cs.Position - start;
        }

        public static int HandleImaginaryPart(CharacterStream cs) {
            int start = cs.Position;

            // Check if this is actually complex number
            NumberTokenizer.SkipWhitespace(cs);

            if (cs.CurrentChar == '+' || cs.CurrentChar == '-') {
                cs.MoveToNextChar();

                if (cs.CurrentChar == '+' || cs.CurrentChar == '-') {
                    cs.MoveToNextChar();
                }

                int imaginaryLength = NumberTokenizer.HandleNumber(cs);
                if (imaginaryLength > 0) {
                    if (cs.CurrentChar == 'i') {
                        cs.MoveToNextChar();
                        return cs.Position - start;
                    }
                }
            }

            return 0;
        }

        internal static void SkipWhitespace(CharacterStream cs) {
            while (!cs.IsEndOfStream() && cs.IsWhiteSpace()) {
                cs.MoveToNextChar();
            }
        }
    }
}
