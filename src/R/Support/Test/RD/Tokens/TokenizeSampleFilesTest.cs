using Microsoft.Languages.Core.Test.Tokens;
using Microsoft.R.Support.RD.Tokens;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.R.Support.Test.RD.Tokens
{
    [TestClass]
    public class TokenizeSampleRdFilesTest : TokenizeTestBase<RdToken, RdTokenType>
    {
        [TestMethod]
        public void TokenizeSampleRdFile1()
        {
            TokenizeFiles.TokenizeFile(this.TestContext, @"Tokenization\1.rd");
        }
    }
}
