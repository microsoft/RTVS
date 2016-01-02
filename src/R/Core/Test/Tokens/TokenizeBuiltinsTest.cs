using System.Diagnostics.CodeAnalysis;
using Microsoft.Languages.Core.Test.Tokens;
using Microsoft.Languages.Core.Text;
using Microsoft.R.Core.Tokens;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.R.Core.Test.Tokens {
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class TokenizeBuiltinsTest : TokenizeTestBase<RToken, RTokenType> {
        [TestMethod]
        [TestCategory("R.Tokenizer")]
        public void Tokenize_BuiltIns01() {
            IReadOnlyTextRangeCollection<RToken> tokens = this.Tokenize("require library switch return", new RTokenizer());

            Assert.AreEqual(4, tokens.Count);
            for (int i = 0; i < tokens.Count; i++) {
                Assert.AreEqual(RTokenType.Identifier, tokens[i].TokenType);
                Assert.AreEqual(RTokenSubType.BuiltinFunction, tokens[i].SubType);
            }
        }

        [TestMethod]
        [TestCategory("R.Tokenizer")]
        public void Tokenize_BuiltIns02() {
            IReadOnlyTextRangeCollection<RToken> tokens = this.Tokenize("require() library() switch() return()", new RTokenizer());

            Assert.AreEqual(12, tokens.Count);
            for (int i = 0; i < tokens.Count; i += 3) {
                Assert.AreEqual(RTokenType.Identifier, tokens[i].TokenType);
                Assert.AreEqual(RTokenSubType.BuiltinFunction, tokens[i].SubType);
                Assert.AreEqual(RTokenType.OpenBrace, tokens[i + 1].TokenType);
                Assert.AreEqual(RTokenType.CloseBrace, tokens[i + 2].TokenType);
            }
        }
    }
}
