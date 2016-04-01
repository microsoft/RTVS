// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.Languages.Core.Test.Tokens;
using Microsoft.R.Core.Tokens;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.R.Core.Test.Tokens {
    [ExcludeFromCodeCoverage]
    public class TokenizeNonAnsiTest : TokenizeTestBase<RToken, RTokenType> {
        [Test]
        [Category.R.Tokenizer]
        public void Tokenize_NonAnsi01() {
            var tokens = Tokenize(" русский ", new RTokenizer());

            tokens.Should().ContainSingle()
                .Which.Should().HaveType(RTokenType.Identifier)
                .And.StartAt(1)
                .And.HaveLength(7);
        }

        [Test]
        [Category.R.Tokenizer]
        public void Tokenize_NonAnsi02() {
            var tokens = Tokenize("äöütest <- .äöü1", new RTokenizer());

            tokens.Should().Equal(new[] {
                RTokenType.Identifier,
                RTokenType.Operator,
                RTokenType.Identifier,
            }, (token, tokenType) => token.TokenType == tokenType);
        }

        [Test]
        [Category.R.Tokenizer]
        public void Tokenize_NonAnsi03() {
            var tokens = Tokenize("注目記事 <- .企画.特集", new RTokenizer());

            tokens.Should().Equal(new[] {
                RTokenType.Identifier,
                RTokenType.Operator,
                RTokenType.Identifier,
            }, (token, tokenType) => token.TokenType == tokenType);
        }
    }
}
