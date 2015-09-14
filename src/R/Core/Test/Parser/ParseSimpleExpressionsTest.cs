using System.Diagnostics.CodeAnalysis;
using Microsoft.Languages.Core.Test.Utility;
using Microsoft.R.Core.Test.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.R.Core.Test.Parser
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class ParseSimpleExpressionsTest : UnitTestBase
    {
        [TestMethod]
        public void ParseSimpleExpressions01()
        {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [a*b+c]
        Expression  [a*b+c]
            TokenOperator  [+ [3...4)]
                TokenOperator  [* [1...2)]
                    Variable  [a]
                    TokenNode  [* [1...2)]
                    Variable  [b]
                TokenNode  [+ [3...4)]
                Variable  [c]
";
            ParserTest.VerifyParse(expected, "a*b+c");
        }

        [TestMethod]
        public void ParseSimpleExpressions02()
        {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [a*b+c*d]
        Expression  [a*b+c*d]
            TokenOperator  [+ [3...4)]
                TokenOperator  [* [1...2)]
                    Variable  [a]
                    TokenNode  [* [1...2)]
                    Variable  [b]
                TokenNode  [+ [3...4)]
                TokenOperator  [* [5...6)]
                    Variable  [c]
                    TokenNode  [* [5...6)]
                    Variable  [d]
";
            ParserTest.VerifyParse(expected, "a*b+c*d");
        }

        [TestMethod]
        public void ParseSimpleExpressions03()
        {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [a*(b+c)]
        Expression  [a*(b+c)]
            TokenOperator  [* [1...2)]
                Variable  [a]
                TokenNode  [* [1...2)]
                Expression  [(b+c)]
                    TokenNode  [( [2...3)]
                    TokenOperator  [+ [4...5)]
                        Variable  [b]
                        TokenNode  [+ [4...5)]
                        Variable  [c]
                    TokenNode  [) [6...7)]
";
            ParserTest.VerifyParse(expected, "a*(b+c)");
        }

        [TestMethod]
        public void ParseSimpleExpressions04()
        {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [((x))]
        Expression  [((x))]
            TokenNode  [( [0...1)]
            Expression  [(x)]
                TokenNode  [( [1...2)]
                Variable  [x]
                TokenNode  [) [3...4)]
            TokenNode  [) [4...5)]
";
            ParserTest.VerifyParse(expected, "((x))");
        }

        [TestMethod]
        public void ParseSimpleExpressions05()
        {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [((x))+1]
        Expression  [((x))+1]
            TokenOperator  [+ [5...6)]
                Expression  [((x))]
                    TokenNode  [( [0...1)]
                    Expression  [(x)]
                        TokenNode  [( [1...2)]
                        Variable  [x]
                        TokenNode  [) [3...4)]
                    TokenNode  [) [4...5)]
                TokenNode  [+ [5...6)]
                NumericalValue  [1 [6...7)]
";
            ParserTest.VerifyParse(expected, "((x))+1");
        }

        [TestMethod]
        public void ParseSimpleExpressions06()
        {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [(x)]
        Expression  [(x)]
            TokenNode  [( [0...1)]
            Variable  [x]
            TokenNode  [) [2...3)]
";
            ParserTest.VerifyParse(expected, "(x)");
        }

        [TestMethod]
        public void ParseSimpleExpressions07()
        {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [x]
        Expression  [x]
            Variable  [x]
";
            ParserTest.VerifyParse(expected, "x");
        }

        [TestMethod]
        public void ParseSimpleExpressions08()
        {
            string expected =
@"GlobalScope  [Global]
";
            ParserTest.VerifyParse(expected, "");
        }

        [TestMethod]
        public void ParseSimpleExpressions09()
        {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [((c+1)/2.0)]
        Expression  [((c+1)/2.0)]
            TokenNode  [( [0...1)]
            TokenOperator  [/ [6...7)]
                Expression  [(c+1)]
                    TokenNode  [( [1...2)]
                    TokenOperator  [+ [3...4)]
                        Variable  [c]
                        TokenNode  [+ [3...4)]
                        NumericalValue  [1 [4...5)]
                    TokenNode  [) [5...6)]
                TokenNode  [/ [6...7)]
                NumericalValue  [2.0 [7...10)]
            TokenNode  [) [10...11)]
";
            ParserTest.VerifyParse(expected, "((c+1)/2.0)");
        }

        [TestMethod]
        public void ParseSimpleExpressions10()
        {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [FALSE & TRUE]
        Expression  [FALSE & TRUE]
            TokenOperator  [& [6...7)]
                LogicalValue  [FALSE [0...5)]
                TokenNode  [& [6...7)]
                LogicalValue  [TRUE [8...12)]
";
            ParserTest.VerifyParse(expected, "FALSE & TRUE");
        }
    }
}
