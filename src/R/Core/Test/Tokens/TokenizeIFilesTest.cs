using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.R.Core.Test.Tokens
{
    [TestClass]
    public class TokenizeFilesTest : TokenizeTestBase
    {
        [TestMethod]
        public void TokenizeLeastSquares()
        {
            TokenizeFiles.TokenizeFile(this.TestContext, @"Tokenization\lsfit.r");
        }
    }
}
