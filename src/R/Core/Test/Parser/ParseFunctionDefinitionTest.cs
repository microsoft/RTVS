using System.Diagnostics.CodeAnalysis;
using Microsoft.Languages.Core.Test.Utility;
using Microsoft.R.Core.Test.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.R.Core.Test.Parser
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class ParseFunctionDefinitionTest : UnitTestBase
    {
        [TestMethod]
        public void ParseFunctionDefinitionTest01()
        {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [x <- function(a) { return(1) }]
        Expression  [x <- function(a) { return(1) }]
            TokenOperator  [<- [2...4)]
                Variable  [x]
                TokenNode  [<- [2...4)]
                FunctionDefinition  [5...30)
                    TokenNode  [function [5...13)]
                    TokenNode  [( [13...14)]
                    ArgumentList  [14...15)
                        ExpressionArgument  [14...15)
                            Expression  [a]
                                Variable  [a]
                    TokenNode  [) [15...16)]
                    Scope  []
                        TokenNode  [{ [17...18)]
                        KeywordExpressionStatement  []
                            TokenNode  [return [19...25)]
                            TokenNode  [( [25...26)]
                            Expression  [1]
                                NumericalValue  [1 [26...27)]
                            TokenNode  [) [27...28)]
                        TokenNode  [} [29...30)]
";
            ParserTest.VerifyParse(expected, "x <- function(a) { return(1) }");
        }

        [TestMethod]
        public void ParseFunctionDefinitionTest02()
        {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [x <- function(a) return(1)]
        Expression  [x <- function(a) return(1)]
            TokenOperator  [<- [2...4)]
                Variable  [x]
                TokenNode  [<- [2...4)]
                FunctionDefinition  [5...26)
                    TokenNode  [function [5...13)]
                    TokenNode  [( [13...14)]
                    ArgumentList  [14...15)
                        ExpressionArgument  [14...15)
                            Expression  [a]
                                Variable  [a]
                    TokenNode  [) [15...16)]
                    SimpleScope  [17...26)
                        KeywordExpressionStatement  []
                            TokenNode  [return [17...23)]
                            TokenNode  [( [23...24)]
                            Expression  [1]
                                NumericalValue  [1 [24...25)]
                            TokenNode  [) [25...26)]
";
            ParserTest.VerifyParse(expected, "x <- function(a) return(1)");
        }

        [TestMethod]
        public void ParseFunctionDefinitionTest03()
        {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [x <- function(a b, c d, e) { }]
        Expression  [x <- function(a b, c d, e) { }]
            TokenOperator  [<- [2...4)]
                Variable  [x]
                TokenNode  [<- [2...4)]
                FunctionDefinition  [5...30)
                    TokenNode  [function [5...13)]
                    TokenNode  [( [13...14)]
                    ArgumentList  [14...25)
                        ErrorArgument  [14...18)
                            TokenNode  [a [14...15)]
                            TokenNode  [b [16...17)]
                            TokenNode  [, [17...18)]
                        ErrorArgument  [19...23)
                            TokenNode  [c [19...20)]
                            TokenNode  [d [21...22)]
                            TokenNode  [, [22...23)]
                        ExpressionArgument  [24...25)
                            Expression  [e]
                                Variable  [e]
                    TokenNode  [) [25...26)]
                    Scope  []
                        TokenNode  [{ [27...28)]
                        TokenNode  [} [29...30)]

OperatorExpected Token [16...17)
OperatorExpected Token [21...22)
";
            ParserTest.VerifyParse(expected, "x <- function(a b, c d, e) { }");
        }

        [TestMethod]
        public void ParseFunctionDefinitionTest04()
        {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [x <- function(a b) { }]
        Expression  [x <- function(a b) { }]
            TokenOperator  [<- [2...4)]
                Variable  [x]
                TokenNode  [<- [2...4)]
                FunctionDefinition  [5...22)
                    TokenNode  [function [5...13)]
                    TokenNode  [( [13...14)]
                    ArgumentList  [14...17)
                        ErrorArgument  [14...17)
                            TokenNode  [a [14...15)]
                            TokenNode  [b [16...17)]
                    TokenNode  [) [17...18)]
                    Scope  []
                        TokenNode  [{ [19...20)]
                        TokenNode  [} [21...22)]

OperatorExpected Token [16...17)
";
            ParserTest.VerifyParse(expected, "x <- function(a b) { }");
        }

        [TestMethod]
        public void ParseFunctionDefinitionTest05()
        {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [x <- function(a, b) a + b]
        Expression  [x <- function(a, b) a + b]
            TokenOperator  [<- [2...4)]
                Variable  [x]
                TokenNode  [<- [2...4)]
                FunctionDefinition  [5...25)
                    TokenNode  [function [5...13)]
                    TokenNode  [( [13...14)]
                    ArgumentList  [14...18)
                        ExpressionArgument  [14...16)
                            Expression  [a]
                                Variable  [a]
                            TokenNode  [, [15...16)]
                        ExpressionArgument  [17...18)
                            Expression  [b]
                                Variable  [b]
                    TokenNode  [) [18...19)]
                    SimpleScope  [20...25)
                        ExpressionStatement  [a + b]
                            Expression  [a + b]
                                TokenOperator  [+ [22...23)]
                                    Variable  [a]
                                    TokenNode  [+ [22...23)]
                                    Variable  [b]
";
            ParserTest.VerifyParse(expected, "x <- function(a, b) a + b");
        }

        [TestMethod]
        public void ParseFunctionDefinitionTest06()
        {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [x <- function(a) -a]
        Expression  [x <- function(a) -a]
            TokenOperator  [<- [2...4)]
                Variable  [x]
                TokenNode  [<- [2...4)]
                FunctionDefinition  [5...19)
                    TokenNode  [function [5...13)]
                    TokenNode  [( [13...14)]
                    ArgumentList  [14...15)
                        ExpressionArgument  [14...15)
                            Expression  [a]
                                Variable  [a]
                    TokenNode  [) [15...16)]
                    SimpleScope  [17...19)
                        ExpressionStatement  [-a]
                            Expression  [-a]
                                TokenOperator  [- [17...18)]
                                    TokenNode  [- [17...18)]
                                    Variable  [a]
";
            ParserTest.VerifyParse(expected, "x <- function(a) -a");
        }

        [TestMethod]
        public void ParseFunctionDefinitionTest07()
        {
            string expected =
@"GlobalScope  [Global]
    FunctionStatement  [0...14)
        TokenNode  [function [0...8)]
        TokenNode  [( [8...9)]
        ArgumentList  [9...10)
            ExpressionArgument  [9...10)
                Expression  [a]
                    Variable  [a]
        TokenNode  [) [10...11)]
        Scope  []
            TokenNode  [{ [12...13)]
            TokenNode  [} [13...14)]
";
            ParserTest.VerifyParse(expected, "function(a) {}");
        }
    }
}
