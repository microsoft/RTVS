// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Text;
using FluentAssertions;
using Microsoft.Languages.Core.Test.Tokens;
using Microsoft.R.Core.Tokens;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.R.Core.Test.Tokens {
    [ExcludeFromCodeCoverage]
    public class TokenizeOperatorsTest : TokenizeTestBase<RToken, RTokenType> {
        [Test]
        [Category.R.Tokenizer]
        public void Tokenize_OneCharOperatorsTest() {
            var tokens = Tokenize("^~-+*/$@<>|&=!?:", new RTokenizer());

            tokens.Should().HaveCount(16);
            for (var i = 0; i < tokens.Count; i++) {
                tokens[i].Should().HaveType(RTokenType.Operator)
                    .And.StartAt(i)
                    .And.HaveLength(1);
            }
        }

        [Test]
        [Category.R.Tokenizer]
        public void Tokenize_TwoCharOperatorsTest() {
            StringBuilder sb = new StringBuilder();
            foreach (var t in Operators._twoChars) {
                sb.Append(t);
                sb.Append(' ');
            }

            var tokens = Tokenize(sb.ToString(), new RTokenizer());

            tokens.Should().HaveCount(Operators._twoChars.Length);

            for (var i = 0; i < tokens.Count; i++) {
                tokens[i].Should().HaveType(RTokenType.Operator)
                    .And.StartAt(3 * i)
                    .And.HaveLength(2);
            }
        }

        [Test]
        [Category.R.Tokenizer]
        public void Tokenize_ThreeCharOperatorsTest() {
            StringBuilder sb = new StringBuilder();
            foreach (var token in Operators._threeChars) {
                sb.Append(token);
                sb.Append(' ');
            }

            var tokens = Tokenize(sb.ToString(), new RTokenizer());

            tokens.Should().HaveCount(Operators._threeChars.Length);

            for (var i = 0; i < tokens.Count; i++) {
                tokens[i].Should().HaveType(RTokenType.Operator)
                    .And.StartAt(4 * i)
                    .And.HaveLength(3);
            }
        }

        [Test]
        [Category.R.Tokenizer]
        public void Tokenize_CustomOperatorsTest01() {
            var tokens = Tokenize("%foo% %русский%", new RTokenizer());

            tokens.Should().HaveCount(2);

            tokens[0].Should().HaveType(RTokenType.Operator)
                .And.StartAt(0)
                .And.HaveLength(5);

            tokens[1].Should().HaveType(RTokenType.Operator)
                .And.StartAt(6)
                .And.HaveLength(9);
        }

        [Test]
        [Category.R.Tokenizer]
        public void Tokenize_CustomOperatorsTest02() {
            var tokens = Tokenize("%<% %?=?%", new RTokenizer());

            tokens.Should().HaveCount(2);

            tokens[0].Should().HaveType(RTokenType.Operator)
                .And.StartAt(0)
                .And.HaveLength(3);

            tokens[1].Should().HaveType(RTokenType.Operator)
                .And.StartAt(4)
                .And.HaveLength(5);
        }
    }
}
