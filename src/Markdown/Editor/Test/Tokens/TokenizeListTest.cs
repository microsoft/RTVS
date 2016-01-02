using System.Diagnostics.CodeAnalysis;
using Microsoft.Languages.Core.Test.Tokens;
using Microsoft.Markdown.Editor.Tokens;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Markdown.Editor.Test.Tokens {
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class TokenizeListTest : TokenizeTestBase<MarkdownToken, MarkdownTokenType> {
        [TestMethod]
        [TestCategory("Md.Tokenizer")]
        public void TokenizeMd_List01() {
            var tokens = this.Tokenize(@"- item", new MdTokenizer());

            Assert.AreEqual(1, tokens.Count);

            Assert.AreEqual(MarkdownTokenType.ListItem, tokens[0].TokenType);
            Assert.AreEqual(0, tokens[0].Start);
            Assert.AreEqual(6, tokens[0].Length);
        }

        [TestMethod]
        [TestCategory("Md.Tokenizer")]
        public void TokenizeMd_List02() {
            var tokens = this.Tokenize(@"* item", new MdTokenizer());

            Assert.AreEqual(1, tokens.Count);

            Assert.AreEqual(MarkdownTokenType.ListItem, tokens[0].TokenType);
            Assert.AreEqual(0, tokens[0].Start);
            Assert.AreEqual(6, tokens[0].Length);
        }

        [TestMethod]
        [TestCategory("Md.Tokenizer")]
        public void TokenizeMd_List03() {
            var tokens = this.Tokenize(@"12. item", new MdTokenizer());

            Assert.AreEqual(1, tokens.Count);

            Assert.AreEqual(MarkdownTokenType.ListItem, tokens[0].TokenType);
            Assert.AreEqual(0, tokens[0].Start);
            Assert.AreEqual(8, tokens[0].Length);
        }

        [TestMethod]
        [TestCategory("Md.Tokenizer")]
        public void TokenizeMd_List04() {
            var tokens = this.Tokenize(@"-item", new MdTokenizer());
            Assert.AreEqual(0, tokens.Count);
        }

        [TestMethod]
        [TestCategory("Md.Tokenizer")]
        public void TokenizeMd_List05() {
            var tokens = this.Tokenize(@"*item", new MdTokenizer());
            Assert.AreEqual(0, tokens.Count);
        }

        [TestMethod]
        [TestCategory("Md.Tokenizer")]
        public void TokenizeMd_List06() {
            var tokens = this.Tokenize(@"1.item", new MdTokenizer());
            Assert.AreEqual(0, tokens.Count);
        }
    }
}
