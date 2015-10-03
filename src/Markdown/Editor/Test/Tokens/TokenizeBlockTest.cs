using System.Diagnostics.CodeAnalysis;
using Microsoft.Languages.Core.Classification;
using Microsoft.Languages.Core.Test.Tokens;
using Microsoft.Markdown.Editor.Tokens;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Markdown.Editor.Test.Tokens
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class TokenizeBlockTest : TokenizeTestBase<MarkdownToken, MarkdownTokenType>
    {
        [TestMethod]
        public void TokenizeMd_Block01()
        {
            var tokens = this.Tokenize(@"```block```", new MdTokenizer());
            Assert.AreEqual(0, tokens.Count);
        }

        [TestMethod]
        public void TokenizeMd_Block02()
        {
            string content =
@"```
block

block
```
";
            var tokens = this.Tokenize(content, new MdTokenizer());

            Assert.AreEqual(1, tokens.Count);

            Assert.AreEqual(MarkdownTokenType.Code, tokens[0].TokenType);
            Assert.AreEqual(0, tokens[0].Start);
            Assert.AreEqual(content.Length - 2, tokens[0].Length);
        }

        [TestMethod]
        public void TokenizeMd_Block03()
        {
            var tokens = this.Tokenize(@"```block", new MdTokenizer());
            Assert.AreEqual(0, tokens.Count);
        }

        [TestMethod]
        public void TokenizeMd_Block04()
        {
            var tokens = this.Tokenize(@"```block` ```", new MdTokenizer());
            Assert.AreEqual(0, tokens.Count);
        }

        [TestMethod]
        public void TokenizeMd_Block05()
        {
            string content =
@"```
block```
 ```
block
```
";
            var tokens = this.Tokenize(content, new MdTokenizer());
            Assert.AreEqual(1, tokens.Count);

            Assert.AreEqual(MarkdownTokenType.Code, tokens[0].TokenType);
            Assert.AreEqual(0, tokens[0].Start);
            Assert.AreEqual(content.Length - 2, tokens[0].Length);
        }

        [TestMethod]
        public void TokenizeMd_Block06()
        {
            var tokens = this.Tokenize(@"`r x <- 1`", new MdTokenizer());
            Assert.AreEqual(3, tokens.Count);
            Assert.AreEqual(MarkdownTokenType.Code, tokens[0].TokenType);
            Assert.IsTrue(tokens[1] is MarkdownRCodeToken);
            Assert.AreEqual(MarkdownTokenType.Code, tokens[2].TokenType);

            ICompositeToken composite = tokens[1] as ICompositeToken;
            Assert.AreEqual(3, composite.TokenList.Count);
        }
    }
}
