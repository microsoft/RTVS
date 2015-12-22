using Microsoft.Languages.Core.Tests.Tokens;
using Microsoft.Markdown.Editor.Tokens;
using Microsoft.UnitTests.Core.XUnit;
using Xunit;

namespace Microsoft.Markdown.Editor.Tests.Tokens {
    public class TokenizeListTest : TokenizeTestBase<MarkdownToken, MarkdownTokenType> {
        [Fact]
        [Trait("Category","Md.Tokenizer")]
        public void TokenizeMd_List01() {
            var tokens = this.Tokenize(@"- item", new MdTokenizer());

            Assert.Equal(1, tokens.Count);

            Assert.Equal(MarkdownTokenType.ListItem, tokens[0].TokenType);
            Assert.Equal(0, tokens[0].Start);
            Assert.Equal(6, tokens[0].Length);
        }

        [Fact]
        [Trait("Category","Md.Tokenizer")]
        public void TokenizeMd_List02() {
            var tokens = this.Tokenize(@"* item", new MdTokenizer());

            Assert.Equal(1, tokens.Count);

            Assert.Equal(MarkdownTokenType.ListItem, tokens[0].TokenType);
            Assert.Equal(0, tokens[0].Start);
            Assert.Equal(6, tokens[0].Length);
        }

        [Fact]
        [Trait("Category","Md.Tokenizer")]
        public void TokenizeMd_List03() {
            var tokens = this.Tokenize(@"12. item", new MdTokenizer());

            Assert.Equal(1, tokens.Count);

            Assert.Equal(MarkdownTokenType.ListItem, tokens[0].TokenType);
            Assert.Equal(0, tokens[0].Start);
            Assert.Equal(8, tokens[0].Length);
        }

        [Fact]
        [Trait("Category","Md.Tokenizer")]
        public void TokenizeMd_List04() {
            var tokens = this.Tokenize(@"-item", new MdTokenizer());
            Assert.Equal(0, tokens.Count);
        }

        [Fact]
        [Trait("Category","Md.Tokenizer")]
        public void TokenizeMd_List05() {
            var tokens = this.Tokenize(@"*item", new MdTokenizer());
            Assert.Equal(0, tokens.Count);
        }

        [Fact]
        [Trait("Category","Md.Tokenizer")]
        public void TokenizeMd_List06() {
            var tokens = this.Tokenize(@"1.item", new MdTokenizer());
            Assert.Equal(0, tokens.Count);
        }
    }
}
