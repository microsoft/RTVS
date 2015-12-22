using Microsoft.Languages.Core.Classification;
using Microsoft.Languages.Core.Tests.Tokens;
using Microsoft.Markdown.Editor.Tokens;
using Microsoft.UnitTests.Core.XUnit;
using Xunit;

namespace Microsoft.Markdown.Editor.Tests.Tokens {
    public class TokenizeBlockTest : TokenizeTestBase<MarkdownToken, MarkdownTokenType> {
        [Fact]
        [Trait("Md.Tokenizer", "")]
        public void TokenizeMd_Block01() {
            var tokens = this.Tokenize(@"```block```", new MdTokenizer());
            Assert.Equal(0, tokens.Count);
        }

        [Fact]
        [Trait("Md.Tokenizer", "")]
        public void TokenizeMd_Block02() {
            string content =
@"```
block

block
```
";
            var tokens = this.Tokenize(content, new MdTokenizer());

            Assert.Equal(1, tokens.Count);

            Assert.Equal(MarkdownTokenType.Code, tokens[0].TokenType);
            Assert.Equal(0, tokens[0].Start);
            Assert.Equal(content.Length - 2, tokens[0].Length);
        }

        [Fact]
        [Trait("Md.Tokenizer", "")]
        public void TokenizeMd_Block03() {
            var tokens = this.Tokenize(@"```block", new MdTokenizer());
            Assert.Equal(0, tokens.Count);
        }

        [Fact]
        [Trait("Md.Tokenizer", "")]
        public void TokenizeMd_Block04() {
            var tokens = this.Tokenize(@"```block` ```", new MdTokenizer());
            Assert.Equal(0, tokens.Count);
        }

        [Fact]
        [Trait("Md.Tokenizer", "")]
        public void TokenizeMd_Block05() {
            string content =
@"```
block```
 ```
block
```
";
            var tokens = this.Tokenize(content, new MdTokenizer());
            Assert.Equal(1, tokens.Count);

            Assert.Equal(MarkdownTokenType.Code, tokens[0].TokenType);
            Assert.Equal(0, tokens[0].Start);
            Assert.Equal(content.Length - 2, tokens[0].Length);
        }

        [Fact]
        [Trait("Md.Tokenizer", "")]
        public void TokenizeMd_Block06() {
            var tokens = this.Tokenize(@"`r x <- 1`", new MdTokenizer());
            Assert.Equal(3, tokens.Count);
            Assert.Equal(MarkdownTokenType.Code, tokens[0].TokenType);
            Assert.True(tokens[1] is MarkdownRCodeToken);
            Assert.Equal(MarkdownTokenType.Code, tokens[2].TokenType);

            ICompositeToken composite = tokens[1] as ICompositeToken;
            Assert.Equal(3, composite.TokenList.Count);
        }
    }
}
