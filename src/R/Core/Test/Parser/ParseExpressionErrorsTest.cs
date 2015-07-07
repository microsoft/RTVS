using Microsoft.Languages.Core.Test.Utility;
using Microsoft.R.Core.Test.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.R.Core.Test.Tokens
{
    [TestClass]
    public class ParseExpressionErrorsTest : UnitTestBase
    {
        [TestMethod]
        public void ParseIncompleteExpressionTest1()
        {
            string expected =
@"GlobalScope  [Global]

OperandExpected AfterToken [0...1)
";
            ParserTest.VerifyParse(expected, "+");
        }

        [TestMethod]
        public void ParseIncompleteExpressionTest2()
        {
            string expected =
@"GlobalScope  [Global]

OperandExpected AfterToken [1...2)
";
            ParserTest.VerifyParse(expected, "x+");
        }

        [TestMethod]
        public void ParseMismatchBracesTest1()
        {
            string expected =
@"GlobalScope  [Global]

CloseBraceExpected AfterToken [0...1)
UnexpectedEndOfFile AfterToken [0...1)
";
            ParserTest.VerifyParse(expected, "(");
        }
        [TestMethod]
        public void ParseMismatchBracesTest2()
        {
            string expected =
@"GlobalScope  [Global]

UnexpectedToken Token [0...1)
";
            ParserTest.VerifyParse(expected, ")");
        }

        [TestMethod]
        public void ParseMismatchBracesTest3()
        {
            string expected =
@"GlobalScope  [Global]

CloseBraceExpected AfterToken [3...4)
UnexpectedEndOfFile AfterToken [3...4)
";
            ParserTest.VerifyParse(expected, "((x)");
        }

        [TestMethod]
        public void ParseMismatchBracesTest4()
        {
            string expected =
@"GlobalScope  [Global]

OperandExpected AfterToken [2...3)
CloseBraceExpected AfterToken [2...3)
UnexpectedEndOfFile AfterToken [2...3)
";
            ParserTest.VerifyParse(expected, "(x+");
        }

        [TestMethod]
        public void ParseMismatchBracesTest5()
        {
            string expected =
@"GlobalScope  [Global]

OperandExpected AfterToken [5...6)
";
            ParserTest.VerifyParse(expected, "(a+b)+)");
        }
    }
}
