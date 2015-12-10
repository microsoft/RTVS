using System.Diagnostics.CodeAnalysis;
using Microsoft.Languages.Core.Test.Tokens;
using Microsoft.R.Core.Tokens;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.R.Core.Test.Tokens {
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class TokenizeIdentifierTest : TokenizeTestBase<RToken, RTokenType> {
        [TestMethod]
        [TestCategory("R.Tokenizer")]
        public void TokenizeIdentifierTest01() {
            var tokens = this.Tokenize("`_data_`", new RTokenizer());

            Assert.AreEqual(1, tokens.Count);
            Assert.AreEqual(RTokenType.Identifier, tokens[0].TokenType);
            Assert.AreEqual(0, tokens[0].Start);
            Assert.AreEqual(8, tokens[0].Length);
        }

        [TestMethod]
        [TestCategory("R.Tokenizer")]
        public void TokenizeIdentifierTest02() {
            var tokens = this.Tokenize("\"odd name\" <- 1", new RTokenizer());

            Assert.AreEqual(3, tokens.Count);
            Assert.AreEqual(RTokenType.Identifier, tokens[0].TokenType);
            Assert.AreEqual(0, tokens[0].Start);
            Assert.AreEqual(10, tokens[0].Length);
        }

        [TestMethod]
        [TestCategory("R.Tokenizer")]
        public void TokenizeIdentifierTest03() {
            var tokens = this.Tokenize("1 -> \"odd name\"", new RTokenizer());

            Assert.AreEqual(3, tokens.Count);
            Assert.AreEqual(RTokenType.Identifier, tokens[2].TokenType);
            Assert.AreEqual(5, tokens[2].Start);
            Assert.AreEqual(10, tokens[2].Length);
        }

        [TestMethod]
        [TestCategory("R.Tokenizer")]
        public void TokenizeIdentifierLogicalTest01() {
            var tokens = this.Tokenize("1 <- F(~x)", new RTokenizer());

            Assert.AreEqual(7, tokens.Count);
            Assert.AreEqual(RTokenType.Number, tokens[0].TokenType);
            Assert.AreEqual(RTokenType.Operator, tokens[1].TokenType);
            Assert.AreEqual(RTokenType.Identifier, tokens[2].TokenType);
            Assert.AreEqual(RTokenType.OpenBrace, tokens[3].TokenType);
            Assert.AreEqual(RTokenType.Operator, tokens[4].TokenType);
            Assert.AreEqual(RTokenType.Identifier, tokens[5].TokenType);
            Assert.AreEqual(RTokenType.CloseBrace, tokens[6].TokenType);
        }

        [TestMethod]
        [TestCategory("R.Tokenizer")]
        public void TokenizeIdentifierLogicalTest02() {
            var tokens = this.Tokenize("1 <- F", new RTokenizer());

            Assert.AreEqual(3, tokens.Count);
            Assert.AreEqual(RTokenType.Number, tokens[0].TokenType);
            Assert.AreEqual(RTokenType.Operator, tokens[1].TokenType);
            Assert.AreEqual(RTokenType.Logical, tokens[2].TokenType);
        }

        [TestMethod]
        [TestCategory("R.Tokenizer")]
        public void Tokenize_IdentifiersFile() {
            TokenizeFiles.TokenizeFile(this.TestContext, @"Tokenization\Identifiers.r");
        }
    }
}
