// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.R.Core.Test.Utility;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.R.Core.Test.Parser {
    [ExcludeFromCodeCoverage]    
    public class ParseLambdaTest {
        [Test]
        [Category.R.Parser]
        public void ParseLambdaTest01() {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [a <- { 1 }]
        Expression  [a <- { 1 }]
            TokenOperator  [<- [2...4)]
                Variable  [a]
                TokenNode  [<- [2...4)]
                Lambda  [_Anonymous_]
                    TokenNode  [{ [5...6)]
                    ExpressionStatement  [1]
                        Expression  [1]
                            NumericalValue  [1 [7...8)]
                    TokenNode  [} [9...10)]
";
            ParserTest.VerifyParse(expected, "a <- { 1 }");
        }

        [Test]
        [Category.R.Parser]
        public void ParseLambdaTest02() {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [a <- b + { x <- c + d; x}]
        Expression  [a <- b + { x <- c + d; x}]
            TokenOperator  [<- [2...4)]
                Variable  [a]
                TokenNode  [<- [2...4)]
                TokenOperator  [+ [7...8)]
                    Variable  [b]
                    TokenNode  [+ [7...8)]
                    Lambda  [_Anonymous_]
                        TokenNode  [{ [9...10)]
                        ExpressionStatement  [x <- c + d]
                            Expression  [x <- c + d]
                                TokenOperator  [<- [13...15)]
                                    Variable  [x]
                                    TokenNode  [<- [13...15)]
                                    TokenOperator  [+ [18...19)]
                                        Variable  [c]
                                        TokenNode  [+ [18...19)]
                                        Variable  [d]
                        EmptyStatement  [21...22)
                            TokenNode  [; [21...22)]
                        ExpressionStatement  [x]
                            Expression  [x]
                                Variable  [x]
                        TokenNode  [} [24...25)]
";
            ParserTest.VerifyParse(expected, "a <- b + { x <- c + d; x}");
        }

        [Test]
        [Category.R.Parser]
        public void ParseLambdaTest03() {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [a({x <- 1; x})]
        Expression  [a({x <- 1; x})]
            FunctionCall  [0...14)
                Variable  [a]
                TokenNode  [( [1...2)]
                ArgumentList  [2...13)
                    ExpressionArgument  [2...13)
                        Expression  [{x <- 1; x}]
                            Lambda  [_Anonymous_]
                                TokenNode  [{ [2...3)]
                                ExpressionStatement  [x <- 1]
                                    Expression  [x <- 1]
                                        TokenOperator  [<- [5...7)]
                                            Variable  [x]
                                            TokenNode  [<- [5...7)]
                                            NumericalValue  [1 [8...9)]
                                EmptyStatement  [9...10)
                                    TokenNode  [; [9...10)]
                                ExpressionStatement  [x]
                                    Expression  [x]
                                        Variable  [x]
                                TokenNode  [} [12...13)]
                TokenNode  [) [13...14)]
";
            ParserTest.VerifyParse(expected, "a({x <- 1; x})");
        }
    }
}
