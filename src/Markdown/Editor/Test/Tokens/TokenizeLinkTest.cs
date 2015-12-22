using Microsoft.Languages.Core.Tests.Tokens;
using Microsoft.Markdown.Editor.Tokens;
using Microsoft.UnitTests.Core.XUnit;
using Xunit;

namespace Microsoft.Markdown.Editor.Tests.Tokens {
    public class TokenizeLinkTest : TokenizeTestBase<MarkdownToken, MarkdownTokenType> {
        [Fact]
        [Trait("Md.Tokenizer", "")]
        public void TokenizeMd_Link01() {
            var tokens = this.Tokenize(@"[text]()", new MdTokenizer());

            Assert.Equal(1, tokens.Count);

            Assert.Equal(MarkdownTokenType.AltText, tokens[0].TokenType);
            Assert.Equal(0, tokens[0].Start);
            Assert.Equal(6, tokens[0].Length);
        }


        [Fact]
        [Trait("Md.Tokenizer", "")]
        public void TokenizeMd_Link02() {
            var tokens = this.Tokenize(@"[text] (", new MdTokenizer());
            Assert.Equal(0, tokens.Count);
        }

        [Fact]
        [Trait("Md.Tokenizer", "")]
        public void TokenizeMd_Link03() {
            var tokens = this.Tokenize(@"[text] ()", new MdTokenizer());
            Assert.Equal(0, tokens.Count);
        }
    }
}
