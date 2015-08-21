using Microsoft.Languages.Core.Tokens;
using Microsoft.R.Support.RD.Tokens;

namespace Microsoft.R.Support.RD.Parser
{
    static class RdParseUtility
    {
        public static bool GetKeywordArgumentBounds(TokenStream<RdToken> tokens, out int startTokenIndex, out int endTokenIndex)
        {
            startTokenIndex = -1;
            endTokenIndex = -1;

            RdBraceCounter<RdToken> braceCounter = new RdBraceCounter<RdToken>(
                new RdToken(RdTokenType.OpenCurlyBrace),
                new RdToken(RdTokenType.CloseCurlyBrace),
                new RdToken(RdTokenType.OpenSquareBracket),
                new RdToken(RdTokenType.CloseSquareBracket)
                );

            for (int pos = tokens.Position; pos < tokens.Length; pos++)
            {
                RdToken token = tokens[pos];

                if (braceCounter.CountBrace(token))
                {
                    if (startTokenIndex < 0)
                    {
                        startTokenIndex = pos;
                    }

                    if (braceCounter.Count == 0)
                    {
                        endTokenIndex = pos;
                        break;
                    }
                }
            }

            return startTokenIndex >= 0 && endTokenIndex >= 0 && startTokenIndex < endTokenIndex;
        }
    }
}
