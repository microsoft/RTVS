using System.Diagnostics.CodeAnalysis;
using Microsoft.Languages.Core.Test.Utility;
using Microsoft.R.Core.Test.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.R.Core.Test.Parser
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class ParseMultilineExpressionsTest : UnitTestBase
    {
        [TestMethod]
        public void ParseMultilineExpressionsTest1()
        {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [x+ \n 1 * ( \r\n a + b)]
        Expression  [x+ \n 1 * ( \r\n a + b)]
            TokenOperator  [+ [1...2)]
                Variable  [x]
                TokenNode  [+ [1...2)]
                TokenOperator  [* [7...8)]
                    NumericalValue  [1 [5...6)]
                    TokenNode  [* [7...8)]
                    Expression  [( \r\n a + b)]
                        TokenNode  [( [9...10)]
                        TokenOperator  [+ [16...17)]
                            Variable  [a]
                            TokenNode  [+ [16...17)]
                            Variable  [b]
                        TokenNode  [) [19...20)]
";
            ParserTest.VerifyParse(expected, "x+ \n 1 * ( \r\n a + b)");
        }

        [TestMethod]
        public void ParseMultilineExpressionsTest2()
        {
            string expected =
@"GlobalScope  [Global]
    KeywordStatement  []
        TokenNode  [break [0...5)]
    EmptyStatement  [8...9)
        TokenNode  [; [8...9)]
";
            ParserTest.VerifyParse(expected, "break \n ;");
        }

        [TestMethod]
        public void ParseMultilineExpressionsTest3()
        {
            string expected =
@"GlobalScope  [Global]
    For  []
        TokenNode  [for [0...3)]
        TokenNode  [( [7...8)]
        EnumerableExpression  [8...18)
            TokenNode  [x [8...9)]
            TokenNode  [in [12...14)]
            Expression  [y]
                Variable  [y]
        TokenNode  [) [18...19)]
        Scope  []
            TokenNode  [{ [22...23)]
            TokenNode  [} [26...27)]
";
            ParserTest.VerifyParse(expected, "for \r\n (x \n in \n y) \n { \n }");
        }

        [TestMethod]
        public void ParseMultilineExpressionsTest4()
        {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [x + 1]
        Expression  [x + 1]
            TokenOperator  [+ [2...3)]
                Variable  [x]
                TokenNode  [+ [2...3)]
                NumericalValue  [1 [4...5)]
    ExpressionStatement  [+ 2]
        Expression  [+ 2]
            TokenOperator  [+ [8...9)]
                TokenNode  [+ [8...9)]
                NumericalValue  [2 [10...11)]
";
            ParserTest.VerifyParse(expected, "x + 1 \n + 2");
        }
    }
}
