// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.Languages.Core.Test.Tokens;
using Microsoft.Markdown.Editor.Tokens;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.Markdown.Editor.Test.Tokens {
    [ExcludeFromCodeCoverage]
    public class TokenizeQuoteTest : TokenizeTestBase<MarkdownToken, MarkdownTokenType> {
        [Test]
        [Category.Md.Tokenizer]
        public void TokenizeMd_Quote01() {
            var tokens = Tokenize(@"> quote", new MdTokenizer());

            tokens.Should().ContainSingle()
                .Which.Should().HaveType(MarkdownTokenType.Blockquote)
                .And.StartAt(0)
                .And.HaveLength(7);
        }

        [Test]
        [Category.Md.Tokenizer]
        public void TokenizeMd_Quote02() {
            var tokens = Tokenize(@">quote", new MdTokenizer());
            tokens.Should().BeEmpty();
        }

        [Test]
        [Category.Md.Tokenizer]
        public void TokenizeMd_Quote03() {
            var tokens = Tokenize(@" > quote", new MdTokenizer());
            tokens.Should().BeEmpty();
        }

        [Test]
        [Category.Md.Tokenizer]
        public void TokenizeMd_Quote04() {
            string content =
@"> quote
  quote

";
            var tokens = Tokenize(content, new MdTokenizer());

            tokens.Should().ContainSingle()
                .Which.Should().HaveType(MarkdownTokenType.Blockquote)
                .And.StartAt(0)
                .And.HaveLength(18);
        }
    }
}
