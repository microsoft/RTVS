// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.Languages.Core.Test.Tokens;
using Microsoft.R.Core.Tokens;
using Microsoft.R.Editor.Roxygen;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.R.Editor.Test.Roxygen {
    [ExcludeFromCodeCoverage]
    [Category.Roxygen]
    public class RoxygenTokenizeTest : TokenizeTestBase<RToken, RTokenType> {
        [Test]
        public void Keywords01() {
            foreach (var k in RoxygenKeywords.Keywords) {
                var tokens = Tokenize($"#' {k} ", new RoxygenTokenizer());
                tokens.Should().ContainSingle();
                tokens[0].TokenType.Should().Be(RTokenType.Keyword);
                tokens[0].Start.Should().Be(3);
                tokens[0].Length.Should().Be(k.Length);
            }
        }

        [Test]
        public void Keywords02() {
            foreach (var k in RoxygenKeywords.Keywords) {
                var tokens = Tokenize($"# {k} ", new RoxygenTokenizer());
                tokens.Should().BeEmpty();
            }
        }

        [Test]
        public void Keywords03() {
            foreach (var k in RoxygenKeywords.Keywords) {
                var tokens = Tokenize($" {k} ", new RoxygenTokenizer());
                tokens.Should().BeEmpty();
            }
        }

        [Test]
        public void Export() {
            var tokens = Tokenize($"#' @export abc ", new RoxygenTokenizer());
            tokens.Should().HaveCount(2);
            tokens[0].TokenType.Should().Be(RTokenType.Keyword);
            tokens[1].TokenType.Should().Be(RTokenType.Identifier);
        }
    }
}
