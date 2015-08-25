using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.R.Support.RD.Tokens;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.R.Support.Test.RD.Utility
{
    [ExcludeFromCodeCoverage]
    class TokenData
    {
        public RdTokenType TokenType;
        public int Start;
        public int Length;

        public TokenData(RdTokenType tokenType, int start, int length)
        {
            TokenType = tokenType;
            Start = start;
            Length = length;
        }
    }

    [ExcludeFromCodeCoverage]
    static class TokensCompare
    {
        public static void Compare(IReadOnlyCollection<TokenData> expectedTokens, IReadOnlyCollection<RdToken> actualTokens)
        {
            Assert.AreEqual(expectedTokens.Count, actualTokens.Count);

            IEnumerable<TokenData> expectedEnum = expectedTokens as IEnumerable<TokenData>;
            IEnumerable<RdToken> actualEnum = actualTokens as IEnumerable<RdToken>;

            IEnumerator<TokenData> expectedEnumerator = expectedEnum.GetEnumerator();
            IEnumerator<RdToken> actualEnumerator = actualEnum.GetEnumerator();

            for (int i = 0; i < expectedTokens.Count; i++)
            {
                expectedEnumerator.MoveNext();
                actualEnumerator.MoveNext();

                TokenData expected = expectedEnumerator.Current;
                RdToken actual = actualEnumerator.Current;

                Assert.AreEqual(expected.TokenType, actual.TokenType);
                Assert.AreEqual(expected.Start, actual.Start);
                Assert.AreEqual(expected.Length, actual.Length);
            }
        }
    }
}
