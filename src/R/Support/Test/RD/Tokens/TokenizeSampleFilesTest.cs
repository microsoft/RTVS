using Microsoft.Languages.Core.Test.Tokens;
using Microsoft.R.Support.RD.Tokens;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.R.Support.Test.RD.Tokens
{
    [TestClass]
    public class TokenizeSampleRdFilesTest : TokenizeTestBase<RdToken, RdTokenType>
    {
        [TestMethod]
        public void TokenizeSampleRdFile01()
        {
            TokenizeFiles.TokenizeFile(this.TestContext, @"Tokenization\01.rd");
        }

        [TestMethod]
        public void TokenizeSampleRdFile02()
        {
            TokenizeFiles.TokenizeFile(this.TestContext, @"Tokenization\02.rd");
        }

        [TestMethod]
        public void TokenizeSampleRdFile03()
        {
            TokenizeFiles.TokenizeFile(this.TestContext, @"Tokenization\03.rd");
        }

        [TestMethod]
        public void TokenizeSampleRdFile04()
        {
            TokenizeFiles.TokenizeFile(this.TestContext, @"Tokenization\04.rd");
        }

        [TestMethod]
        public void TokenizeSampleRdFile05()
        {
            TokenizeFiles.TokenizeFile(this.TestContext, @"Tokenization\05.rd");
        }

        [TestMethod]
        public void TokenizeSampleRdFile06()
        {
            TokenizeFiles.TokenizeFile(this.TestContext, @"Tokenization\06.rd");
        }

        [TestMethod]
        public void TokenizeSampleRdFile07()
        {
            TokenizeFiles.TokenizeFile(this.TestContext, @"Tokenization\07.rd");
        }

        [TestMethod]
        public void TokenizeSampleRdFile08()
        {
            TokenizeFiles.TokenizeFile(this.TestContext, @"Tokenization\08.rd");
        }

        [TestMethod]
        public void TokenizeSampleRdFile09()
        {
            TokenizeFiles.TokenizeFile(this.TestContext, @"Tokenization\09.rd");
        }

        [TestMethod]
        public void TokenizeSampleRdFile10()
        {
            TokenizeFiles.TokenizeFile(this.TestContext, @"Tokenization\10.rd");
        }

        [TestMethod]
        public void TokenizeSampleRdFile11()
        {
            TokenizeFiles.TokenizeFile(this.TestContext, @"Tokenization\11.rd");
        }

        [TestMethod]
        public void TokenizeSampleRdFile12()
        {
            TokenizeFiles.TokenizeFile(this.TestContext, @"Tokenization\12.rd");
        }
    }
}
