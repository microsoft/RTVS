using System.Diagnostics.CodeAnalysis;
using Microsoft.Languages.Core.Test.Tokens;
using Microsoft.R.Core.Tokens;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.R.Core.Test.Tokens
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class TokenizeIdentifierTest : TokenizeTestBase<RToken, RTokenType>
    {
        [TestMethod]
        public void TokenizeIdentifierTest01()
        {
            var tokens = this.Tokenize("`_data_`", new RTokenizer());

            Assert.AreEqual(1, tokens.Count);
            Assert.AreEqual(RTokenType.Identifier, tokens[0].TokenType);
            Assert.AreEqual(0, tokens[0].Start);
            Assert.AreEqual(8, tokens[0].Length);
        }

        [TestMethod]
        public void TokenizeIdentifierTest02()
        {
            var tokens = this.Tokenize("\"odd name\" <- 1", new RTokenizer());

            Assert.AreEqual(3, tokens.Count);
            Assert.AreEqual(RTokenType.Identifier, tokens[0].TokenType);
            Assert.AreEqual(0, tokens[0].Start);
            Assert.AreEqual(10, tokens[0].Length);
        }

        [TestMethod]
        public void TokenizeIdentifierTest03()
        {
            var tokens = this.Tokenize("1 -> \"odd name\"", new RTokenizer());

            Assert.AreEqual(3, tokens.Count);
            Assert.AreEqual(RTokenType.Identifier, tokens[2].TokenType);
            Assert.AreEqual(5, tokens[2].Start);
            Assert.AreEqual(10, tokens[2].Length);
        }

        [TestMethod]
        public void Tokenize_IdentifiersFile()
        {
            TokenizeFiles.TokenizeFile(this.TestContext, @"Tokenization\Identifiers.r");
        }
    }
}
