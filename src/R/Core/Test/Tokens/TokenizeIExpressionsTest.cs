using Microsoft.R.Core.Tokens;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.R.Core.Test.Tokens
{
    [TestClass]
    public class TokenizeExpressionsTest : TokenizeTestBase
    {
        [TestMethod]
        public void TokenizeFile_ExpressionsFile()
        {
            TokenizeFiles.TokenizeFile(this.TestContext, @"Tokenization\Expressions.r");
        }
    }
}
