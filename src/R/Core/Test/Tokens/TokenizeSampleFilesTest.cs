using System.Diagnostics.CodeAnalysis;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.R.Core.Test.Tokens {
    [ExcludeFromCodeCoverage]
    public class TokenizeRSampleFilesTest {
        private readonly CoreTestFilesFixture _files;

        public TokenizeRSampleFilesTest(CoreTestFilesFixture files) {
            _files = files;
        }

        [Test]
        [Category.R.Tokenizer]
        public void TokenizeLeastSquares() {
            TokenizeFiles.TokenizeFile(_files, @"Tokenization\lsfit.r");
        }
    }
}
