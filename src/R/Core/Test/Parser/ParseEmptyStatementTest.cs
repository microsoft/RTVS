using Microsoft.Languages.Core.Test.Utility;
using Microsoft.R.Core.Test.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.R.Core.Test.Parser
{
    [TestClass]
    public class ParseEmptyStatementTest : UnitTestBase
    {
        [TestMethod]
        public void ParseEmptyStatementTest1()
        {
            string expected =
@"GlobalScope  [Global]
    EmptyStatement  [EmptyStatement]
        TokenNode  [; [0...1]]
    EmptyStatement  [EmptyStatement]
        TokenNode  [; [1...2]]
    EmptyStatement  [EmptyStatement]
        TokenNode  [; [2...3]]
";
            ParserTest.VerifyParse(expected, ";;;");
        }
    }
}
