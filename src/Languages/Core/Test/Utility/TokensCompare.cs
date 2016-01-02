using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Languages.Core.Tokens;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Languages.Core.Test.Utility
{
    [ExcludeFromCodeCoverage]
    public sealed class TokenData<TTokenType>
    {
        public TTokenType TokenType;
        public int Start;
        public int Length;

        public TokenData(TTokenType tokenType, int start, int length)
        {
            TokenType = tokenType;
            Start = start;
            Length = length;
        }
    }

    [ExcludeFromCodeCoverage]
    public static class TokensCompare<TTokenType, TTokenClass> where TTokenClass: IToken<TTokenType>
    {
        public static void Compare(IReadOnlyCollection<TokenData<TTokenType>> expectedTokens, IReadOnlyCollection<TTokenClass> actualTokens)
        {
            Assert.AreEqual(expectedTokens.Count, actualTokens.Count);

            IEnumerable<TokenData< TTokenType >> expectedEnum = expectedTokens as IEnumerable<TokenData<TTokenType>>;
            IEnumerable<TTokenClass> actualEnum = actualTokens as IEnumerable<TTokenClass>;

            IEnumerator<TokenData< TTokenType >> expectedEnumerator = expectedEnum.GetEnumerator();
            IEnumerator<TTokenClass> actualEnumerator = actualEnum.GetEnumerator();

            for (int i = 0; i < expectedTokens.Count; i++)
            {
                expectedEnumerator.MoveNext();
                actualEnumerator.MoveNext();

                TokenData<TTokenType> expected = expectedEnumerator.Current;
                TTokenClass actual = actualEnumerator.Current;

                Assert.AreEqual(expected.TokenType, actual.TokenType);
                Assert.AreEqual(expected.Start, actual.Start);
                Assert.AreEqual(expected.Length, actual.Length);
            }
        }
    }
}
