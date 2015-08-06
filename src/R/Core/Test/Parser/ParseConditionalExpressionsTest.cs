using Microsoft.Languages.Core.Test.Utility;
using Microsoft.R.Core.Test.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.R.Core.Test.Parser
{
    [TestClass]
    public class ParseConditionalExpressionsTest : UnitTestBase
    {
        [TestMethod]
        public void ParseConditionalExpressionsTest1()
        {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [dimy[2L] == 1 && is.null(yname)]
        Expression  [dimy[2L] == 1 && is.null(yname)]
            TokenOperator  [&& [14...16]]
                TokenOperator  [== [9...11]]
                    Indexer  [Indexer]
                        Variable  [dimy]
                        TokenNode  [[ [4...5]]
                        ArgumentList  [ArgumentList]
                            ExpressionArgument  [ExpressionArgument]
                                Expression  [2L]
                                    NumericalValue  [2L [5...7]]
                        TokenNode  [] [7...8]]
                    TokenNode  [== [9...11]]
                    NumericalValue  [1 [12...13]]
                TokenNode  [&& [14...16]]
                FunctionCall  [FunctionCall]
                    Variable  [is.null]
                    TokenNode  [( [24...25]]
                    ArgumentList  [ArgumentList]
                        ExpressionArgument  [ExpressionArgument]
                            Expression  [yname]
                                Variable  [yname]
                    TokenNode  [) [30...31]]
";
            ParserTest.VerifyParse(expected, "dimy[2L] == 1 && is.null(yname)");
        }
    }
}
