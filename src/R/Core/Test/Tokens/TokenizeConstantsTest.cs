using System.Diagnostics.CodeAnalysis;
using System.Text;
using Microsoft.Languages.Core.Test.Tokens;
using Microsoft.Languages.Core.Text;
using Microsoft.R.Core.Tokens;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.R.Core.Test.Tokens
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class TokenizeConstantsTest : TokenizeTestBase<RToken, RTokenType>
    {
        [TestMethod]
        public void Tokenize_Missing()
        {
            string s = "NA NA_character_ NA_complex_ NA_integer_ NA_real_";

            IReadOnlyTextRangeCollection<RToken> tokens = this.Tokenize(s, new RTokenizer());

            Assert.AreEqual(5, tokens.Count);
            for (int i = 0; i < tokens.Count; i++)
            {
                Assert.AreEqual(RTokenType.Missing, tokens[i].TokenType);
                Assert.AreEqual(RTokenSubType.BuiltinConstant, tokens[i].SubType);
            }
        }
    }
}
