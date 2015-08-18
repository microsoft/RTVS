using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Languages.Core.Tokens;
using Microsoft.R.Support.RD.Tokens;

namespace Microsoft.R.Support.RD.Parser
{
    static class RdParseUtility
    {
        public static int FindRdKeywordArgumentBounds(TokenStream<RdToken> tokens)
        {
            int start = tokens.Position;

            int end = SkipAllBraces(tokens);
            tokens.Position = start;

            return end;
        }

        private static int SkipAllBraces(TokenStream<RdToken> tokens)
        {
            if (tokens.CurrentToken.TokenType != RdTokenType.OpenBrace && tokens.NextToken.TokenType == RdTokenType.OpenBrace)
            {
                tokens.MoveToNextToken();
            }

            Debug.Assert(tokens.CurrentToken.TokenType == RdTokenType.OpenBrace);

            Stack<RdToken> braces = new Stack<RdToken>();
            int start = tokens.Position;
            int end = -1;
            int i;

            braces.Push(tokens[start]);

            for (i = start + 1; i < tokens.Length && braces.Count > 0; i++)
            {
                RdToken token = tokens[i];

                switch (token.TokenType)
                {
                    case RdTokenType.OpenBrace:
                        braces.Push(token);
                        break;

                    case RdTokenType.CloseBrace:
                        if (braces.Count > 0)
                        {
                            braces.Pop();
                        }
                        else
                        {
                            return -1;
                        }
                        break;
                }
            }

            if (braces.Count == 0)
            {
                end = i;
            }

            return end;
        }
    }
}
