// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Linq;
using FluentAssertions;
using Microsoft.Languages.Core.Braces;
using Microsoft.UnitTests.Core.XUnit;
using Xunit;

namespace Microsoft.Languages.Core.Test.Braces {
    [ExcludeFromCodeCoverage]
    [Category.Languages.Core]
    public sealed class BraceTokenizerTest {
        [CompositeTest]
        [InlineData("", new BraceTokenType[0])]
        [InlineData("func()[x]{}", new[] { BraceTokenType.OpenBrace, BraceTokenType.CloseBrace, BraceTokenType.OpenBracket, BraceTokenType.CloseBracket, BraceTokenType.OpenCurly, BraceTokenType.CloseCurly })]
        [InlineData("([3 {\n ]})1]* ", new[] { BraceTokenType.OpenBrace, BraceTokenType.OpenBracket, BraceTokenType.OpenCurly, BraceTokenType.CloseBracket, BraceTokenType.CloseCurly, BraceTokenType.CloseBrace, BraceTokenType.CloseBracket })]
        public void Tokenize(string content, BraceTokenType[] expected) {
            var tokenizer = new BraceTokenizer();
            var actual = tokenizer.Tokenize(content);

            actual.Should().HaveSameCount(expected);
            actual.Select(t => t.TokenType).Should().ContainInOrder(expected);
        }
    }
}
