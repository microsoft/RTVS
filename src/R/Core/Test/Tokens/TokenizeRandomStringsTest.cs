using System.Diagnostics.CodeAnalysis;
using Microsoft.Languages.Core.Tests.Tokens;
using Microsoft.R.Core.Tokens;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.R.Core.Test.Tokens {
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class TokenizeRandomStringsTest : TokenizeTestBase<RToken, RTokenType> {
        [TestMethod]
        [TestCategory("R.Tokenizer")]
        public void Tokenize_NonEnglishString01() {
            var tokens = this.Tokenize(" русский ", new RTokenizer());
            Assert.AreEqual(1, tokens.Count);
            Assert.AreEqual(RTokenType.Unknown, tokens[0].TokenType);
            Assert.AreEqual(1, tokens[0].Start);
            Assert.AreEqual(7, tokens[0].Length);
        }
    }
}
