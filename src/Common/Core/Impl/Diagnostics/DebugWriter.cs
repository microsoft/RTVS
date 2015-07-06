using System;
using System.Globalization;
using System.Text;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Core.Tokens;

namespace Microsoft.Languages.Core.Diagnostics
{
    public class DebugWriter
    {
        public static string WriteTokens<Token, TokenType>(IReadOnlyTextRangeCollection<Token> tokens) where Token : ITextRange
        {
            var sb = new StringBuilder();

            foreach (var range in tokens)
            {
                IToken<TokenType> token = (IToken<TokenType>)range;
                string enumName = Enum.GetName(typeof(TokenType), token.TokenType);

                int spaceCount = 20 - enumName.Length;
                var sbSpaces = new StringBuilder(spaceCount);
                sbSpaces.Append(' ', spaceCount);

                sb.AppendFormat(CultureInfo.InvariantCulture, "{0}{1} : {2} - {3}\t({4})\r\n", enumName, sbSpaces.ToString(), token.Start, token.End, token.Length);
            }

            string s = sb.ToString();
            return s;
        }
    }
}
