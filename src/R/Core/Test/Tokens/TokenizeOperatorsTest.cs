using System.Diagnostics.CodeAnalysis;
using System.Text;
using Microsoft.Languages.Core.Tests.Tokens;
using Microsoft.R.Core.Tokens;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.R.Core.Test.Tokens {
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class TokenizeOperatorsTest : TokenizeTestBase<RToken, RTokenType> {
        [TestMethod]
        [TestCategory("R.Tokenizer")]
        public void Tokenize_OneCharOperatorsTest() {
            var tokens = this.Tokenize("^~-+*/$@<>|&=!?:", new RTokenizer());

            Assert.AreEqual(16, tokens.Count);
            for (int i = 0; i < tokens.Count; i++) {
                Assert.AreEqual(RTokenType.Operator, tokens[i].TokenType);
                Assert.AreEqual(i, tokens[i].Start);
                Assert.AreEqual(1, tokens[i].Length);
            }
        }

        [TestMethod]
        [TestCategory("R.Tokenizer")]
        public void Tokenize_TwoCharOperatorsTest() {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < Operators._twoChars.Length; i++) {
                sb.Append(Operators._twoChars[i]);
                sb.Append(' ');
            }

            var tokens = this.Tokenize(sb.ToString(), new RTokenizer());

            Assert.AreEqual(Operators._twoChars.Length, tokens.Count);
            for (int i = 0; i < tokens.Count; i++) {
                Assert.AreEqual(RTokenType.Operator, tokens[i].TokenType);
                Assert.AreEqual(3 * i, tokens[i].Start);
                Assert.AreEqual(2, tokens[i].Length);
            }
        }

        [TestMethod]
        [TestCategory("R.Tokenizer")]
        public void Tokenize_ThreeCharOperatorsTest() {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < Operators._threeChars.Length; i++) {
                sb.Append(Operators._threeChars[i]);
                sb.Append(' ');
            }

            var tokens = this.Tokenize(sb.ToString(), new RTokenizer());

            Assert.AreEqual(Operators._threeChars.Length, tokens.Count);
            for (int i = 0; i < tokens.Count; i++) {
                Assert.AreEqual(RTokenType.Operator, tokens[i].TokenType);
                Assert.AreEqual(4 * i, tokens[i].Start);
                Assert.AreEqual(3, tokens[i].Length);
            }
        }

        [TestMethod]
        [TestCategory("R.Tokenizer")]
        public void Tokenize_CustomOperatorsTest01() {
            var tokens = this.Tokenize("%foo% %русский%", new RTokenizer());

            Assert.AreEqual(2, tokens.Count);
            Assert.AreEqual(RTokenType.Operator, tokens[0].TokenType);
            Assert.AreEqual(0, tokens[0].Start);
            Assert.AreEqual(5, tokens[0].Length);

            Assert.AreEqual(RTokenType.Operator, tokens[1].TokenType);
            Assert.AreEqual(6, tokens[1].Start);
            Assert.AreEqual(9, tokens[1].Length);
        }

        [TestMethod]
        [TestCategory("R.Tokenizer")]
        public void Tokenize_CustomOperatorsTest02() {
            var tokens = this.Tokenize("%<% %?=?%", new RTokenizer());

            Assert.AreEqual(2, tokens.Count);
            Assert.AreEqual(RTokenType.Operator, tokens[0].TokenType);
            Assert.AreEqual(0, tokens[0].Start);
            Assert.AreEqual(3, tokens[0].Length);

            Assert.AreEqual(RTokenType.Operator, tokens[1].TokenType);
            Assert.AreEqual(4, tokens[1].Start);
            Assert.AreEqual(5, tokens[1].Length);
        }
    }
}
