// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;

using Microsoft.R.Core.Test.Utility;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.R.Core.Test.Parser {
    [ExcludeFromCodeCoverage]
    public class Assignments {
        [Test]
        [Category.R.Parser]
        public void Assignments01() {
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
";
            ParserTest.VerifyParse(expected, "x <- as.matrix(x)");
        }

        [Test]
        [Category.R.Parser]
        public void Assignments02() {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [as.matrix(x) -> x]
        Expression  [as.matrix(x) -> x]
            TokenOperator  [-> [13...15)]
                FunctionCall  [0...12)
                    Variable  [as.matrix]
                    TokenNode  [( [9...10)]
                    ArgumentList  [10...11)
                        ExpressionArgument  [10...11)
                            Expression  [x]
                                Variable  [x]
                    TokenNode  [) [11...12)]
                TokenNode  [-> [13...15)]
                Variable  [x]
";
            ParserTest.VerifyParse(expected, "as.matrix(x) -> x");
        }

        [Test]
        [Category.R.Parser]
        public void Assignments03() {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [a <- b <- c <- 0]
        Expression  [a <- b <- c <- 0]
            TokenOperator  [<- [2...4)]
                Variable  [a]
                TokenNode  [<- [2...4)]
                TokenOperator  [<- [7...9)]
                    Variable  [b]
                    TokenNode  [<- [7...9)]
                    TokenOperator  [<- [12...14)]
                        Variable  [c]
                        TokenNode  [<- [12...14)]
                        NumericalValue  [0 [15...16)]
";
            ParserTest.VerifyParse(expected, "a <- b <- c <- 0");
        }

        [Test]
        [Category.R.Parser]
        public void Assignments04() {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [0 -> a -> b]
        Expression  [0 -> a -> b]
            TokenOperator  [-> [7...9)]
                TokenOperator  [-> [2...4)]
                    NumericalValue  [0 [0...1)]
                    TokenNode  [-> [2...4)]
                    Variable  [a]
                TokenNode  [-> [7...9)]
                Variable  [b]
";
            ParserTest.VerifyParse(expected, "0 -> a -> b");
        }

        [Test]
        [Category.R.Parser]
        public void Assignments05() {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [z <- .Call(x)]
        Expression  [z <- .Call(x)]
            TokenOperator  [<- [2...4)]
                Variable  [z]
                TokenNode  [<- [2...4)]
                FunctionCall  [5...13)
                    Variable  [.Call]
                    TokenNode  [( [10...11)]
                    ArgumentList  [11...12)
                        ExpressionArgument  [11...12)
                            Expression  [x]
                                Variable  [x]
                    TokenNode  [) [12...13)]
";
            ParserTest.VerifyParse(expected, "z <- .Call(x)");
        }

        [Test]
        [Category.R.Parser]
        public void Assignments06() {
            string expected =
@"GlobalScope  [Global]

UnexpectedToken Token [0...2)
";
            ParserTest.VerifyParse(expected, "_z <- 0");
        }

        [Test]
        [Category.R.Parser]
        public void Assignments07() {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [StudentData$ScoreRounded<-round(StudentData$Score)]
        Expression  [StudentData$ScoreRounded<-round(StudentData$Score)]
            TokenOperator  [<- [24...26)]
                TokenOperator  [$ [11...12)]
                    Variable  [StudentData]
                    TokenNode  [$ [11...12)]
                    Variable  [ScoreRounded]
                TokenNode  [<- [24...26)]
                FunctionCall  [26...50)
                    Variable  [round]
                    TokenNode  [( [31...32)]
                    ArgumentList  [32...49)
                        ExpressionArgument  [32...49)
                            Expression  [StudentData$Score]
                                TokenOperator  [$ [43...44)]
                                    Variable  [StudentData]
                                    TokenNode  [$ [43...44)]
                                    Variable  [Score]
                    TokenNode  [) [49...50)]
";
            ParserTest.VerifyParse(expected, "StudentData$ScoreRounded<-round(StudentData$Score)");
        }

        [Test]
        [Category.R.Parser]
        public void ParseDataTableAssignment() {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [dt[, b := letters[1:3]]]
        Expression  [dt[, b := letters[1:3]]]
            Indexer  [0...23)
                Variable  [dt]
                TokenNode  [[ [2...3)]
                ArgumentList  [3...22)
                    MissingArgument  [{Missing}]
                        TokenNode  [, [3...4)]
                    ExpressionArgument  [5...22)
                        Expression  [b := letters[1:3]]
                            TokenOperator  [:= [7...9)]
                                Variable  [b]
                                TokenNode  [:= [7...9)]
                                Indexer  [10...22)
                                    Variable  [letters]
                                    TokenNode  [[ [17...18)]
                                    ArgumentList  [18...21)
                                        ExpressionArgument  [18...21)
                                            Expression  [1:3]
                                                TokenOperator  [: [19...20)]
                                                    NumericalValue  [1 [18...19)]
                                                    TokenNode  [: [19...20)]
                                                    NumericalValue  [3 [20...21)]
                                    TokenNode  [] [21...22)]
                TokenNode  [] [22...23)]
";
            ParserTest.VerifyParse(expected, "dt[, b := letters[1:3]]");
        }

        [Test]
        [Category.R.Parser]
        public void LeftSideExpressionAssignments01() {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [x + y <- 1]
        Expression  [x + y <- 1]
            TokenOperator  [<- [6...8)]
                TokenOperator  [+ [2...3)]
                    Variable  [x]
                    TokenNode  [+ [2...3)]
                    Variable  [y]
                TokenNode  [<- [6...8)]
                NumericalValue  [1 [9...10)]
";
            ParserTest.VerifyParse(expected, "x + y <- 1");
        }
    }
}
