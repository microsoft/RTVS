using Microsoft.Languages.Core.Test.Utility;
using Microsoft.R.Core.Test.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.R.Core.Test.Parser
{
    [TestClass]
    public class ParseExponentTest : UnitTestBase
    {
        [TestMethod]
        public void ParseExponentTest1()
        {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [a^b]
        Expression  [a^b]
            TokenOperator  [^ [1...2]]
                Variable  [a]
                TokenNode  [^ [1...2]]
                Variable  [b]
";
            ParserTest.VerifyParse(expected, "a^b");
        }

        [TestMethod]
        public void ParseExponentTest2()
        {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [a^b^c]
        Expression  [a^b^c]
            TokenOperator  [^ [1...2]]
                Variable  [a]
                TokenNode  [^ [1...2]]
                TokenOperator  [^ [3...4]]
                    Variable  [b]
                    TokenNode  [^ [3...4]]
                    Variable  [c]
";
            ParserTest.VerifyParse(expected, "a^b^c");
        }

        [TestMethod]
        public void ParseExponentTest3()
        {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [a^(b+c)]
        Expression  [a^(b+c)]
            TokenOperator  [^ [1...2]]
                Variable  [a]
                TokenNode  [^ [1...2]]
                Expression  [(b+c)]
                    TokenNode  [( [2...3]]
                    TokenOperator  [+ [4...5]]
                        Variable  [b]
                        TokenNode  [+ [4...5]]
                        Variable  [c]
                    TokenNode  [) [6...7]]
";
            ParserTest.VerifyParse(expected, "a^(b+c)");
        }

        [TestMethod]
        public void ParseExponentTest4()
        {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [a^b::c]
        Expression  [a^b::c]
            TokenOperator  [^ [1...2]]
                Variable  [a]
                TokenNode  [^ [1...2]]
                TokenOperator  [:: [3...5]]
                    Variable  [b]
                    TokenNode  [:: [3...5]]
                    Variable  [c]
";
            ParserTest.VerifyParse(expected, "a^b::c");
        }
    }
}
