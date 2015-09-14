using System;
using Microsoft.Languages.Core.Text;

namespace Microsoft.R.Core.Tokens
{
    public static class Operators
    {
        /// <summary>
        /// Given candidate returns length of operator
        /// or zero if character sequence is not an operator.
        /// </summary>
        public static int OperatorLength(CharacterStream cs)
        {
            //
            // http://stat.ethz.ch/R-manual/R-patched/library/base/html/Syntax.html
            //

            // Longest first
            return GetNCharOperatorLength(cs);
        }

        private static int GetNCharOperatorLength(CharacterStream cs)
        {
            if (cs.CurrentChar == '%' && Char.IsLetter(cs.NextChar))
            {
                int start = cs.Position;
                int length;

                cs.Advance(2);

                while (!cs.IsEndOfStream() && !cs.IsWhiteSpace())
                {
                    if (cs.CurrentChar == '%')
                    {
                        cs.MoveToNextChar();

                        length = cs.Position - start;
                        cs.Position = start;

                        return length;
                    }

                    if (!Char.IsLetterOrDigit(cs.CurrentChar))
                    {
                        break;
                    }

                    cs.MoveToNextChar();
                }
            }

            return Get3CharOrShorterOperatorLength(cs);
        }

        private static int Get3CharOrShorterOperatorLength(CharacterStream cs)
        {
            if (cs.DistanceFromEnd >= 3)
            {
                string threeLetterCandidate = cs.GetSubstringAt(cs.Position, 3);
                if (threeLetterCandidate.Length == 3)
                {
                    int index = Array.BinarySearch<string>(_threeChars, threeLetterCandidate);
                    if (index >= 0)
                    {
                        return 3;
                    }
                }
            }

            return Get2CharOrShorterOperatorLength(cs);
        }

        internal static int Get2CharOrShorterOperatorLength(CharacterStream cs)
        {
            if (cs.DistanceFromEnd >= 2)
            {
                string twoLetterCandidate = cs.GetSubstringAt(cs.Position, 2);

                if (twoLetterCandidate.Length == 2)
                {
                    int index = Array.BinarySearch<string>(_twoChars, twoLetterCandidate);
                    if(index >= 0)
                    {
                        return 2;
                    }
                }
            }

            return GetSingleCharOperatorLength(cs.CurrentChar);
        }

        private static int GetSingleCharOperatorLength(char candidate)
        {
            switch (candidate)
            {
                case '~': // as in formulae
                case '^': // exponentiation (right to left)
                case '+':
                case '-':
                case '*':
                case '/':
                case '$': // component / slot extraction
                case '@': // component / slot extraction
                case '<':
                case '>':
                case '|':
                case '&':
                case '!':
                case '=':
                case '?':
                case ':': // sequence operator
                    return 1;

                default:
                    break;
            }

            return 0;
        }

        // must be sorted
        internal static string[] _twoChars = new string[]
            {
            "!=",
            "%%",
            "&&",
            "**", // alternative to ^
            "::",
            "||",
            "<-",
            "<=",
            "==",
            "->",
            ">=",
            };

        // must be sorted
        internal static string[] _threeChars = new string[]
        {
            "%*%",
            "%/%",
            ":::",
            "<<-",
            "->>",
        };
    }
}