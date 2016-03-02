// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.R.Core.Test.Utility;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.R.Core.Test.Parser {
    [ExcludeFromCodeCoverage]
    public class ParseBuiltInsTest {
        [Test]
        [Category.R.Parser]
        public void ParseBuiltInsTest01() {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [return <- as.matrix(x)]
        Expression  [return <- as.matrix(x)]
            TokenOperator  [<- [7...9)]
                Variable  [return]
                TokenNode  [<- [7...9)]
                FunctionCall  [10...22)
                    Variable  [as.matrix]
                    TokenNode  [( [19...20)]
                    ArgumentList  [20...21)
                        ExpressionArgument  [20...21)
                            Expression  [x]
                                Variable  [x]
                    TokenNode  [) [21...22)]
";
            ParserTest.VerifyParse(expected, "return <- as.matrix(x)");
        }

        [Test]
        [Category.R.Parser]
        public void ParseBuiltInsTest02() {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [require <- 1]
        Expression  [require <- 1]
            TokenOperator  [<- [8...10)]
                Variable  [require]
                TokenNode  [<- [8...10)]
                NumericalValue  [1 [11...12)]
";
            ParserTest.VerifyParse(expected, "require <- 1");
        }

        [Test]
        [Category.R.Parser]
        public void ParseBuiltInsTest03() {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [switch <- as.matrix(x)]
        Expression  [switch <- as.matrix(x)]
            TokenOperator  [<- [7...9)]
                Variable  [switch]
                TokenNode  [<- [7...9)]
                FunctionCall  [10...22)
                    Variable  [as.matrix]
                    TokenNode  [( [19...20)]
                    ArgumentList  [20...21)
                        ExpressionArgument  [20...21)
                            Expression  [x]
                                Variable  [x]
                    TokenNode  [) [21...22)]
";
            ParserTest.VerifyParse(expected, "switch <- as.matrix(x)");
        }

        [Test]
        [Category.R.Parser]
        public void ParseBuiltInsTest04() {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [x[require] <- y[library]]
        Expression  [x[require] <- y[library]]
            TokenOperator  [<- [11...13)]
                Indexer  [0...10)
                    Variable  [x]
                    TokenNode  [[ [1...2)]
                    ArgumentList  [2...9)
                        ExpressionArgument  [2...9)
                            Expression  [require]
                                Variable  [require]
                    TokenNode  [] [9...10)]
                TokenNode  [<- [11...13)]
                Indexer  [14...24)
                    Variable  [y]
                    TokenNode  [[ [15...16)]
                    ArgumentList  [16...23)
                        ExpressionArgument  [16...23)
                            Expression  [library]
                                Variable  [library]
                    TokenNode  [] [23...24)]
";
            ParserTest.VerifyParse(expected, "x[require] <- y[library]");
        }
    }
}
