// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Linq;
using FluentAssertions;
using Microsoft.Languages.Core.Braces;
using Microsoft.UnitTests.Core.XUnit;
using Xunit;
using static Microsoft.Languages.Core.Braces.BraceTokenType;

namespace Microsoft.Languages.Core.Test.Braces {
    [ExcludeFromCodeCoverage]
    [Category.Languages.Core]
    public sealed class BraceTokenizerTest {
        [CompositeTest]
        [InlineData("", new BraceTokenType[0])]
        [InlineData("func()[x]{}", new[] { OpenBrace, CloseBrace, OpenBracket, CloseBracket, OpenCurly, CloseCurly })]
        [InlineData("([3 {\n ]})1]* ", new[] { OpenBrace, OpenBracket, OpenCurly, CloseBracket, CloseCurly, CloseBrace, CloseBracket })]
        public void Tokenize(string content, BraceTokenType[] expected) {
            var tokenizer = new BraceTokenizer();
            var actual = tokenizer.Tokenize(content);

            actual.Should().HaveSameCount(expected);
            actual.Select(t => t.TokenType).Should().ContainInOrder(expected);
        }
    }
}
