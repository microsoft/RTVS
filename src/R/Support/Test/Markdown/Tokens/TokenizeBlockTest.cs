using System.Diagnostics.CodeAnalysis;
using Microsoft.Languages.Core.Test.Tokens;
using Microsoft.R.Support.Markdown.Tokens;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.R.Support.Test.Markdown.Tokens
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class TokenizeBlockTest : TokenizeTestBase<MdToken, MdTokenType>
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

            Assert.AreEqual(MdTokenType.Code, tokens[0].TokenType);
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

            Assert.AreEqual(MdTokenType.Code, tokens[0].TokenType);
            Assert.AreEqual(0, tokens[0].Start);
            Assert.AreEqual(content.Length - 2, tokens[0].Length);
        }
    }
}
