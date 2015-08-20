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
            int end = tokens.Position;

            RdBraceCounter<RdToken> braceCounter = new RdBraceCounter<RdToken>(
                new RdToken(RdTokenType.OpenCurlyBrace),
                new RdToken(RdTokenType.CloseCurlyBrace),
                new RdToken(RdTokenType.OpenSquareBracket),
                new RdToken(RdTokenType.CloseSquareBracket)
                );

            tokens.MoveToNextToken();

            while (!tokens.IsEndOfStream())
            {
                if (braceCounter.CountBrace(tokens.CurrentToken))
                {
                    if (braceCounter.Count == 0)
                        break;
                }
                else
                {
                    tokens.MoveToNextToken();
                }
            }

            if (braceCounter.Count == 0)
            {
                end = tokens.Position;
            }

            return end;
        }
    }
}
