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
    public class TokenizeFloatsTest : TokenizeTestBase<RToken, RTokenType> {
        private readonly CoreTestFilesFixture _files;

        public TokenizeFloatsTest(CoreTestFilesFixture files) {
            _files = files;
        }

        [CompositeTest]
        [InlineData("+1 ", 0, 2)]
        [InlineData("-.0", 0, 3)]
        [InlineData("0.e1", 0, 4)]
        [InlineData(".0e-2", 0, 5)]
        [InlineData("1e5", 0, 3)]
        [InlineData("-1e+1", 0, 5)]
        [Category.R.Tokenizer]
        public void TokenizeFloats(string text, int start, int length) {
            var tokens = Tokenize(text, new RTokenizer());

            tokens.Should().ContainSingle()
                .Which.Should().HaveType(RTokenType.Number)
                .And.StartAt(start)
                .And.HaveLength(length);
        }

        [CompositeTest]
        [InlineData("+1L ", 0, 3)]
        [InlineData("1e4L", 0, 4)]
        [InlineData("-.0L", 0, 4)]
        [InlineData("0.e1L", 0, 5)]
        [InlineData(".0e-2L", 0, 6)]
        [InlineData("2.4L", 0, 4)]
        [InlineData(".1L", 0, 3)]
        [InlineData("1.L", 0, 3)]
        [Category.R.Tokenizer]
        public void TokenizeLongFloats(string text, int start, int length)
        {
            var tokens = Tokenize(text, new RTokenizer());

            tokens.Should().ContainSingle()
                .Which.Should().HaveType(RTokenType.Number)
                .And.StartAt(start)
                .And.HaveLength(length);
        }

        [Test]
        [Category.R.Tokenizer]
        public void TokenizeFloats05() {
            var tokens = Tokenize("-0.e", new RTokenizer());
            tokens.Should().BeEmpty();
        }

        [Test]
        [Category.R.Tokenizer]
        public void TokenizeFloats06() {
            var tokens = Tokenize("-12.%foo%-.1e", new RTokenizer());

            tokens.Should().HaveCount(2);

            tokens[0].Should().HaveType(RTokenType.Number)
                .And.StartAt(0)
                .And.HaveLength(4);

            tokens[1].Should().HaveType(RTokenType.Operator)
                .And.StartAt(4)
                .And.HaveLength(5);
        }

        [Test]
        [Category.R.Tokenizer]
        public void TokenizeFloats07() {
            var tokens = Tokenize(".1", new RTokenizer());

            tokens.Should().ContainSingle()
                .Which.Should().HaveType(RTokenType.Number)
                .And.StartAt(0)
                .And.HaveLength(2);
        }

        [Test]
        [Category.R.Tokenizer]
        public void TokenizeFloats08() {
            var tokens = Tokenize("1..1", new RTokenizer());

            tokens.Should().HaveCount(2);

            tokens[0].Should().HaveType(RTokenType.Number)
                .And.StartAt(0)
                .And.HaveLength(2);

            tokens[1].Should().HaveType(RTokenType.Number)
                .And.StartAt(2)
                .And.HaveLength(2);
        }

        [Test]
        [Category.R.Tokenizer]
        public void TokenizeFloats09() {
            var tokens = Tokenize("1e", new RTokenizer());
            tokens.Should().BeEmpty();
        }

        [Test]
        [Category.R.Tokenizer]
        public void TokenizeFloats10() {
            var tokens = Tokenize("1.0)", new RTokenizer());
            tokens.Should().HaveCount(2);

            tokens[0].Should().HaveType(RTokenType.Number)
                .And.StartAt(0)
                .And.HaveLength(3);

            tokens[1].Should().HaveType(RTokenType.CloseBrace)
                .And.StartAt(3)
                .And.HaveLength(1);
        }

        [Test]
        [Category.R.Tokenizer]
        public void TokenizeFloats11() {
            var tokens = Tokenize("1e+1+1", new RTokenizer());
            tokens.Should().HaveCount(3);

            tokens[0].Should().HaveType(RTokenType.Number)
                .And.StartAt(0)
                .And.HaveLength(4);

            tokens[1].Should().HaveType(RTokenType.Operator)
                .And.StartAt(4)
                .And.HaveLength(1);

            tokens[2].Should().HaveType(RTokenType.Number)
                 .And.StartAt(5)
                 .And.HaveLength(1);
        }

        [Test]
        [Category.R.Tokenizer]
        public void TokenizeFloats12() {
            var tokens = Tokenize("-1eL", new RTokenizer());
            tokens.Should().HaveCount(1);

            tokens[0].Should().HaveType(RTokenType.Identifier)
                .And.StartAt(3)
                .And.HaveLength(1);
        }

        [Test]
        [Category.R.Tokenizer]
        public void TokenizeFile_FloatsFile() {
            TokenizeFiles.TokenizeFile<RToken, RTokenType, RTokenizer>(_files, @"Tokenization\Floats.r", "R");
        }
    }
}
