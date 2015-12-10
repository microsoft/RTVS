using System.Diagnostics.CodeAnalysis;
using Microsoft.Languages.Core.Test.Tokens;
using Microsoft.R.Core.Tokens;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.R.Core.Test.Tokens {
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class TokenizeIntegersTest : TokenizeTestBase<RToken, RTokenType> {
        [TestMethod]
        [TestCategory("R.Tokenizer")]
        public void TokenizeIntegers1() {
            var tokens = this.Tokenize("+1 ", new RTokenizer());

            Assert.AreEqual(1, tokens.Count);
            Assert.AreEqual(RTokenType.Number, tokens[0].TokenType);
            Assert.AreEqual(0, tokens[0].Start);
            Assert.AreEqual(2, tokens[0].Length);
        }

        [TestMethod]
        [TestCategory("R.Tokenizer")]
        public void TokenizeIntegers2() {
            var tokens = this.Tokenize("-12 +1", new RTokenizer());

            Assert.AreEqual(3, tokens.Count);
            Assert.AreEqual(RTokenType.Number, tokens[0].TokenType);
            Assert.AreEqual(0, tokens[0].Start);
            Assert.AreEqual(3, tokens[0].Length);

            Assert.AreEqual(RTokenType.Operator, tokens[1].TokenType);
            Assert.AreEqual(4, tokens[1].Start);
            Assert.AreEqual(1, tokens[1].Length);

            Assert.AreEqual(RTokenType.Number, tokens[2].TokenType);
            Assert.AreEqual(5, tokens[2].Start);
            Assert.AreEqual(1, tokens[2].Length);
        }

        [TestMethod]
        [TestCategory("R.Tokenizer")]
        public void TokenizeIntegers3() {
            var tokens = this.Tokenize("-12+-1", new RTokenizer());

            Assert.AreEqual(3, tokens.Count);
            Assert.AreEqual(RTokenType.Number, tokens[0].TokenType);
            Assert.AreEqual(0, tokens[0].Start);
            Assert.AreEqual(3, tokens[0].Length);

            Assert.AreEqual(RTokenType.Operator, tokens[1].TokenType);
            Assert.AreEqual(3, tokens[1].Start);
            Assert.AreEqual(1, tokens[1].Length);

            Assert.AreEqual(RTokenType.Number, tokens[2].TokenType);
            Assert.AreEqual(4, tokens[2].Start);
            Assert.AreEqual(2, tokens[2].Length);
        }

        [TestMethod]
        [TestCategory("R.Tokenizer")]
        public void TokenizeFile_IntegerFile() {
            TokenizeFiles.TokenizeFile(this.TestContext, @"Tokenization\Integers.r");
        }

        [TestMethod]
        [TestCategory("R.Tokenizer")]
        public void TokenizeFile_HexFile() {
            TokenizeFiles.TokenizeFile(this.TestContext, @"Tokenization\Hex.r");
        }
    }
}
