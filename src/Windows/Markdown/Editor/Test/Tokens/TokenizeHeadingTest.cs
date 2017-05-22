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
    public class TokenizeHeadingTest : TokenizeTestBase<MarkdownToken, MarkdownTokenType> {
        [CompositeTest]
        [InlineData(@"---", 0, 3)]
        [InlineData(@"----", 0, 4)]
        [InlineData(@"===", 0, 3)]
        [InlineData(@"====", 0, 4)]
        [InlineData(@"#", 0, 1)]
        [InlineData(@"##", 0, 2)]
        [Category.Md.Tokenizer]
        public void TokenizeMd_Heading(string text, int start, int length) {
            var tokens = Tokenize(text, new MdTokenizer());

            tokens.Should().ContainSingle()
                .Which.Should().HaveType(MarkdownTokenType.Heading)
                .And.StartAt(start)
                .And.HaveLength(length);
        }

        [CompositeTest]
        [InlineData(@"--")]
        [InlineData(@"==")]
        [InlineData(@" ---")]
        [InlineData(@" ===")]
        [InlineData(@" #")]
        [Category.Md.Tokenizer]
        public void TokenizeMd_HeadingEmpty(string text) {
            var tokens = Tokenize(text, new MdTokenizer());
            tokens.Should().BeEmpty();
        }
    }
}
