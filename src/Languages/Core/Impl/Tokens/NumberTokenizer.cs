using System;
using System.Diagnostics;
using Microsoft.Languages.Core.Text;

namespace Microsoft.Languages.Core.Tokens
{
    public static class NumberTokenizer
    {
//        public static object CharacterSteam { get; private set; }

        public static int HandleNumber(CharacterStream cs)
        {
            int start = cs.Position;

            if (cs.CurrentChar == '-' || cs.CurrentChar == '+')
            {
                cs.MoveToNextChar();
            }

            if (cs.CurrentChar == '0' && cs.NextChar == 'x')
            {
                cs.Advance(2);
                return HandleHex(cs, start);
            }

            if (cs.CurrentChar == 'x' && CharacterStream.IsHex(cs.NextChar))
            {
                cs.MoveToNextChar();
                return HandleHex(cs, start);
            }

            int integerPartStart = cs.Position;
            int integerPartLength = 0;
            int fractionPartLength = 0;
            bool isDouble = false;

            // collect decimals (there may be none like in .1e+20
            while (cs.IsDecimal())
            {
                cs.MoveToNextChar();
                integerPartLength++;
            }

            if (cs.CurrentChar == '.')
            {
                isDouble = true;

                // float/double
                cs.MoveToNextChar();

                // If we've seen don we need to collect factional part of any
                while (cs.IsDecimal())
                {
                    cs.MoveToNextChar();
                    fractionPartLength++;
                }
            }

            if (integerPartLength + fractionPartLength == 0)
            {
                return 0; // +e or +.e is not a number and neither is lonely + or -
            }

            if (cs.CurrentChar == 'e' || cs.CurrentChar == 'E')
            {
                int numberLength = HandleExponent(cs, start);
                if(numberLength > 0)
                {
                    return IsValidDouble(cs, start, cs.Position) ? numberLength : 0;
                }
            }

            if (!isDouble)
            {
                if (cs.CurrentChar == 'L')
                {
                    cs.MoveToNextChar();
                }
            }
            else
            {
                return IsValidDouble(cs, start, cs.Position) ? cs.Position - start : 0;
            }

            return cs.Position - start;
        }

        private static bool IsValidDouble(CharacterStream cs, int start, int end)
        {
            int len = end - start;
            string s = cs.GetSubstringAt(start, len);
            double n;
            return Double.TryParse(s, out n);
        }

        internal static int HandleHex(CharacterStream cs, int start)
        {
            while (CharacterStream.IsHex(cs.CurrentChar))
            {
                cs.MoveToNextChar();
            }

            // TODO: handle C99 floating point hex syntax like 0x1.1p-2
            if (cs.CurrentChar == 'L')
            {
                cs.MoveToNextChar();
            }

            return cs.Position - start;
        }

        internal static int HandleExponent(CharacterStream cs, int start)
        {
            Debug.Assert(cs.CurrentChar == 'E' || cs.CurrentChar == 'e');

            bool hasSign = false;

            if (cs.IsWhiteSpace() || cs.IsEndOfStream())
            {
                // 0.1E
                cs.MoveToNextChar();
                return cs.Position - start;
            }

            cs.MoveToNextChar();

            if (cs.CurrentChar == '-' || cs.CurrentChar == '+')
            {
                hasSign = true;
                cs.MoveToNextChar();
            }

            int digitsStart = cs.Position;

            // collect decimals
            while (cs.IsDecimal())
            {
                cs.MoveToNextChar();
            }

            if (hasSign && digitsStart == cs.Position)
                return 0; // NaN like 1.0E-

            // Technically if letter or braces follows this is not 
            // a number but we'll leave it alone for now.
            if (char.IsLetter(cs.CurrentChar) && cs.CurrentChar != 'i')
            {
                return 0;
            }

            if (cs.CurrentChar == '[' || cs.CurrentChar == ']' ||
                cs.CurrentChar == '{' || cs.CurrentChar == '}' ||
                cs.CurrentChar == '(' || cs.CurrentChar == ')')
            {
                return 0;
            }

            return cs.Position - start;
        }

        public static int HandleImaginaryPart(CharacterStream cs)
        {
            int start = cs.Position;

            // Check if this is actually complex number
            NumberTokenizer.SkipWhitespace(cs);

            if (cs.CurrentChar == '+' || cs.CurrentChar == '-')
            {
                cs.MoveToNextChar();

                if (cs.CurrentChar == '+' || cs.CurrentChar == '-')
                {
                    cs.MoveToNextChar();
                }

                int imaginaryLength = NumberTokenizer.HandleNumber(cs);
                if (imaginaryLength > 0)
                {
                    if (cs.CurrentChar == 'i')
                    {
                        cs.MoveToNextChar();
                        return cs.Position - start;
                    }
                }
            }

            return 0;
        }

        internal static void SkipWhitespace(CharacterStream cs)
        {
            while (!cs.IsEndOfStream() && cs.IsWhiteSpace())
            {
                cs.MoveToNextChar();
            }
        }
    }
}
