using System;
using Microsoft.Languages.Core.Text;

namespace Microsoft.Languages.Core.Tokens
{
    public static class Tokenizer
    {
        /// <summary>
        /// Handle generic comment. Comment goes to the end of the line.
        /// </summary>
        public static void HandleEolComment(CharacterStream cs, Action<int, int> addToken)
        {
            int start = cs.Position;

            while (!cs.IsEndOfStream() && !cs.IsAtNewLine())
            {
                cs.MoveToNextChar();
            }

            int length = cs.Position - start;
            if (length > 0)
            {
                addToken(start, length);
            }
        }

        /// <summary>
        /// Handles string sequence with escapes
        /// </summary>
        /// <param name="openQuote"></param>
        public static void HandleString(char openQuote, CharacterStream cs, Action<int, int> addToken)
        {
            int start = cs.Position;

            cs.MoveToNextChar();

            while (!cs.IsEndOfStream())
            {
                if (cs.CurrentChar == openQuote)
                {
                    cs.MoveToNextChar();
                    break;
                }

                cs.Advance(cs.CurrentChar == '\\' ? 2 : 1);
            }

            int length = cs.Position - start;
            if (length > 0)
            {
                addToken(start, length);
            }
        }

        public static void SkipIdentifier(CharacterStream cs, Func<CharacterStream, bool> isIdentifierLeadCharacter, Func<CharacterStream, bool> isIdentifierCharacter)
        {
            if (!isIdentifierLeadCharacter(cs))
                return;

            while (!cs.IsEndOfStream() && !cs.IsWhiteSpace())
            {
                if (!isIdentifierCharacter(cs))
                    break;

                cs.MoveToNextChar();
            }
        }
    }
}
