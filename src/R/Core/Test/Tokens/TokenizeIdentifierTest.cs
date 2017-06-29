// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.Languages.Core.Test.Tokens;
using Microsoft.Languages.Core.Test.Utility;
using Microsoft.R.Core.Tokens;
using Microsoft.UnitTests.Core.XUnit;
using Xunit;

namespace Microsoft.R.Core.Test.Tokens {
    [ExcludeFromCodeCoverage]
    [Category.R.Tokenizer]
    public class TokenizeIdentifierTest : TokenizeTestBase<RToken, RTokenType> {
        private readonly CoreTestFilesFixture _files;

        public TokenizeIdentifierTest(CoreTestFilesFixture files) {
            _files = files;
        }

        [Test]
        public void TokenizeIdentifierTest01() {
            var tokens = Tokenize("`_data_`", new RTokenizer());

            tokens.Should().ContainSingle()
                .Which.Should().HaveType(RTokenType.Identifier)
                .And.StartAt(0)
                .And.HaveLength(8);
        }

        [Test]
        public void TokenizeIdentifierTest02() {
            var tokens = Tokenize("\"odd name\" <- 1", new RTokenizer());

            tokens.Should().HaveCount(3);
            tokens[0].Should().HaveType(RTokenType.String)
                .And.StartAt(0)
                .And.HaveLength(10);
        }

        [Test]
        public void TokenizeIdentifierTest03() {
            var tokens = Tokenize("1 -> \"odd name\"", new RTokenizer());

            tokens.Should().HaveCount(3);
            tokens[2].Should().HaveType(RTokenType.String)
                .And.StartAt(5)
                .And.HaveLength(10);
        }

        [Test]
        public void IdentifierLogical() {
            var tokens = Tokenize("1 <- F(~x)", new RTokenizer());

            tokens.Should().Equal(new[] {
                RTokenType.Number,
                RTokenType.Operator,
                RTokenType.Identifier,
                RTokenType.OpenBrace,
                RTokenType.Operator,
                RTokenType.Identifier,
                RTokenType.CloseBrace,
            }, (token, tokenType) => token.TokenType == tokenType);
        }

        [CompositeTest]
        [InlineData("1 <- F", 2, RTokenType.Logical)]
        [InlineData("F <- 1", 0, RTokenType.Identifier)]
        [InlineData("a -> F", 2, RTokenType.Identifier)]
        [InlineData("F = 1", 0, RTokenType.Identifier)]
        [InlineData("F = 2", 0, RTokenType.Identifier)]
        [InlineData("F =<- 2", 0, RTokenType.Identifier)]
        [InlineData("T = T", 0, RTokenType.Identifier)]
        [InlineData("T = T", 2, RTokenType.Logical)]
        [InlineData("F -> T", 0, RTokenType.Logical)]
        [InlineData("F -> T", 2, RTokenType.Identifier)]
        public void IdentifierLogicals(string input, int tokenIndex, RTokenType tokenType) {
            var tokens = Tokenize(input, new RTokenizer());
            tokens[tokenIndex].TokenType.Should().Be(tokenType);
        }

        [Test]
        public void Tokenize_IdentifiersFile() 
            => TokenizeFiles.TokenizeFile<RToken, RTokenType, RTokenizer>(_files, @"Tokenization\Identifiers.r", "R");
    }
}
