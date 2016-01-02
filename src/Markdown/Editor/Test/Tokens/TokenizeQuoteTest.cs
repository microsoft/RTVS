using Microsoft.Languages.Core.Tests.Tokens;
using Microsoft.Markdown.Editor.Tokens;
using Microsoft.UnitTests.Core.XUnit;
using Xunit;

namespace Microsoft.Markdown.Editor.Tests.Tokens {
    public class TokenizeQuoteTest : TokenizeTestBase<MarkdownToken, MarkdownTokenType> {
        [Fact]
        [Trait("Category","Md.Tokenizer")]
        public void TokenizeMd_Quote01() {
            var tokens = this.Tokenize(@"> quote", new MdTokenizer());

            Assert.Equal(1, tokens.Count);

            Assert.Equal(MarkdownTokenType.Blockquote, tokens[0].TokenType);
            Assert.Equal(0, tokens[0].Start);
            Assert.Equal(7, tokens[0].Length);
        }

        [Fact]
        [Trait("Category","Md.Tokenizer")]
        public void TokenizeMd_Quote02() {
            var tokens = this.Tokenize(@">quote", new MdTokenizer());
            Assert.Equal(0, tokens.Count);
        }

        [Fact]
        [Trait("Category","Md.Tokenizer")]
        public void TokenizeMd_Quote03() {
            var tokens = this.Tokenize(@" > quote", new MdTokenizer());
            Assert.Equal(0, tokens.Count);
        }

        [Fact]
        [Trait("Category","Md.Tokenizer")]
        public void TokenizeMd_Quote04() {
            string content =
@"> quote
  quote

";
            var tokens = this.Tokenize(content, new MdTokenizer());

            Assert.Equal(1, tokens.Count);

            Assert.Equal(MarkdownTokenType.Blockquote, tokens[0].TokenType);
            Assert.Equal(0, tokens[0].Start);
            Assert.Equal(18, tokens[0].Length);
        }
    }
}
