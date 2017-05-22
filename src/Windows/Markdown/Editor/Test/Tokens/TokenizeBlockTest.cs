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
    public class TokenizeMdBlockTest : TokenizeTestBase<MarkdownToken, MarkdownTokenType> {
        [CompositeTest]
        [InlineData(
@"```
block

block
```
", 24)]
        [InlineData(
@"```
block```
 ```
block
```
", 31)]
        [Category.Md.Tokenizer]
        public void CodeBlock01(string text, int length) {
            var tokens = Tokenize(text, new MdTokenizer());
            tokens.Should().HaveCount(1);
            tokens[0].Should().HaveType(MarkdownTokenType.Monospace);
            tokens[0].Length.Should().Be(length);
        }

        [CompositeTest]
        [InlineData(
@"```{r}
#comment
```
", 21)]
        [InlineData(
@"```{r}

", 10)]
        [Category.Md.Tokenizer]
        public void CodeBlock02(string text, int length) {
            var tokens = Tokenize(text, new MdTokenizer());
            tokens.Should().HaveCount(1);
            tokens[0].Should().HaveType(MarkdownTokenType.Code);
            tokens[0].Should().BeOfType(typeof(MarkdownCodeToken));
            tokens[0].Length.Should().Be(length);
        }

        [CompositeTest]
        [Category.Md.Tokenizer]
        [InlineData(@"`r x <- 1`", 2, 1)]
        [InlineData(@"`rtoken`", 0, 0)]
        public void CodeBlock03(string content, int leadingSeparatorLength, int trailingSeparatorLength) {
            var tokens = Tokenize(@"`r x <- 1`", new MdTokenizer());
            tokens.Should().HaveCount(1);
            tokens[0].Should().HaveType(MarkdownTokenType.Code);
            tokens[0].Should().BeOfType(typeof(MarkdownCodeToken));

            if (leadingSeparatorLength > 0) {
                var mdct = tokens[0] as MarkdownCodeToken;
                mdct.LeadingSeparatorLength.Should().Be(leadingSeparatorLength);
                mdct.TrailingSeparatorLength.Should().Be(trailingSeparatorLength);
            }
        }

        [CompositeTest]
        [InlineData("```block```", 11)]
        [InlineData("```block", 8)]
        [InlineData("```block\n", 9)]
        [InlineData("```block\r", 9)]
        [InlineData("```block\r\n", 10)]
        [InlineData("```block` ```", 13)]
        [Category.Md.Tokenizer]
        public void CodeBlock04(string text, int length) {
            var tokens = Tokenize(text, new MdTokenizer());
            tokens.Should().HaveCount(1);
            tokens[0].Length.Should().Be(length);

            var mdct = tokens[0] as MarkdownCodeToken;
            mdct.Should().BeNull();
        }
    }
}
