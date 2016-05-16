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
")]
        [InlineData(
@"```
block```
 ```
block
```
")]
        [Category.Md.Tokenizer]
        public void CodeBlock01(string text) {
            //var tokens = Tokenize(text, new MdTokenizer());

            //tokens.Should().HaveCount(3);
            //tokens[0].Should().HaveType(MarkdownTokenType.CodeStart);
            //tokens[0].Length.Should().Be(3);
            //tokens[1].Should().HaveType(MarkdownTokenType.CodeContent);
            //tokens[2].Should().HaveType(MarkdownTokenType.CodeEnd);
            //tokens[2].Length.Should().Be(3);
        }

        [CompositeTest]
        [InlineData(
@"```{r}
#comment
```
")]
        [Category.Md.Tokenizer]
        public void CodeBlock02(string text) {
            //var tokens = Tokenize(text, new MdTokenizer());

            //tokens.Should().HaveCount(3);
            //tokens[0].Should().HaveType(MarkdownTokenType.CodeStart);
            //tokens[0].Length.Should().Be(3);
            //tokens[1].Should().HaveType(MarkdownTokenType.CodeContent);
            //tokens[1].Should().BeOfType<MarkdownRCodeToken>()
            //    .And.BeAssignableTo<ICompositeToken>();
            //tokens[2].Should().HaveType(MarkdownTokenType.CodeEnd);
            //tokens[2].Length.Should().Be(3);
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

        [Test]
        [Category.Md.Tokenizer]
        public void CodeBlock03() {
            //var tokens = Tokenize(@"`r x <- 1`", new MdTokenizer());

            //tokens.Should().HaveCount(3);
            //tokens[0].Should().HaveType(MarkdownTokenType.CodeStart);
            //tokens[1].Should().BeOfType<MarkdownRCodeToken>()
            //    .And.BeAssignableTo<ICompositeToken>()
            //    .Which.TokenList.Should().HaveCount(3);
            //tokens[2].Should().HaveType(MarkdownTokenType.CodeEnd);
        }

        [Test]
        [Category.Md.Tokenizer]
        public void CodeBlock04() {
            var tokens = Tokenize(@"`rtoken`", new MdTokenizer());

            tokens.Should().HaveCount(1);
            tokens[0].Should().HaveType(MarkdownTokenType.Monospace);
        }
    }
}
