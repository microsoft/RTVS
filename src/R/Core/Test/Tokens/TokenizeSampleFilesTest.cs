using System.Diagnostics.CodeAnalysis;
using Microsoft.Languages.Core.Tests.Tokens;
using Microsoft.R.Core.Tokens;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.R.Core.Test.Tokens {
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class TokenizeRSampleFilesTest : TokenizeTestBase<RToken, RTokenType> {
        [TestMethod]
        [TestCategory("R.Tokenizer")]
        public void TokenizeLeastSquares() {
            TokenizeFiles.TokenizeFile(this.TestContext, @"Tokenization\lsfit.r");
        }
    }
}
