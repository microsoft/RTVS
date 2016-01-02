using System.Diagnostics.CodeAnalysis;
using Microsoft.Languages.Core.Test.Tokens;
using Microsoft.R.Core.Tokens;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.R.Core.Test.Tokens {
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class TokenizeComplexTest : TokenizeTestBase<RToken, RTokenType> {
        [TestMethod]
        [TestCategory("R.Tokenizer")]
        public void TokenizeComplexTest1() {
            var tokens = this.Tokenize("+1i", new RTokenizer());

            Assert.AreEqual(1, tokens.Count);
            Assert.AreEqual(RTokenType.Complex, tokens[0].TokenType);
            Assert.AreEqual(0, tokens[0].Start);
            Assert.AreEqual(3, tokens[0].Length);
        }

        [TestMethod]
        [TestCategory("R.Tokenizer")]
        public void TokenizeComplexTest2() {
            var tokens = this.Tokenize("-.0+1i", new RTokenizer());

            Assert.AreEqual(1, tokens.Count);
            Assert.AreEqual(RTokenType.Complex, tokens[0].TokenType);
            Assert.AreEqual(0, tokens[0].Start);
            Assert.AreEqual(6, tokens[0].Length);
        }

        [TestMethod]
        [TestCategory("R.Tokenizer")]
        public void TokenizeComplexTest3() {
            var tokens = this.Tokenize("0.e1-+1i", new RTokenizer());

            Assert.AreEqual(1, tokens.Count);
            Assert.AreEqual(RTokenType.Complex, tokens[0].TokenType);
            Assert.AreEqual(0, tokens[0].Start);
            Assert.AreEqual(8, tokens[0].Length);
        }

        [TestMethod]
        [TestCategory("R.Tokenizer")]
        public void TokenizeComplexTest4() {
            var tokens = this.Tokenize(".0e-5+-1.e23i", new RTokenizer());

            Assert.AreEqual(1, tokens.Count);
            Assert.AreEqual(RTokenType.Complex, tokens[0].TokenType);
            Assert.AreEqual(0, tokens[0].Start);
            Assert.AreEqual(13, tokens[0].Length);
        }

        [TestMethod]
        [TestCategory("R.Tokenizer")]
        public void TokenizeComplexTest5() {
            var tokens = this.Tokenize("-0.e2i", new RTokenizer());

            Assert.AreEqual(1, tokens.Count);
            Assert.AreEqual(RTokenType.Complex, tokens[0].TokenType);
            Assert.AreEqual(0, tokens[0].Start);
            Assert.AreEqual(6, tokens[0].Length);
        }

        [TestMethod]
        [TestCategory("R.Tokenizer")]
        public void TokenizeComplexTest6() {
            var tokens = this.Tokenize("1i", new RTokenizer());

            Assert.AreEqual(1, tokens.Count);
            Assert.AreEqual(RTokenType.Complex, tokens[0].TokenType);
            Assert.AreEqual(0, tokens[0].Start);
            Assert.AreEqual(2, tokens[0].Length);
        }
    }
}
