using System.Diagnostics.CodeAnalysis;
using Microsoft.Languages.Core.Test.Utility;
using Microsoft.R.Core.Test.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.R.Core.Test.Parser
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class ParseExpressionErrorsTest : UnitTestBase
    {
        [TestMethod]
        public void ParseIncompleteExpressionTest01()
        {
            string expected =
@"GlobalScope  [Global]

OperandExpected Token [0...1)
";
            ParserTest.VerifyParse(expected, "+");
        }

        [TestMethod]
        public void ParseIncompleteExpressionTest02()
        {
            string expected =
@"GlobalScope  [Global]

OperandExpected AfterToken [1...2)
";
            ParserTest.VerifyParse(expected, "x+");
        }

        [TestMethod]
        public void ParseIncompleteExpressionTest03()
        {
            string expected =
@"GlobalScope  [Global]

OperatorExpected Token [2...3)
";
            ParserTest.VerifyParse(expected, "a b");
        }

        [TestMethod]
        public void ParseMismatchBracesTest01()
        {
            string expected =
@"GlobalScope  [Global]

CloseBraceExpected AfterToken [0...1)
UnexpectedEndOfFile AfterToken [0...1)
";
            ParserTest.VerifyParse(expected, "(");
        }

        [TestMethod]
        public void ParseMismatchBracesTest02()
        {
            string expected =
@"GlobalScope  [Global]

CloseBraceExpected AfterToken [3...4)
UnexpectedEndOfFile AfterToken [3...4)
";
            ParserTest.VerifyParse(expected, "((x)");
        }

        [TestMethod]
        public void ParseMismatchBracesTest03()
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
        public void ParseMismatchBracesTest04()
        {
            string expected =
@"GlobalScope  [Global]

OperandExpected Token [6...7)
";
            ParserTest.VerifyParse(expected, "(a+b)+)");
        }

        [TestMethod]
        public void ParseMissingAssignmentTest01()
        {
            string expected =
@"GlobalScope  [Global]

OperatorExpected Token [2...10)
FunctionBodyExpected Token [12...13)
";
            ParserTest.VerifyParse(expected, "x function(a)");
        }

        [TestMethod]
        public void ParseIncompleteOperatorTest01()
        {
            string expected =
@"GlobalScope  [Global]

OperandExpected AfterToken [8...9)
";
            ParserTest.VerifyParse(expected, "y <- 2.5*");
        }

        [TestMethod]
        public void ParseMissingOperatorTest01()
        {
            string expected =
@"GlobalScope  [Global]

OperatorExpected Token [3...4)
";
            ParserTest.VerifyParse(expected, "a()b");
        }

        [TestMethod]
        public void ParseMissingOperatorTest02()
        {
            string expected =
@"GlobalScope  [Global]

OperatorExpected Token [2...3)
";
            ParserTest.VerifyParse(expected, "a b");
        }
    }
}
