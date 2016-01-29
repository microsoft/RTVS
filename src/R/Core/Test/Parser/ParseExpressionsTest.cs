using System.Diagnostics.CodeAnalysis;
using Microsoft.R.Core.Test.Utility;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.R.Core.Test.Parser {
    [ExcludeFromCodeCoverage]
    public class ParseExpressionsTest {
        [Test]
        [Category.R.Parser]
        public void ParseExpressions01() {
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

        [Test]
        [Category.R.Parser]
        public void ParseListExpression01() {
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

        [Test]
        [Category.R.Parser]
        public void ParseExpressionSequence01() {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [a <- 1*b]
        Expression  [a <- 1*b]
            TokenOperator  [<- [2...4)]
                Variable  [a]
                TokenNode  [<- [2...4)]
                TokenOperator  [* [6...7)]
                    NumericalValue  [1 [5...6)]
                    TokenNode  [* [6...7)]
                    Variable  [b]
    ExpressionStatement  [(c+1)]
        Expression  [(c+1)]
            Group  [12...17)
                TokenNode  [( [12...13)]
                Expression  [c+1]
                    TokenOperator  [+ [14...15)]
                        Variable  [c]
                        TokenNode  [+ [14...15)]
                        NumericalValue  [1 [15...16)]
                TokenNode  [) [16...17)]
";

            string content =
@"a <- 1*b
  (c+1)";
            ParserTest.VerifyParse(expected, content);
        }

        [Test]
        [Category.R.Parser]
        public void ParseExpressionSequence02() {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [a <- 1*b[1]]
        Expression  [a <- 1*b[1]]
            TokenOperator  [<- [2...4)]
                Variable  [a]
                TokenNode  [<- [2...4)]
                TokenOperator  [* [6...7)]
                    NumericalValue  [1 [5...6)]
                    TokenNode  [* [6...7)]
                    Indexer  [7...11)
                        Variable  [b]
                        TokenNode  [[ [8...9)]
                        ArgumentList  [9...10)
                            ExpressionArgument  [9...10)
                                Expression  [1]
                                    NumericalValue  [1 [9...10)]
                        TokenNode  [] [10...11)]
    ExpressionStatement  [(c+1)]
        Expression  [(c+1)]
            Group  [15...20)
                TokenNode  [( [15...16)]
                Expression  [c+1]
                    TokenOperator  [+ [17...18)]
                        Variable  [c]
                        TokenNode  [+ [17...18)]
                        NumericalValue  [1 [18...19)]
                TokenNode  [) [19...20)]
";

            string content =
@"a <- 1*b[1]
  (c+1)";
            ParserTest.VerifyParse(expected, content);
        }

        [Test]
        [Category.R.Parser]
        public void ParseExpressionSequence03() {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [a <- 1*b[[1]]]
        Expression  [a <- 1*b[[1]]]
            TokenOperator  [<- [2...4)]
                Variable  [a]
                TokenNode  [<- [2...4)]
                TokenOperator  [* [6...7)]
                    NumericalValue  [1 [5...6)]
                    TokenNode  [* [6...7)]
                    Indexer  [7...13)
                        Variable  [b]
                        TokenNode  [[[ [8...10)]
                        ArgumentList  [10...11)
                            ExpressionArgument  [10...11)
                                Expression  [1]
                                    NumericalValue  [1 [10...11)]
                        TokenNode  []] [11...13)]
    ExpressionStatement  [(c+1)]
        Expression  [(c+1)]
            Group  [17...22)
                TokenNode  [( [17...18)]
                Expression  [c+1]
                    TokenOperator  [+ [19...20)]
                        Variable  [c]
                        TokenNode  [+ [19...20)]
                        NumericalValue  [1 [20...21)]
                TokenNode  [) [21...22)]
";

            string content =
@"a <- 1*b[[1]]
  (c+1)";
            ParserTest.VerifyParse(expected, content);
        }

        [Test]
        [Category.R.Parser]
        public void ParseMultipleTilde() {
            string expected = 
@"GlobalScope  [Global]
    ExpressionStatement  [x ~ ~ ~ y]
        Expression  [x ~ ~ ~ y]
            TokenOperator  [~ [2...3)]
                Variable  [x]
                TokenNode  [~ [2...3)]
                Variable  [y]
";
            string content = "x ~ ~ ~ y";

            ParserTest.VerifyParse(expected, content);
        }
    }
}
