using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.Languages.Core.Classification;
using Microsoft.Languages.Core.Test.Tokens;
using Microsoft.Markdown.Editor.Tokens;
using Microsoft.UnitTests.Core.XUnit;
using Xunit;

namespace Microsoft.Markdown.Editor.Test.Tokens {
    [ExcludeFromCodeCoverage]
    public class TokenizeBlockTest : TokenizeTestBase<MarkdownToken, MarkdownTokenType> {
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
        public void TokenizeMd_Block(string text) {
            var tokens = Tokenize(text, new MdTokenizer());

            tokens.Should().ContainSingle()
                .Which.Should().HaveType(MarkdownTokenType.Code)
                .And.StartAt(0)
                .And.HaveLength(text.Length - 2);
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
        public void TokenizeMd_Block06() {
            var tokens = Tokenize(@"`r x <- 1`", new MdTokenizer());

            tokens.Should().HaveCount(3);
            tokens[0].Should().HaveType(MarkdownTokenType.Code);
            tokens[1].Should().BeOfType<MarkdownRCodeToken>()
                .And.BeAssignableTo<ICompositeToken>()
                .Which.TokenList.Should().HaveCount(3);

            tokens[2].Should().HaveType(MarkdownTokenType.Code);
        }
    }
}
