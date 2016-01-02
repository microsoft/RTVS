using System.Diagnostics.CodeAnalysis;
using Microsoft.Languages.Core.Tests.Tokens;
using Microsoft.R.Support.RD.Tokens;
using Microsoft.R.Support.Test.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.R.Support.Test.RD.Tokens {
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class TokenizeSampleRdFilesTest : TokenizeTestBase<RdToken, RdTokenType> {
        [TestMethod]
        [TestCategory("Rd.Tokenizer")]
        public void TokenizeSampleRdFile01() {
            TokenizeFiles.TokenizeFile<RdToken, RdTokenType, RdTokenizer>(this.TestContext, @"Tokenization\01.rd", "RD");
        }

        [TestMethod]
        [TestCategory("Rd.Tokenizer")]
        public void TokenizeSampleRdFile02() {
            TokenizeFiles.TokenizeFile<RdToken, RdTokenType, RdTokenizer>(this.TestContext, @"Tokenization\02.rd", "RD");
        }

        [TestMethod]
        [TestCategory("Rd.Tokenizer")]
        public void TokenizeSampleRdFile03() {
            TokenizeFiles.TokenizeFile<RdToken, RdTokenType, RdTokenizer>(this.TestContext, @"Tokenization\03.rd", "RD");
        }

        [TestMethod]
        [TestCategory("Rd.Tokenizer")]
        public void TokenizeSampleRdFile04() {
            TokenizeFiles.TokenizeFile<RdToken, RdTokenType, RdTokenizer>(this.TestContext, @"Tokenization\04.rd", "RD");
        }

        [TestMethod]
        [TestCategory("Rd.Tokenizer")]
        public void TokenizeSampleRdFile05() {
            TokenizeFiles.TokenizeFile<RdToken, RdTokenType, RdTokenizer>(this.TestContext, @"Tokenization\05.rd", "RD");
        }

        [TestMethod]
        [TestCategory("Rd.Tokenizer")]
        public void TokenizeSampleRdFile06() {
            TokenizeFiles.TokenizeFile<RdToken, RdTokenType, RdTokenizer>(this.TestContext, @"Tokenization\06.rd", "RD");
        }

        [TestMethod]
        [TestCategory("Rd.Tokenizer")]
        public void TokenizeSampleRdFile07() {
            TokenizeFiles.TokenizeFile<RdToken, RdTokenType, RdTokenizer>(this.TestContext, @"Tokenization\07.rd", "RD");
        }

        [TestMethod]
        [TestCategory("Rd.Tokenizer")]
        public void TokenizeSampleRdFile08() {
            TokenizeFiles.TokenizeFile<RdToken, RdTokenType, RdTokenizer>(this.TestContext, @"Tokenization\08.rd", "RD");
        }

        [TestMethod]
        [TestCategory("Rd.Tokenizer")]
        public void TokenizeSampleRdFile09() {
            TokenizeFiles.TokenizeFile<RdToken, RdTokenType, RdTokenizer>(this.TestContext, @"Tokenization\09.rd", "RD");
        }

        [TestMethod]
        [TestCategory("Rd.Tokenizer")]
        public void TokenizeSampleRdFile10() {
            TokenizeFiles.TokenizeFile<RdToken, RdTokenType, RdTokenizer>(this.TestContext, @"Tokenization\10.rd", "RD");
        }

        [TestMethod]
        [TestCategory("Rd.Tokenizer")]
        public void TokenizeSampleRdFile11() {
            TokenizeFiles.TokenizeFile<RdToken, RdTokenType, RdTokenizer>(this.TestContext, @"Tokenization\11.rd", "RD");
        }

        [TestMethod]
        [TestCategory("Rd.Tokenizer")]
        public void TokenizeSampleRdFile12() {
            TokenizeFiles.TokenizeFile<RdToken, RdTokenType, RdTokenizer>(this.TestContext, @"Tokenization\12.rd", "RD");
        }
    }
}
