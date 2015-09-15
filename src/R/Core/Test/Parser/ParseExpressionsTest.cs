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
    }
}
