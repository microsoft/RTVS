using System.Diagnostics.CodeAnalysis;
using Microsoft.R.Core.Test.Utility;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.R.Core.Test.Parser {
    [ExcludeFromCodeCoverage]
    public class ParseFilesTest {
        private readonly CoreTestFilesFixture _files;

        public ParseFilesTest(CoreTestFilesFixture files) {
            _files = files;
        }

        [Test]
        [Category.R.Parser]
        public void ParseFile_CheckR() {
            ParseFiles.ParseFile(_files, @"Parser\Check.r");
        }

        [Test]
        [Category.R.Parser]
        public void ParseFile_FrametoolsR() {
            ParseFiles.ParseFile(_files, @"Parser\frametools.r");
        }
    }
}
