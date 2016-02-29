// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.Languages.Core.Test.Tokens;
using Microsoft.R.Core.Tokens;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.R.Core.Test.Tokens {
    [ExcludeFromCodeCoverage]
    public class TokenizeFunctionsTest : TokenizeTestBase<RToken, RTokenType> {
        private readonly CoreTestFilesFixture _files;

        public TokenizeFunctionsTest(CoreTestFilesFixture files) {
            _files = files;
        }

        [Test]
        [Category.R.Tokenizer]
        public void TokenizeFunctionsTest1() {
            var tokens = Tokenize("x <- function( ", new RTokenizer());

            tokens.Should().HaveCount(4);

            tokens[0].Should().HaveType(RTokenType.Identifier)
                .And.StartAt(0)
                .And.HaveLength(1);

            tokens[1].Should().HaveType(RTokenType.Operator)
                .And.StartAt(2)
                .And.HaveLength(2);

            tokens[2].Should().HaveType(RTokenType.Keyword)
                .And.StartAt(5)
                .And.HaveLength(8);

            tokens[3].Should().HaveType(RTokenType.OpenBrace)
                .And.StartAt(13)
                .And.HaveLength(1);
        }

        [Test]
        [Category.R.Tokenizer]
        public void TokenizeFile_FunctionsFile() {
            TokenizeFiles.TokenizeFile(_files, @"Tokenization\Functions.r");
        }
    }
}
