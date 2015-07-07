using Microsoft.Languages.Core.Test.Utility;
using Microsoft.R.Core.Test.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.R.Core.Test.Tokens
{
    [TestClass]
    public class ParseFunctionsTest : UnitTestBase
    {
        [TestMethod]
        public void ParseFunctionsTest1()
        {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [a(1)]
        Expression  [a(1)]
            FunctionCall  [FunctionCall]
                Variable  [a]
                TokenNode  [( [1...2]]
                ArgumentList  [ArgumentList]
                    ExpressionArgument  [ExpressionArgument]
                        Expression  [1]
                            NumericalValue  [1 [2...3]]
                TokenNode  [) [3...4]]
";
            ParserTest.VerifyParse(expected, "a(1)");
        }

        [TestMethod]
        public void ParseFunctionsTest2()
        {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [a(1,2)]
        Expression  [a(1,2)]
            FunctionCall  [FunctionCall]
                Variable  [a]
                TokenNode  [( [1...2]]
                ArgumentList  [ArgumentList]
                    ExpressionArgument  [ExpressionArgument]
                        Expression  [1]
                            NumericalValue  [1 [2...3]]
                        TokenNode  [, [3...4]]
                    ExpressionArgument  [ExpressionArgument]
                        Expression  [2]
                            NumericalValue  [2 [4...5]]
                TokenNode  [) [5...6]]
";
            ParserTest.VerifyParse(expected, "a(1,2)");
        }

        [TestMethod]
        public void ParseFunctionsTest3()
        {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [x(a, b=NA, c=NULL, ...)]
        Expression  [x(a, b=NA, c=NULL, ...)]
            FunctionCall  [FunctionCall]
                Variable  [x]
                TokenNode  [( [1...2]]
                ArgumentList  [ArgumentList]
                    ExpressionArgument  [ExpressionArgument]
                        Expression  [a]
                            Variable  [a]
                        TokenNode  [, [3...4]]
                    NamedArgument  [NamedArgument]
                        TokenNode  [b [5...6]]
                        TokenNode  [= [6...7]]
                        Expression  [NA]
                            MissingValue  [NA [7...9]]
                        TokenNode  [, [9...10]]
                    NamedArgument  [NamedArgument]
                        TokenNode  [c [11...12]]
                        TokenNode  [= [12...13]]
                        Expression  [NULL]
                            NullValue  [NULL [13...17]]
                        TokenNode  [, [17...18]]
                    EllipsisArgument  [...]
                TokenNode  [) [22...23]]
";
            ParserTest.VerifyParse(expected, "x(a, b=NA, c=NULL, ...)");
        }
    }
}
