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
            TokenOperator  [<- [2...4)]
                Variable  [x]
                TokenNode  [<- [2...4)]
                FunctionCall  [5...17)
                    Variable  [as.matrix]
                    TokenNode  [( [14...15)]
                    ArgumentList  [15...16)
                        ExpressionArgument  [15...16)
                            Expression  [x]
                                Variable  [x]
                    TokenNode  [) [16...17)]
    ExpressionStatement  [y <- as.matrix(y)]
        Expression  [y <- as.matrix(y)]
            TokenOperator  [<- [22...24)]
                Variable  [y]
                TokenNode  [<- [22...24)]
                FunctionCall  [25...37)
                    Variable  [as.matrix]
                    TokenNode  [( [34...35)]
                    ArgumentList  [35...36)
                        ExpressionArgument  [35...36)
                            Expression  [y]
                                Variable  [y]
                    TokenNode  [) [36...37)]
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
