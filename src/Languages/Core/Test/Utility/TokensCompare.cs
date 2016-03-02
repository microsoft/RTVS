// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.Languages.Core.Tokens;

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
        public static void Compare(IReadOnlyCollection<TokenData<TTokenType>> expectedTokens, IReadOnlyCollection<TTokenClass> actualTokens) {
            actualTokens.Should().HaveSameCount(expectedTokens);

            IEnumerable<TokenData<TTokenType>> expectedEnum = expectedTokens;
            IEnumerable<TTokenClass> actualEnum = actualTokens;

            IEnumerator<TokenData< TTokenType >> expectedEnumerator = expectedEnum.GetEnumerator();
            IEnumerator<TTokenClass> actualEnumerator = actualEnum.GetEnumerator();

            for (var i = 0; i < expectedTokens.Count; i++)
            {
                expectedEnumerator.MoveNext();
                actualEnumerator.MoveNext();

                TokenData<TTokenType> expected = expectedEnumerator.Current;
                TTokenClass actual = actualEnumerator.Current;

                actual.Should().HaveType(expected.TokenType)
                    .And.StartAt(expected.Start)
                    .And.HaveLength(expected.Length);
            }
        }
    }
}
