// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.R.Core.Test.Utility;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.R.Core.Test.Parser {
    [ExcludeFromCodeCoverage]
    public class ParseFunctionDefinitionTest {
        [Test]
        [Category.R.Parser]
        public void ParseFunctionDefinitionTest01() {
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
                        ExpressionStatement  [return(1)]
                            Expression  [return(1)]
                                FunctionCall  [19...28)
                                    Variable  [return]
                                    TokenNode  [( [25...26)]
                                    ArgumentList  [26...27)
                                        ExpressionArgument  [26...27)
                                            Expression  [1]
                                                NumericalValue  [1 [26...27)]
                                    TokenNode  [) [27...28)]
                        TokenNode  [} [29...30)]
";
            ParserTest.VerifyParse(expected, "x <- function(a) { return(1) }");
        }

        [Test]
        [Category.R.Parser]
        public void ParseFunctionDefinitionTest02() {
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
                        ExpressionStatement  [return(1)]
                            Expression  [return(1)]
                                FunctionCall  [17...26)
                                    Variable  [return]
                                    TokenNode  [( [23...24)]
                                    ArgumentList  [24...25)
                                        ExpressionArgument  [24...25)
                                            Expression  [1]
                                                NumericalValue  [1 [24...25)]
                                    TokenNode  [) [25...26)]
";
            ParserTest.VerifyParse(expected, "x <- function(a) return(1)");
        }

        [Test]
        [Category.R.Parser]
        public void ParseFunctionDefinitionTest03() {
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

        [Test]
        [Category.R.Parser]
        public void ParseFunctionDefinitionTest04() {
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

        [Test]
        [Category.R.Parser]
        public void ParseFunctionDefinitionTest05() {
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

        [Test]
        [Category.R.Parser]
        public void ParseFunctionDefinitionTest06() {
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

        [Test]
        [Category.R.Parser]
        public void ParseFunctionDefinitionTest07() {
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

        [Test]
        [Category.R.Parser]
        public void ParseFunctionDefinitionTest08() {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [a((function(x) vector(length = x))(x), 1)]
        Expression  [a((function(x) vector(length = x))(x), 1)]
            FunctionCall  [0...41)
                Variable  [a]
                TokenNode  [( [1...2)]
                ArgumentList  [2...40)
                    ExpressionArgument  [2...38)
                        Expression  [(function(x) vector(length = x))(x)]
                            FunctionCall  [2...37)
                                Group  [2...34)
                                    TokenNode  [( [2...3)]
                                    Expression  [function(x) vector(length = x)]
                                        FunctionDefinition  [3...33)
                                            TokenNode  [function [3...11)]
                                            TokenNode  [( [11...12)]
                                            ArgumentList  [12...13)
                                                ExpressionArgument  [12...13)
                                                    Expression  [x]
                                                        Variable  [x]
                                            TokenNode  [) [13...14)]
                                            SimpleScope  [15...33)
                                                ExpressionStatement  [vector(length = x)]
                                                    Expression  [vector(length = x)]
                                                        FunctionCall  [15...33)
                                                            Variable  [vector]
                                                            TokenNode  [( [21...22)]
                                                            ArgumentList  [22...32)
                                                                NamedArgument  [22...32)
                                                                    TokenNode  [length [22...28)]
                                                                    TokenNode  [= [29...30)]
                                                                    Expression  [x]
                                                                        Variable  [x]
                                                            TokenNode  [) [32...33)]
                                    TokenNode  [) [33...34)]
                                TokenNode  [( [34...35)]
                                ArgumentList  [35...36)
                                    ExpressionArgument  [35...36)
                                        Expression  [x]
                                            Variable  [x]
                                TokenNode  [) [36...37)]
                        TokenNode  [, [37...38)]
                    ExpressionArgument  [39...40)
                        Expression  [1]
                            NumericalValue  [1 [39...40)]
                TokenNode  [) [40...41)]
";
            ParserTest.VerifyParse(expected, "a((function(x) vector(length = x))(x), 1))");
        }
    }
}
