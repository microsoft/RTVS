using System.Diagnostics.CodeAnalysis;
using Microsoft.Languages.Core.Test.Tokens;
using Microsoft.Markdown.Editor.Tokens;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Markdown.Editor.Test.Tokens
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class TokenizeHeadingTest : TokenizeTestBase<MarkdownToken, MarkdownTokenType>
    {
        [TestMethod]
        public void TokenizeMd_Heading01()
        {
            var tokens = this.Tokenize(@"---", new MdTokenizer());

            Assert.AreEqual(1, tokens.Count);

            Assert.AreEqual(MarkdownTokenType.Heading, tokens[0].TokenType);
            Assert.AreEqual(0, tokens[0].Start);
            Assert.AreEqual(3, tokens[0].Length);
        }

        [TestMethod]
        public void TokenizeMd_Heading02()
        {
            var tokens = this.Tokenize(@"----", new MdTokenizer());

            Assert.AreEqual(1, tokens.Count);

            Assert.AreEqual(MarkdownTokenType.Heading, tokens[0].TokenType);
            Assert.AreEqual(0, tokens[0].Start);
            Assert.AreEqual(4, tokens[0].Length);
        }

        [TestMethod]
        public void TokenizeMd_Heading03()
        {
            var tokens = this.Tokenize(@"===", new MdTokenizer());

            Assert.AreEqual(1, tokens.Count);

            Assert.AreEqual(MarkdownTokenType.Heading, tokens[0].TokenType);
            Assert.AreEqual(0, tokens[0].Start);
            Assert.AreEqual(3, tokens[0].Length);
        }

        [TestMethod]
        public void TokenizeMd_Heading04()
        {
            var tokens = this.Tokenize(@"====", new MdTokenizer());

            Assert.AreEqual(1, tokens.Count);

            Assert.AreEqual(MarkdownTokenType.Heading, tokens[0].TokenType);
            Assert.AreEqual(0, tokens[0].Start);
            Assert.AreEqual(4, tokens[0].Length);
        }

        [TestMethod]
        public void TokenizeMd_Heading05()
        {
            var tokens = this.Tokenize(@"#", new MdTokenizer());

            Assert.AreEqual(1, tokens.Count);

            Assert.AreEqual(MarkdownTokenType.Heading, tokens[0].TokenType);
            Assert.AreEqual(0, tokens[0].Start);
            Assert.AreEqual(1, tokens[0].Length);
        }

        [TestMethod]
        public void TokenizeMd_Heading06()
        {
            var tokens = this.Tokenize(@"##", new MdTokenizer());

            Assert.AreEqual(1, tokens.Count);

            Assert.AreEqual(MarkdownTokenType.Heading, tokens[0].TokenType);
            Assert.AreEqual(0, tokens[0].Start);
            Assert.AreEqual(2, tokens[0].Length);
        }

        [TestMethod]
        public void TokenizeMd_Heading07()
        {
            var tokens = this.Tokenize(@"--", new MdTokenizer());
            Assert.AreEqual(0, tokens.Count);
        }

        [TestMethod]
        public void TokenizeMd_Heading08()
        {
            var tokens = this.Tokenize(@"==", new MdTokenizer());
            Assert.AreEqual(0, tokens.Count);
        }

        [TestMethod]
        public void TokenizeMd_Heading09()
        {
            var tokens = this.Tokenize(@" ---", new MdTokenizer());
            Assert.AreEqual(0, tokens.Count);
        }

        [TestMethod]
        public void TokenizeMd_Heading10()
        {
            var tokens = this.Tokenize(@" ===", new MdTokenizer());
            Assert.AreEqual(0, tokens.Count);
        }

        [TestMethod]
        public void TokenizeMd_Heading11()
        {
            var tokens = this.Tokenize(@" #", new MdTokenizer());
            Assert.AreEqual(0, tokens.Count);
        }
    }
}
