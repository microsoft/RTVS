// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Languages.Core.Text;

namespace Microsoft.Languages.Core.Tokens {
    public static class Tokenizer {
        /// <summary>
        /// Handle generic comment. Comment goes to the end of the line.
        /// </summary>
        public static void HandleEolComment(CharacterStream cs, Action<int, int> addToken) {
            int start = cs.Position;

            while (!cs.IsEndOfStream() && !cs.IsAtNewLine()) {
                cs.MoveToNextChar();
            }

            int length = cs.Position - start;
            if (length > 0) {
                addToken(start, length);
            }
        }

        /// <summary>
        /// Handles string sequence with escapes
        /// </summary>
        /// <param name="openQuote"></param>
        public static void HandleString(char openQuote, CharacterStream cs, Action<int, int> addToken) {
            int start = cs.Position;

            cs.MoveToNextChar();

            if (!cs.IsEndOfStream()) {
                while (true) {
                    if (cs.CurrentChar == openQuote) {
                        cs.MoveToNextChar();
                        break;
                    }

                    if (cs.CurrentChar == '\\') {
                        cs.MoveToNextChar();
                    }

                    if (!cs.MoveToNextChar()) {
                        break;
                    }
                }
            }

            int length = cs.Position - start;
            if (length > 0) {
                addToken(start, length);
            }
        }

        public static void SkipIdentifier(CharacterStream cs, Func<CharacterStream, bool> isIdentifierLeadCharacter, Func<CharacterStream, bool> isIdentifierCharacter) {
            if (!isIdentifierLeadCharacter(cs)) {
                return;
            }

            if (cs.IsEndOfStream()) {
                return;
            }

            while (!cs.IsWhiteSpace()) {
                if (!isIdentifierCharacter(cs)) {
                    break;
                }

                if (!cs.MoveToNextChar()) {
                    break;
                }
            }
        }
    }
}
