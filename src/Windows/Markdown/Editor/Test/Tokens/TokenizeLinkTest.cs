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
    public class TokenizeLinkTest : TokenizeTestBase<MarkdownToken, MarkdownTokenType> {
        [CompositeTest]
        [InlineData(@"[text]()", 0, 6)]
        [Category.Md.Tokenizer]
        public void TokenizeMd_Link(string text, int start, int length) {
            var tokens = Tokenize(text, new MdTokenizer());

            tokens.Should().ContainSingle()
                .Which.Should().HaveType(MarkdownTokenType.AltText)
                .And.StartAt(start)
                .And.HaveLength(length);
        }

        [CompositeTest]
        [InlineData(@"[text] (")]
        [InlineData(@"[text] ()")]
        [Category.Md.Tokenizer]
        public void TokenizeMd_LinkEmpty(string text) {
            var tokens = Tokenize(text, new MdTokenizer());
            tokens.Should().BeEmpty();
        }
    }
}
