using System.Diagnostics.CodeAnalysis;
using Microsoft.Common.Core.Test.Utility;
using Microsoft.R.Core.Test.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.R.Core.Test.Parser {
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class ParseConditionalExpressionsTest : UnitTestBase {
        [TestMethod]
        [TestCategory("R.Parser")]
        public void ParseConditionalExpressionsTest1() {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [dimy[2L] == 1 && is.null(yname)]
        Expression  [dimy[2L] == 1 && is.null(yname)]
            TokenOperator  [&& [14...16)]
                TokenOperator  [== [9...11)]
                    Indexer  [0...8)
                        Variable  [dimy]
                        TokenNode  [[ [4...5)]
                        ArgumentList  [5...7)
                            ExpressionArgument  [5...7)
                                Expression  [2L]
                                    NumericalValue  [2L [5...7)]
                        TokenNode  [] [7...8)]
                    TokenNode  [== [9...11)]
                    NumericalValue  [1 [12...13)]
                TokenNode  [&& [14...16)]
                FunctionCall  [17...31)
                    Variable  [is.null]
                    TokenNode  [( [24...25)]
                    ArgumentList  [25...30)
                        ExpressionArgument  [25...30)
                            Expression  [yname]
                                Variable  [yname]
                    TokenNode  [) [30...31)]
";
            ParserTest.VerifyParse(expected, "dimy[2L] == 1 && is.null(yname)");
        }
    }
}
