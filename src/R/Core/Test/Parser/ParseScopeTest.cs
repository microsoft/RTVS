using System.Diagnostics.CodeAnalysis;
using Microsoft.Languages.Core.Test.Utility;
using Microsoft.R.Core.Test.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.R.Core.Test.Parser
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class ParseScopeTest : UnitTestBase
    {
        [ExcludeFromCodeCoverage]
        [TestMethod]
        public void ParseScopeTest1()
        {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [x <- as.matrix(x)]
        Expression  [x <- as.matrix(x)]
            TokenOperator  [<- [2...4]]
                Variable  [x]
                TokenNode  [<- [2...4]]
                FunctionCall  [FunctionCall]
                    Variable  [as.matrix]
                    TokenNode  [( [14...15]]
                    ArgumentList  [ArgumentList]
                        ExpressionArgument  [ExpressionArgument]
                            Expression  [x]
                                Variable  [x]
                    TokenNode  [) [16...17]]
    ExpressionStatement  [y <- as.matrix(y)]
        Expression  [y <- as.matrix(y)]
            TokenOperator  [<- [22...24]]
                Variable  [y]
                TokenNode  [<- [22...24]]
                FunctionCall  [FunctionCall]
                    Variable  [as.matrix]
                    TokenNode  [( [34...35]]
                    ArgumentList  [ArgumentList]
                        ExpressionArgument  [ExpressionArgument]
                            Expression  [y]
                                Variable  [y]
                    TokenNode  [) [36...37]]
";
            ParserTest.VerifyParse(expected, "x <- as.matrix(x) \n y <- as.matrix(y)");
        }

        [TestMethod]
        public void ParseScopeTest2()
        {
            string expected =
@"
CloseCurlyBraceExpected AfterToken [2...3)
";
            ParserTest.VerifyParse(expected, "{{}");
        }

        [TestMethod]
        public void ParseScopeTest3()
        {
            string expected =
@"
CloseCurlyBraceExpected AfterToken [0...1)
";
            ParserTest.VerifyParse(expected, "{");
        }
    }
}
