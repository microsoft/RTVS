// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.Languages.Core.Test.Tokens;
using Microsoft.Languages.Core.Test.Utility;
using Microsoft.R.Core.Tokens;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.R.Core.Test.Tokens {
    [ExcludeFromCodeCoverage]
    [Category.R.Tokenizer]
    public class TokenizeIntegersTest : TokenizeTestBase<RToken, RTokenType> {
        private readonly CoreTestFilesFixture _files;

        public TokenizeIntegersTest(CoreTestFilesFixture files) {
            _files = files;
        }

        [Test]
        public void TokenizeIntegers1() {
            var tokens = Tokenize("+1 ", new RTokenizer());

            tokens.Should().ContainSingle()
                .Which.Should().HaveType(RTokenType.Number)
                .And.StartAt(0)
                .And.HaveLength(2);
        }

        [Test]
        public void TokenizeIntegers2() {
            var tokens = this.Tokenize("-12 +1", new RTokenizer());

            tokens.Should().HaveCount(3);

            tokens[0].Should().HaveType(RTokenType.Number)
                .And.StartAt(0)
                .And.HaveLength(3);

            tokens[1].Should().HaveType(RTokenType.Operator)
                .And.StartAt(4)
                .And.HaveLength(1);

            tokens[2].Should().HaveType(RTokenType.Number)
                .And.StartAt(5)
                .And.HaveLength(1);
        }

        [Test]
        public void TokenizeIntegers3() {
            var tokens = Tokenize("-12+-1", new RTokenizer());

            tokens.Should().HaveCount(3);

            tokens[0].Should().HaveType(RTokenType.Number)
                .And.StartAt(0)
                .And.HaveLength(3);

            tokens[1].Should().HaveType(RTokenType.Operator)
                .And.StartAt(3)
                .And.HaveLength(1);

            tokens[2].Should().HaveType(RTokenType.Number)
                .And.StartAt(4)
                .And.HaveLength(2);
        }

        [Test]
        public void TokenizeHex1() {
            var tokens = Tokenize("0x10000", new RTokenizer());

            tokens.Should().HaveCount(1);

            tokens[0].Should().HaveType(RTokenType.Number)
                .And.StartAt(0)
                .And.HaveLength(7);
        }

        [Test]
        public void TokenizeFile_IntegerFile() 
            => TokenizeFiles.TokenizeFile<RToken, RTokenType, RTokenizer>(_files, @"Tokenization\Integers.r", "R");

        [Test]
        public void TokenizeFile_HexFile() 
            => TokenizeFiles.TokenizeFile<RToken, RTokenType, RTokenizer>(_files, @"Tokenization\Hex.r", "R");
    }
}
