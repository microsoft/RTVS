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
            TokenOperator  [<- [2...4]]
                Variable  [x]
                TokenNode  [<- [2...4]]
                FunctionDefinition  [FunctionDefinition]
                    TokenNode  [function [5...13]]
                    TokenNode  [( [13...14]]
                    ArgumentList  [ArgumentList]
                        ExpressionArgument  [ExpressionArgument]
                            Expression  [a]
                                Variable  [a]
                    TokenNode  [) [15...16]]
                    Scope  []
                        TokenNode  [{ [17...18]]
                        KeywordExpressionStatement  []
                            TokenNode  [return [19...25]]
                            TokenNode  [( [25...26]]
                            Expression  [1]
                                NumericalValue  [1 [26...27]]
                            TokenNode  [) [27...28]]
                        TokenNode  [} [29...30]]
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
            TokenOperator  [<- [2...4]]
                Variable  [x]
                TokenNode  [<- [2...4]]
                FunctionDefinition  [FunctionDefinition]
                    TokenNode  [function [5...13]]
                    TokenNode  [( [13...14]]
                    ArgumentList  [ArgumentList]
                        ExpressionArgument  [ExpressionArgument]
                            Expression  [a]
                                Variable  [a]
                    TokenNode  [) [15...16]]
                    SimpleScope  [SimpleScope]
                        KeywordExpressionStatement  []
                            TokenNode  [return [17...23]]
                            TokenNode  [( [23...24]]
                            Expression  [1]
                                NumericalValue  [1 [24...25]]
                            TokenNode  [) [25...26]]
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
            TokenOperator  [<- [2...4]]
                Variable  [x]
                TokenNode  [<- [2...4]]
                FunctionDefinition  [FunctionDefinition]
                    TokenNode  [function [5...13]]
                    TokenNode  [( [13...14]]
                    ArgumentList  [ArgumentList]
                        MissingArgument  [{Missing}]
                            TokenNode  [, [17...18]]
                        MissingArgument  [{Missing}]
                            TokenNode  [, [22...23]]
                        ExpressionArgument  [ExpressionArgument]
                            Expression  [e]
                                Variable  [e]
                    TokenNode  [) [25...26]]
                    Scope  []
                        TokenNode  [{ [27...28]]
                        TokenNode  [} [29...30]]

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
            TokenOperator  [<- [2...4]]
                Variable  [x]
                TokenNode  [<- [2...4]]
                FunctionDefinition  [FunctionDefinition]
                    TokenNode  [function [5...13]]
                    TokenNode  [( [13...14]]
                    TokenNode  [) [17...18]]
                    Scope  []
                        TokenNode  [{ [19...20]]
                        TokenNode  [} [21...22]]

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
            TokenOperator  [<- [2...4]]
                Variable  [x]
                TokenNode  [<- [2...4]]
                FunctionDefinition  [FunctionDefinition]
                    TokenNode  [function [5...13]]
                    TokenNode  [( [13...14]]
                    ArgumentList  [ArgumentList]
                        ExpressionArgument  [ExpressionArgument]
                            Expression  [a]
                                Variable  [a]
                            TokenNode  [, [15...16]]
                        ExpressionArgument  [ExpressionArgument]
                            Expression  [b]
                                Variable  [b]
                    TokenNode  [) [18...19]]
                    SimpleScope  [SimpleScope]
                        ExpressionStatement  [a + b]
                            Expression  [a + b]
                                TokenOperator  [+ [22...23]]
                                    Variable  [a]
                                    TokenNode  [+ [22...23]]
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
            TokenOperator  [<- [2...4]]
                Variable  [x]
                TokenNode  [<- [2...4]]
                FunctionDefinition  [FunctionDefinition]
                    TokenNode  [function [5...13]]
                    TokenNode  [( [13...14]]
                    ArgumentList  [ArgumentList]
                        ExpressionArgument  [ExpressionArgument]
                            Expression  [a]
                                Variable  [a]
                    TokenNode  [) [15...16]]
                    SimpleScope  [SimpleScope]
                        ExpressionStatement  [-a]
                            Expression  [-a]
                                TokenOperator  [- [17...18]]
                                    TokenNode  [- [17...18]]
                                    Variable  [a]
";
            ParserTest.VerifyParse(expected, "x <- function(a) -a");
        }
    }
}
