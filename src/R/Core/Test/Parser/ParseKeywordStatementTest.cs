using System.Diagnostics.CodeAnalysis;
using Microsoft.Languages.Core.Test.Utility;
using Microsoft.R.Core.Test.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.R.Core.Test.Parser
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class ParseKeywordStatement : UnitTestBase
    {
        [TestMethod]
        public void ParseBreakTest1()
        {
            string expected =
@"GlobalScope  [Global]
    KeywordStatement  []
        TokenNode  [break [0...5)]
";
            ParserTest.VerifyParse(expected, "break");
        }

        [TestMethod]
        public void ParseBreakTest2()
        {
            string expected =
@"GlobalScope  [Global]
    KeywordStatement  [;]
        TokenNode  [break [0...5)]
        TokenNode  [; [5...6)]
";
            ParserTest.VerifyParse(expected, "break;");
        }

        [TestMethod]
        public void ParseNextTest1()
        {
            string expected =
@"GlobalScope  [Global]
    KeywordStatement  []
        TokenNode  [next [0...4)]
";
            ParserTest.VerifyParse(expected, "next");
        }

        [TestMethod]
        public void ParseNextTest2()
        {
            string expected =
@"GlobalScope  [Global]
    KeywordStatement  [;]
        TokenNode  [next [0...4)]
        TokenNode  [; [4...5)]
";
            ParserTest.VerifyParse(expected, "next;");
        }

        [TestMethod]
        public void ParseLibraryTest01()
        {
            string expected =
@"GlobalScope  [Global]
    LibraryStatement  []
        TokenNode  [library [0...7)]
        TokenNode  [( [7...8)]
        TokenNode  [abind [8...13)]
        TokenNode  [) [13...14)]
";
            ParserTest.VerifyParse(expected, "library(abind)");
        }

        [TestMethod]
        public void ParseLibraryTest02()
        {
            string expected =
@"GlobalScope  [Global]
    LibraryStatement  []
        TokenNode  [library [0...7)]
        TokenNode  [( [7...8)]
        TokenNode  ['abind' [8...15)]
        TokenNode  [) [15...16)]
";
            ParserTest.VerifyParse(expected, "library('abind')");
        }

        [TestMethod]
        public void ParseLibraryTest03()
        {
            string expected =
@"GlobalScope  [Global]

IndentifierExpected Token [8...9)
";
            ParserTest.VerifyParse(expected, "library()");
        }

        [TestMethod]
        public void ParseReturnTest01()
        {
            string expected =
@"GlobalScope  [Global]
    KeywordExpressionStatement  []
        TokenNode  [return [0...6)]
        TokenNode  [( [6...7)]
        TokenNode  [) [7...8)]
";
            ParserTest.VerifyParse(expected, "return()");
        }

        [TestMethod]
        public void ParseTypeofTest()
        {
            string expected =
@"GlobalScope  [Global]
    KeywordExpressionStatement  []
        TokenNode  [typeof [0...6)]
        TokenNode  [( [6...7)]
        Expression  [1]
            NumericalValue  [1 [7...8)]
        TokenNode  [) [8...9)]
";
            ParserTest.VerifyParse(expected, "typeof(1)");
        }

        [TestMethod]
        public void ParseSwitchTest()
        {
            string expected =
@"GlobalScope  [Global]
    KeywordFunctionStatement  [0...11)
        TokenNode  [switch [0...6)]
        TokenNode  [( [6...7)]
        ArgumentList  [7...10)
            ExpressionArgument  [7...9)
                Expression  [1]
                    NumericalValue  [1 [7...8)]
                TokenNode  [, [8...9)]
            ExpressionArgument  [9...10)
                Expression  [2]
                    NumericalValue  [2 [9...10)]
        TokenNode  [) [10...11)]
";
            ParserTest.VerifyParse(expected, "switch(1,2)");
        }
    }
}
