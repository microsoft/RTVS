// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Languages.Core.Text;

namespace Microsoft.R.Core.Tokens {
    public static class Operators {
        /// <summary>
        /// Given candidate returns length of operator
        /// or zero if character sequence is not an operator.
        /// </summary>
        public static int OperatorLength(CharacterStream cs) {
            //
            // http://stat.ethz.ch/R-manual/R-patched/library/base/html/Syntax.html
            //

            // Longest first
            return GetNCharOperatorLength(cs);
        }

        private static int GetNCharOperatorLength(CharacterStream cs) {
            // R allows user-defined infix operators. These have the form of 
            // a string of characters delimited by the ‘%’ character. The string 
            // can contain any printable character except ‘%’. 
            if (cs.CurrentChar == '%' && !char.IsWhiteSpace(cs.NextChar)) {
                // In case of broken or partially typed operators
                // make sure we terminate at whitespace or end of the line
                // so in 'x <- y % z' '% z' is not an operator.
                var start = cs.Position;

                cs.MoveToNextChar();

                while (!cs.IsEndOfStream() && !cs.IsWhiteSpace()) {
                    if (cs.CurrentChar == '%') {
                        cs.MoveToNextChar();

                        var length = cs.Position - start;
                        cs.Position = start;

                        return length;
                    }

                    if (cs.IsAtNewLine()) {
                        // x <- y %abcd
                        cs.Position = start;
                        return 1;
                    }

                    cs.MoveToNextChar();
                }
            }

            return Get3CharOrShorterOperatorLength(cs);
        }

        private static int Get3CharOrShorterOperatorLength(CharacterStream cs) {
            if (cs.DistanceFromEnd >= 3) {
                var threeLetterCandidate = cs.GetSubstringAt(cs.Position, 3);
                if (threeLetterCandidate.Length == 3) {
                    var index = Array.BinarySearch<string>(_threeChars, threeLetterCandidate);
                    if (index >= 0) {
                        return 3;
                    }
                }
            }

            return Get2CharOrShorterOperatorLength(cs);
        }

        internal static int Get2CharOrShorterOperatorLength(CharacterStream cs) {
            if (cs.DistanceFromEnd >= 2) {
                var twoLetterCandidate = cs.GetSubstringAt(cs.Position, 2);

                if (twoLetterCandidate.Length == 2) {
                    var index = Array.BinarySearch<string>(_twoChars, twoLetterCandidate);
                    if (index >= 0) {
                        return 2;
                    }
                }
            }

            return GetSingleCharOperatorLength(cs.CurrentChar);
        }

        private static int GetSingleCharOperatorLength(char candidate) {
            switch (candidate) {
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
            }
            return 0;
        }

        // must be sorted
        internal static string[] _twoChars = {
            "!=",
            "%%",
            "&&",
            "**", // alternative to ^
            "::",
            ":=", // reserved for data.table package
            "??",
            "||",
            "<-",
            "<=",
            "==",
            "->",
            ">=",
            };

        // must be sorted
        internal static string[] _threeChars = {
            "%*%",
            "%/%",
            ":::",
            "<<-",
            "->>",
        };
    }
}