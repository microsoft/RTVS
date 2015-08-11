using Microsoft.Languages.Core.Test.Tokens;
using Microsoft.R.Core.Tokens;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.R.Core.Test.Tokens
{
    [TestClass]
    public class TokenizeFunctionsTest : TokenizeTestBase<RToken, RTokenType>
    {
        [TestMethod]
        public void TokenizeFunctionsTest1()
        {
            var tokens = this.Tokenize("x <- function( ", new RTokenizer());

            Assert.AreEqual(4, tokens.Count);

            Assert.AreEqual(RTokenType.Identifier, tokens[0].TokenType);
            Assert.AreEqual(0, tokens[0].Start);
            Assert.AreEqual(1, tokens[0].Length);

            Assert.AreEqual(RTokenType.Operator, tokens[1].TokenType);
            Assert.AreEqual(2, tokens[1].Start);
            Assert.AreEqual(2, tokens[1].Length);

            Assert.AreEqual(RTokenType.Keyword, tokens[2].TokenType);
            Assert.AreEqual(5, tokens[2].Start);
            Assert.AreEqual(8, tokens[2].Length);

            Assert.AreEqual(RTokenType.OpenBrace, tokens[3].TokenType);
            Assert.AreEqual(13, tokens[3].Start);
            Assert.AreEqual(1, tokens[3].Length);
        }

        [TestMethod]
        public void TokenizeFile_FunctionsFile()
        {
            TokenizeFiles.TokenizeFile(this.TestContext, @"Tokenization\Functions.r");
        }
    }
}
