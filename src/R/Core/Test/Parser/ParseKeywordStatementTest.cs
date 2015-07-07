using Microsoft.Languages.Core.Test.Utility;
using Microsoft.R.Core.Test.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.R.Core.Test.Tokens
{
    [TestClass]
    public class ParseKeywordStatement : UnitTestBase
    {
        [TestMethod]
        public void ParseBreakTest1()
        {
            string expected =
@"GlobalScope  [Global]
    KeywordStatement  []
        TokenNode  [break [0...5]]
";
            ParserTest.VerifyParse(expected, "break");
        }

        [TestMethod]
        public void ParseBreakTest2()
        {
            string expected =
@"GlobalScope  [Global]
    KeywordStatement  [;]
        TokenNode  [break [0...5]]
        TokenNode  [; [5...6]]
";
            ParserTest.VerifyParse(expected, "break;");
        }

        [TestMethod]
        public void ParseNextTest1()
        {
            string expected =
@"GlobalScope  [Global]
    KeywordStatement  []
        TokenNode  [next [0...4]]
";
            ParserTest.VerifyParse(expected, "next");
        }

        [TestMethod]
        public void ParseNextTest2()
        {
            string expected =
@"GlobalScope  [Global]
    KeywordStatement  [;]
        TokenNode  [next [0...4]]
        TokenNode  [; [4...5]]
";
            ParserTest.VerifyParse(expected, "next;");
        }
    }
}
