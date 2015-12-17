using System.Diagnostics.CodeAnalysis;
using Microsoft.Common.Core.Test.Utility;
using Microsoft.R.Core.Test.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.R.Core.Test.Parser {
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class ParseEmptyStatementTest : UnitTestBase {
        [TestMethod]
        [TestCategory("R.Parser")]
        public void ParseEmptyStatementTest1() {
            string expected =
@"GlobalScope  [Global]
    EmptyStatement  [0...1)
        TokenNode  [; [0...1)]
    EmptyStatement  [1...2)
        TokenNode  [; [1...2)]
    EmptyStatement  [2...3)
        TokenNode  [; [2...3)]
";
            ParserTest.VerifyParse(expected, ";;;");
        }
    }
}
