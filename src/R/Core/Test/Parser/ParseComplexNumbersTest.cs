using Microsoft.Languages.Core.Test.Utility;
using Microsoft.R.Core.Test.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.R.Core.Test.Tokens
{
    [TestClass]
    public class ParseComplexNumbersTest : UnitTestBase
    {
        [TestMethod]
        public void ParseComplexNumbersTest1()
        {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [(1i+2)/(1e2+.1i)]
        Expression  [(1i+2)/(1e2+.1i)]
            TokenOperator  [/ [6...7]]
                Expression  [(1i+2)]
                    TokenNode  [( [0...1]]
                    TokenOperator  [+ [3...4]]
                        ComplexValue  [1i [1...3]]
                        TokenNode  [+ [3...4]]
                        NumericalValue  [2 [4...5]]
                    TokenNode  [) [5...6]]
                TokenNode  [/ [6...7]]
                Expression  [(1e2+.1i)]
                    TokenNode  [( [7...8]]
                    ComplexValue  [1e2+.1i [8...15]]
                    TokenNode  [) [15...16]]
";
            ParserTest.VerifyParse(expected, "(1i+2)/(1e2+.1i)");
        }
    }
}
