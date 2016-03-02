// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.Languages.Core.Test.Tokens;
using Microsoft.R.Core.Tokens;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.R.Core.Test.Tokens {
    [ExcludeFromCodeCoverage]
    public class TokenizeIndexerTest : TokenizeTestBase<RToken, RTokenType> {
        [Test]
        [Category.R.Tokenizer]
        public void TokenizeIndexerTest01() {
            var tokens = Tokenize("a[b[c]]", new RTokenizer());
            tokens.Should().Equal(new[] {
                RTokenType.Identifier,
                RTokenType.OpenSquareBracket,
                RTokenType.Identifier,
                RTokenType.OpenSquareBracket,
                RTokenType.Identifier,
                RTokenType.CloseSquareBracket,
                RTokenType.CloseSquareBracket
            }, (token, tokenType) => token.TokenType == tokenType);
        }

        [Test]
        [Category.R.Tokenizer]
        public void TokenizeIndexerTest02() {
            var tokens = Tokenize("a[[b[c]]]", new RTokenizer());

            tokens.Should().Equal(new[] {
                RTokenType.Identifier,
                RTokenType.OpenDoubleSquareBracket,
                RTokenType.Identifier,
                RTokenType.OpenSquareBracket,
                RTokenType.Identifier,
                RTokenType.CloseSquareBracket,
                RTokenType.CloseDoubleSquareBracket
            }, (token, tokenType) => token.TokenType == tokenType);
        }

        [Test]
        [Category.R.Tokenizer]
        public void TokenizeIndexerTest03() {
            var tokens = Tokenize("a[[b]][x]", new RTokenizer());

            tokens.Should().Equal(new[] {
                RTokenType.Identifier,
                RTokenType.OpenDoubleSquareBracket,
                RTokenType.Identifier,
                RTokenType.CloseDoubleSquareBracket,
                RTokenType.OpenSquareBracket,
                RTokenType.Identifier,
                RTokenType.CloseSquareBracket
            }, (token, tokenType) => token.TokenType == tokenType);
        }

        [Test]
        [Category.R.Tokenizer]
        public void TokenizeIndexerTest04() {
            var tokens = this.Tokenize("a[[b][x]]", new RTokenizer());

            tokens.Should().Equal(new[] {
                RTokenType.Identifier,
                RTokenType.OpenDoubleSquareBracket,
                RTokenType.Identifier,
                RTokenType.CloseSquareBracket,
                RTokenType.OpenSquareBracket,
                RTokenType.Identifier,
                RTokenType.CloseSquareBracket,
                RTokenType.CloseSquareBracket
            }, (token, tokenType) => token.TokenType == tokenType);
        }
    }
}
