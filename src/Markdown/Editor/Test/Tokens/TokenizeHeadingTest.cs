using Microsoft.Languages.Core.Tests.Tokens;
using Microsoft.Markdown.Editor.Tokens;
using Microsoft.UnitTests.Core.XUnit;
using Xunit;

namespace Microsoft.Markdown.Editor.Tests.Tokens {
    public class TokenizeHeadingTest : TokenizeTestBase<MarkdownToken, MarkdownTokenType> {
        [Fact]
        [Trait("Category","Md.Tokenizer")]
        public void TokenizeMd_Heading01() {
            var tokens = this.Tokenize(@"---", new MdTokenizer());

            Assert.Equal(1, tokens.Count);

            Assert.Equal(MarkdownTokenType.Heading, tokens[0].TokenType);
            Assert.Equal(0, tokens[0].Start);
            Assert.Equal(3, tokens[0].Length);
        }

        [Fact]
        [Trait("Category","Md.Tokenizer")]
        public void TokenizeMd_Heading02() {
            var tokens = this.Tokenize(@"----", new MdTokenizer());

            Assert.Equal(1, tokens.Count);

            Assert.Equal(MarkdownTokenType.Heading, tokens[0].TokenType);
            Assert.Equal(0, tokens[0].Start);
            Assert.Equal(4, tokens[0].Length);
        }

        [Fact]
        [Trait("Category","Md.Tokenizer")]
        public void TokenizeMd_Heading03() {
            var tokens = this.Tokenize(@"===", new MdTokenizer());

            Assert.Equal(1, tokens.Count);

            Assert.Equal(MarkdownTokenType.Heading, tokens[0].TokenType);
            Assert.Equal(0, tokens[0].Start);
            Assert.Equal(3, tokens[0].Length);
        }

        [Fact]
        [Trait("Category","Md.Tokenizer")]
        public void TokenizeMd_Heading04() {
            var tokens = this.Tokenize(@"====", new MdTokenizer());

            Assert.Equal(1, tokens.Count);

            Assert.Equal(MarkdownTokenType.Heading, tokens[0].TokenType);
            Assert.Equal(0, tokens[0].Start);
            Assert.Equal(4, tokens[0].Length);
        }

        [Fact]
        [Trait("Category","Md.Tokenizer")]
        public void TokenizeMd_Heading05() {
            var tokens = this.Tokenize(@"#", new MdTokenizer());

            Assert.Equal(1, tokens.Count);

            Assert.Equal(MarkdownTokenType.Heading, tokens[0].TokenType);
            Assert.Equal(0, tokens[0].Start);
            Assert.Equal(1, tokens[0].Length);
        }

        [Fact]
        [Trait("Category","Md.Tokenizer")]
        public void TokenizeMd_Heading06() {
            var tokens = this.Tokenize(@"##", new MdTokenizer());

            Assert.Equal(1, tokens.Count);

            Assert.Equal(MarkdownTokenType.Heading, tokens[0].TokenType);
            Assert.Equal(0, tokens[0].Start);
            Assert.Equal(2, tokens[0].Length);
        }

        [Fact]
        [Trait("Category","Md.Tokenizer")]
        public void TokenizeMd_Heading07() {
            var tokens = this.Tokenize(@"--", new MdTokenizer());
            Assert.Equal(0, tokens.Count);
        }

        [Fact]
        [Trait("Category","Md.Tokenizer")]
        public void TokenizeMd_Heading08() {
            var tokens = this.Tokenize(@"==", new MdTokenizer());
            Assert.Equal(0, tokens.Count);
        }

        [Fact]
        [Trait("Category","Md.Tokenizer")]
        public void TokenizeMd_Heading09() {
            var tokens = this.Tokenize(@" ---", new MdTokenizer());
            Assert.Equal(0, tokens.Count);
        }

        [Fact]
        [Trait("Category","Md.Tokenizer")]
        public void TokenizeMd_Heading10() {
            var tokens = this.Tokenize(@" ===", new MdTokenizer());
            Assert.Equal(0, tokens.Count);
        }

        [Fact]
        [Trait("Category","Md.Tokenizer")]
        public void TokenizeMd_Heading11() {
            var tokens = this.Tokenize(@" #", new MdTokenizer());
            Assert.Equal(0, tokens.Count);
        }
    }
}
