// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.Languages.Core.Classification;
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
")]
        [Category.Md.Tokenizer]
        public void CodeBlock02(string text) {
            var tokens = Tokenize(text, new MdTokenizer());
            tokens.Should().HaveCount(1);
            tokens[0].Should().HaveType(MarkdownTokenType.Code);
            tokens[0].Length.Should().Be(21);
        }

        [CompositeTest]
        [InlineData(@"```block```")]
        [InlineData(@"```block")]
        [InlineData(@"```block` ```")]
        [Category.Md.Tokenizer]
        public void TokenizeMd_BlockEmpty(string text) {
            var tokens = Tokenize(text, new MdTokenizer());
            tokens.Should().BeEmpty();
        }

        [CompositeTest]
        [Category.Md.Tokenizer]
        [InlineData(@"`r x <- 1`")]
        [InlineData(@"`rtoken`")]
        public void CodeBlock03(string content) {
            var tokens = Tokenize(@"`r x <- 1`", new MdTokenizer());
            tokens.Should().HaveCount(1);
            tokens[0].Should().HaveType(MarkdownTokenType.Code);
        }
    }
}
