// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.Languages.Core.Test.Tokens;
using Microsoft.R.Core.Tokens;
using Microsoft.UnitTests.Core.XUnit;
using Xunit;

namespace Microsoft.R.Core.Test.Tokens {
    [ExcludeFromCodeCoverage]   
    public class TokenizeComplexTest : TokenizeTestBase<RToken, RTokenType> {
        [CompositeTest]
        [InlineData("+1i", 0, 3)]
        [InlineData("-.0+1i", 0, 6)]
        [InlineData("0.e1-+1i", 0, 8)]
        [InlineData(".0e-5+-1.e23i", 0, 13)]
        [InlineData("-0.e2i", 0, 6)]
        [InlineData("1i", 0, 2)]
        [InlineData("1e4L+1i", 0, 7)]
        [InlineData("1L+1i", 0, 5)]
        [Category.R.Tokenizer]
        public void TokenizeComplex(string text, int start, int length) {
            var tokens = Tokenize(text, new RTokenizer());

            tokens.Should().ContainSingle()
                .Which.Should().HaveType(RTokenType.Complex)
                .And.StartAt(start)
                .And.HaveLength(length);
        }

        [CompositeTest]
        [InlineData("0xAi", 0, 4)]
        [Category.R.Tokenizer]
        public void TokenizeHexComplex(string text, int start, int length) {
            var tokens = Tokenize(text, new RTokenizer());

            tokens.Should().ContainSingle()
                .Which.Should().HaveType(RTokenType.Complex)
                .And.StartAt(start)
                .And.HaveLength(length);
        }
    }
}
