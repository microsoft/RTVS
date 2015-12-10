using System.Diagnostics.CodeAnalysis;
using Microsoft.Languages.Core.Test.Tokens;
using Microsoft.R.Core.Tokens;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.R.Core.Test.Tokens {
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class TokenizeIndexerTest : TokenizeTestBase<RToken, RTokenType> {
        [TestMethod]
        [TestCategory("R.Tokenizer")]
        public void TokenizeIndexerTest01() {
            var tokens = this.Tokenize("a[b[c]]", new RTokenizer());

            Assert.AreEqual(7, tokens.Count);
            Assert.AreEqual(RTokenType.Identifier, tokens[0].TokenType);
            Assert.AreEqual(RTokenType.OpenSquareBracket, tokens[1].TokenType);
            Assert.AreEqual(RTokenType.Identifier, tokens[2].TokenType);
            Assert.AreEqual(RTokenType.OpenSquareBracket, tokens[3].TokenType);
            Assert.AreEqual(RTokenType.Identifier, tokens[4].TokenType);
            Assert.AreEqual(RTokenType.CloseSquareBracket, tokens[5].TokenType);
            Assert.AreEqual(RTokenType.CloseSquareBracket, tokens[6].TokenType);
        }

        [TestMethod]
        [TestCategory("R.Tokenizer")]
        public void TokenizeIndexerTest02() {
            var tokens = this.Tokenize("a[[b[c]]]", new RTokenizer());

            Assert.AreEqual(7, tokens.Count);
            Assert.AreEqual(RTokenType.Identifier, tokens[0].TokenType);
            Assert.AreEqual(RTokenType.OpenDoubleSquareBracket, tokens[1].TokenType);
            Assert.AreEqual(RTokenType.Identifier, tokens[2].TokenType);
            Assert.AreEqual(RTokenType.OpenSquareBracket, tokens[3].TokenType);
            Assert.AreEqual(RTokenType.Identifier, tokens[4].TokenType);
            Assert.AreEqual(RTokenType.CloseSquareBracket, tokens[5].TokenType);
            Assert.AreEqual(RTokenType.CloseDoubleSquareBracket, tokens[6].TokenType);
        }

        [TestMethod]
        [TestCategory("R.Tokenizer")]
        public void TokenizeIndexerTest03() {
            var tokens = this.Tokenize("a[[b]][x]", new RTokenizer());

            Assert.AreEqual(7, tokens.Count);
            Assert.AreEqual(RTokenType.Identifier, tokens[0].TokenType);
            Assert.AreEqual(RTokenType.OpenDoubleSquareBracket, tokens[1].TokenType);
            Assert.AreEqual(RTokenType.Identifier, tokens[2].TokenType);
            Assert.AreEqual(RTokenType.CloseDoubleSquareBracket, tokens[3].TokenType);
            Assert.AreEqual(RTokenType.OpenSquareBracket, tokens[4].TokenType);
            Assert.AreEqual(RTokenType.Identifier, tokens[5].TokenType);
            Assert.AreEqual(RTokenType.CloseSquareBracket, tokens[6].TokenType);
        }

        [TestMethod]
        [TestCategory("R.Tokenizer")]
        public void TokenizeIndexerTest04() {
            var tokens = this.Tokenize("a[[b][x]]", new RTokenizer());

            Assert.AreEqual(8, tokens.Count);
            Assert.AreEqual(RTokenType.Identifier, tokens[0].TokenType);
            Assert.AreEqual(RTokenType.OpenDoubleSquareBracket, tokens[1].TokenType);
            Assert.AreEqual(RTokenType.Identifier, tokens[2].TokenType);
            Assert.AreEqual(RTokenType.CloseSquareBracket, tokens[3].TokenType);
            Assert.AreEqual(RTokenType.OpenSquareBracket, tokens[4].TokenType);
            Assert.AreEqual(RTokenType.Identifier, tokens[5].TokenType);
            Assert.AreEqual(RTokenType.CloseSquareBracket, tokens[6].TokenType);
            Assert.AreEqual(RTokenType.CloseSquareBracket, tokens[7].TokenType);
        }
    }
}
