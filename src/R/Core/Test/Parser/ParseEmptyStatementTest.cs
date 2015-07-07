using Microsoft.Languages.Core.Test.Utility;
using Microsoft.R.Core.Test.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.R.Core.Test.Tokens
{
    [TestClass]
    public class ParseEmptyStatementTest : UnitTestBase
    {
        [TestMethod]
        public void ParseEmptyStatementTest1()
        {
            string expected =
@"GlobalScope  [Global]
    Statement  [;]
        TokenNode  [; [0...1]]
    Statement  [;]
        TokenNode  [; [1...2]]
    Statement  [;]
        TokenNode  [; [2...3]]
";
            ParserTest.VerifyParse(expected, ";;;");
        }
    }
}
