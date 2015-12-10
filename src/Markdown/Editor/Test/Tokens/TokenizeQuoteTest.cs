using System.Diagnostics.CodeAnalysis;
using Microsoft.Languages.Core.Test.Tokens;
using Microsoft.Markdown.Editor.Tokens;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Markdown.Editor.Test.Tokens {
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class TokenizeQuoteTest : TokenizeTestBase<MarkdownToken, MarkdownTokenType> {
        [TestMethod]
        [TestCategory("Md.Tokenizer")]
        public void TokenizeMd_Quote01() {
            var tokens = this.Tokenize(@"> quote", new MdTokenizer());

            Assert.AreEqual(1, tokens.Count);

            Assert.AreEqual(MarkdownTokenType.Blockquote, tokens[0].TokenType);
            Assert.AreEqual(0, tokens[0].Start);
            Assert.AreEqual(7, tokens[0].Length);
        }

        [TestMethod]
        [TestCategory("Md.Tokenizer")]
        public void TokenizeMd_Quote02() {
            var tokens = this.Tokenize(@">quote", new MdTokenizer());
            Assert.AreEqual(0, tokens.Count);
        }

        [TestMethod]
        [TestCategory("Md.Tokenizer")]
        public void TokenizeMd_Quote03() {
            var tokens = this.Tokenize(@" > quote", new MdTokenizer());
            Assert.AreEqual(0, tokens.Count);
        }

        [TestMethod]
        [TestCategory("Md.Tokenizer")]
        public void TokenizeMd_Quote04() {
            string content =
@"> quote
  quote

";
            var tokens = this.Tokenize(content, new MdTokenizer());

            Assert.AreEqual(1, tokens.Count);

            Assert.AreEqual(MarkdownTokenType.Blockquote, tokens[0].TokenType);
            Assert.AreEqual(0, tokens[0].Start);
            Assert.AreEqual(18, tokens[0].Length);
        }
    }
}
