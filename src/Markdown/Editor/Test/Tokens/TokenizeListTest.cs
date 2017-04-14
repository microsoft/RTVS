// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.Languages.Core.Test.Tokens;
using Microsoft.Markdown.Editor.Tokens;
using Microsoft.UnitTests.Core.XUnit;
using Xunit;

namespace Microsoft.Markdown.Editor.Test.Tokens {
    [ExcludeFromCodeCoverage]
    public class TokenizeListTest : TokenizeTestBase<MarkdownToken, MarkdownTokenType> {
        [CompositeTest]
        [InlineData(@"- item", 0, 6)]
        [InlineData(@"* item", 0, 6)]
        [InlineData(@"12. item", 0, 8)]
        [Category.Md.Tokenizer]
        public void TokenizeMd_List(string text, int start, int length) {
            var tokens = Tokenize(text, new MdTokenizer());

            tokens.Should().ContainSingle()
                .Which.Should().HaveType(MarkdownTokenType.ListItem)
                .And.StartAt(start)
                .And.HaveLength(length);
        }

        [CompositeTest]
        [InlineData(@"-item")]
        [InlineData(@"*item")]
        [InlineData(@"1.item")]
        [InlineData(@"a - item")]
        [InlineData(@"b * item")]
        [InlineData(@"z 1. 12. item")]
        [Category.Md.Tokenizer]
        public void TokenizeMd_EmptyList(string text) {
            var tokens = Tokenize(text, new MdTokenizer());
            tokens.Should().BeEmpty();
        }
    }
}
