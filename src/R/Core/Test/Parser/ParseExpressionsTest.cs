using System.Diagnostics.CodeAnalysis;
using Microsoft.Languages.Core.Test.Utility;
using Microsoft.R.Core.Test.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.R.Core.Test.Parser
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class ParseExpressionsTest : UnitTestBase
    {
        [TestMethod]
        public void ParseExpressions01()
        {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [a <-(grepl('^check', install) || R_check_use_install_log)]
        Expression  [a <-(grepl('^check', install) || R_check_use_install_log)]
            TokenOperator  [<- [2...4)]
                Variable  [a]
                TokenNode  [<- [2...4)]
                Group  [4...57)
                    TokenNode  [( [4...5)]
                    Expression  [grepl('^check', install) || R_check_use_install_log]
                        TokenOperator  [|| [30...32)]
                            FunctionCall  [5...29)
                                Variable  [grepl]
                                TokenNode  [( [10...11)]
                                ArgumentList  [11...28)
                                    ExpressionArgument  [11...20)
                                        Expression  ['^check']
                                            StringValue  ['^check' [11...19)]
                                        TokenNode  [, [19...20)]
                                    ExpressionArgument  [21...28)
                                        Expression  [install]
                                            Variable  [install]
                                TokenNode  [) [28...29)]
                            TokenNode  [|| [30...32)]
                            Variable  [R_check_use_install_log]
                    TokenNode  [) [56...57)]
";
            ParserTest.VerifyParse(expected, @"a <-(grepl('^check', install) || R_check_use_install_log)");
        }

        [TestMethod]
        public void ParseListExpression01()
        {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [fitted.zeros <- xzero * z$coefficients]
        Expression  [fitted.zeros <- xzero * z$coefficients]
            TokenOperator  [<- [13...15)]
                Variable  [fitted.zeros]
                TokenNode  [<- [13...15)]
                TokenOperator  [* [22...23)]
                    Variable  [xzero]
                    TokenNode  [* [22...23)]
                    TokenOperator  [$ [25...26)]
                        Variable  [z]
                        TokenNode  [$ [25...26)]
                        Variable  [coefficients]
";
            ParserTest.VerifyParse(expected, @"fitted.zeros <- xzero * z$coefficients");
        }
    }
}
