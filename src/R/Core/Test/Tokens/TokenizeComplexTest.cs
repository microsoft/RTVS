using Microsoft.R.Core.Tokens;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.R.Core.Test.Tokens
{
    [TestClass]
    public class TokenizeComplexTest : TokenizeTestBase
    {
        [TestMethod]
        public void TokenizeComplexTest1()
        {
            var tokens = this.Tokenize("+1i");

            Assert.AreEqual(1, tokens.Count);
            Assert.AreEqual(RTokenType.Complex, tokens[0].TokenType);
            Assert.AreEqual(0, tokens[0].Start);
            Assert.AreEqual(3, tokens[0].Length);
        }

        [TestMethod]
        public void TokenizeComplexTest2()
        {
            var tokens = this.Tokenize("-.0+1i");

            Assert.AreEqual(1, tokens.Count);
            Assert.AreEqual(RTokenType.Complex, tokens[0].TokenType);
            Assert.AreEqual(0, tokens[0].Start);
            Assert.AreEqual(6, tokens[0].Length);
        }

        [TestMethod]
        public void TokenizeComplexTest3()
        {
            var tokens = this.Tokenize("0.e1-+1i");

            Assert.AreEqual(1, tokens.Count);
            Assert.AreEqual(RTokenType.Complex, tokens[0].TokenType);
            Assert.AreEqual(0, tokens[0].Start);
            Assert.AreEqual(8, tokens[0].Length);
        }

        [TestMethod]
        public void TokenizeComplexTest4()
        {
            var tokens = this.Tokenize(".0e-5+-1.e23i");

            Assert.AreEqual(1, tokens.Count);
            Assert.AreEqual(RTokenType.Complex, tokens[0].TokenType);
            Assert.AreEqual(0, tokens[0].Start);
            Assert.AreEqual(13, tokens[0].Length);
        }

        [TestMethod]
        public void TokenizeComplexTest5()
        {
            var tokens = this.Tokenize("-0.e2i");

            Assert.AreEqual(1, tokens.Count);
            Assert.AreEqual(RTokenType.Complex, tokens[0].TokenType);
            Assert.AreEqual(0, tokens[0].Start);
            Assert.AreEqual(6, tokens[0].Length);
        }

        [TestMethod]
        public void TokenizeComplexTest6()
        {
            var tokens = this.Tokenize("-12.+-.1ei");

            Assert.AreEqual(1, tokens.Count);
            Assert.AreEqual(RTokenType.Complex, tokens[0].TokenType);
            Assert.AreEqual(0, tokens[0].Start);
            Assert.AreEqual(10, tokens[0].Length);
        }

        [TestMethod]
        public void TokenizeComplexTest7()
        {
            var tokens = this.Tokenize("1i");

            Assert.AreEqual(1, tokens.Count);
            Assert.AreEqual(RTokenType.Complex, tokens[0].TokenType);
            Assert.AreEqual(0, tokens[0].Start);
            Assert.AreEqual(2, tokens[0].Length);
        }
    }
}
