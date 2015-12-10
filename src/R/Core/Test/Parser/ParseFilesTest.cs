using System.Diagnostics.CodeAnalysis;
using Microsoft.Languages.Core.Test.Utility;
using Microsoft.R.Core.Test.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.R.Core.Test.Parser {
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class ParseFilesTest : UnitTestBase {
        [TestMethod]
        [TestCategory("R.Parser")]
        public void ParseFile_CheckR() {
            ParseFiles.ParseFile(this.TestContext, @"Parser\Check.r");
        }

        [TestMethod]
        [TestCategory("R.Parser")]
        public void ParseFile_FrametoolsR() {
            ParseFiles.ParseFile(this.TestContext, @"Parser\frametools.r");
        }
    }
}
