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
    }
}
