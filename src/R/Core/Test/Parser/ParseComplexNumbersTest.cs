using System.Diagnostics.CodeAnalysis;
using Microsoft.Languages.Core.Test.Utility;
using Microsoft.R.Core.Test.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.R.Core.Test.Parser
{
    [ExcludeFromCodeCoverage]
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
            TokenOperator  [/ [6...7)]
                Group  [0...6)
                    TokenNode  [( [0...1)]
                    Expression  [1i+2]
                        TokenOperator  [+ [3...4)]
                            ComplexValue  [1i [1...3)]
                            TokenNode  [+ [3...4)]
                            NumericalValue  [2 [4...5)]
                    TokenNode  [) [5...6)]
                TokenNode  [/ [6...7)]
                Group  [7...16)
                    TokenNode  [( [7...8)]
                    Expression  [1e2+.1i]
                        ComplexValue  [1e2+.1i [8...15)]
                    TokenNode  [) [15...16)]
";
            ParserTest.VerifyParse(expected, "(1i+2)/(1e2+.1i)");
        }
    }
}
