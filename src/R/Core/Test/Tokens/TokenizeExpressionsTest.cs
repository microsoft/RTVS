using Microsoft.Languages.Core.Test.Tokens;
using Microsoft.R.Core.Tokens;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.R.Core.Test.Tokens
{
    [TestClass]
    public class TokenizeExpressionsTest : TokenizeTestBase<RToken, RTokenType>
    {
        [TestMethod]
        public void TokenizeFile_ExpressionsFile()
        {
            TokenizeFiles.TokenizeFile(this.TestContext, @"Tokenization\Expressions.r");
        }
    }
}
