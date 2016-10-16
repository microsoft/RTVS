// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.R.Core.Test.Utility;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.R.Core.Test.Parser {
    [ExcludeFromCodeCoverage] 
    public class Indexer {
        [Test]
        [Category.R.Parser]
        public void Indexer01() {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [a[1]]
        Expression  [a[1]]
            Indexer  [0...4)
                Variable  [a]
                TokenNode  [[ [1...2)]
                ArgumentList  [2...3)
                    ExpressionArgument  [2...3)
                        Expression  [1]
                            NumericalValue  [1 [2...3)]
                TokenNode  [] [3...4)]
";
            ParserTest.VerifyParse(expected, "a[1]");
        }

        [Test]
        [Category.R.Parser]
        public void Indexer02() {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [x[1,2]]
        Expression  [x[1,2]]
            Indexer  [0...6)
                Variable  [x]
                TokenNode  [[ [1...2)]
                ArgumentList  [2...5)
                    ExpressionArgument  [2...4)
                        Expression  [1]
                            NumericalValue  [1 [2...3)]
                        TokenNode  [, [3...4)]
                    ExpressionArgument  [4...5)
                        Expression  [2]
                            NumericalValue  [2 [4...5)]
                TokenNode  [] [5...6)]
";
            ParserTest.VerifyParse(expected, "x[1,2]");
        }

        [Test]
        [Category.R.Parser]
        public void Indexer03() {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [x(a)[1]]
        Expression  [x(a)[1]]
            Indexer  [0...7)
                FunctionCall  [0...4)
                    Variable  [x]
                    TokenNode  [( [1...2)]
                    ArgumentList  [2...3)
                        ExpressionArgument  [2...3)
                            Expression  [a]
                                Variable  [a]
                    TokenNode  [) [3...4)]
                TokenNode  [[ [4...5)]
                ArgumentList  [5...6)
                    ExpressionArgument  [5...6)
                        Expression  [1]
                            NumericalValue  [1 [5...6)]
                TokenNode  [] [6...7)]
";
            ParserTest.VerifyParse(expected, "x(a)[1]");
        }

        [Test]
        [Category.R.Parser]
        public void Indexer04() {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [x[[a+(b*c)]]]
        Expression  [x[[a+(b*c)]]]
            Indexer  [0...12)
                Variable  [x]
                TokenNode  [[[ [1...3)]
                ArgumentList  [3...10)
                    ExpressionArgument  [3...10)
                        Expression  [a+(b*c)]
                            TokenOperator  [+ [4...5)]
                                Variable  [a]
                                TokenNode  [+ [4...5)]
                                Group  [5...10)
                                    TokenNode  [( [5...6)]
                                    Expression  [b*c]
                                        TokenOperator  [* [7...8)]
                                            Variable  [b]
                                            TokenNode  [* [7...8)]
                                            Variable  [c]
                                    TokenNode  [) [9...10)]
                TokenNode  []] [10...12)]
";
            ParserTest.VerifyParse(expected, "x[[a+(b*c)]]");
        }

        [Test]
        [Category.R.Parser]
        public void Indexer05() {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [x[1](c)]
        Expression  [x[1](c)]
            FunctionCall  [0...7)
                Indexer  [0...4)
                    Variable  [x]
                    TokenNode  [[ [1...2)]
                    ArgumentList  [2...3)
                        ExpressionArgument  [2...3)
                            Expression  [1]
                                NumericalValue  [1 [2...3)]
                    TokenNode  [] [3...4)]
                TokenNode  [( [4...5)]
                ArgumentList  [5...6)
                    ExpressionArgument  [5...6)
                        Expression  [c]
                            Variable  [c]
                TokenNode  [) [6...7)]
";
            ParserTest.VerifyParse(expected, "x[1](c)");
        }

        [Test]
        [Category.R.Parser]
        public void Indexer06() {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [x[, 1]]
        Expression  [x[, 1]]
            Indexer  [0...6)
                Variable  [x]
                TokenNode  [[ [1...2)]
                ArgumentList  [2...5)
                    MissingArgument  [{Missing}]
                        TokenNode  [, [2...3)]
                    ExpressionArgument  [4...5)
                        Expression  [1]
                            NumericalValue  [1 [4...5)]
                TokenNode  [] [5...6)]
";
            ParserTest.VerifyParse(expected, "x[, 1]");
        }

        [Test]
        [Category.R.Parser]
        public void Indexer07() {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [x[2,]]
        Expression  [x[2,]]
            Indexer  [0...5)
                Variable  [x]
                TokenNode  [[ [1...2)]
                ArgumentList  [2...4)
                    ExpressionArgument  [2...4)
                        Expression  [2]
                            NumericalValue  [2 [2...3)]
                        TokenNode  [, [3...4)]
                    StubArgument  [{Stub}]
                TokenNode  [] [4...5)]
";
            ParserTest.VerifyParse(expected, "x[2,]");
        }

        [Test]
        [Category.R.Parser]
        public void Indexer08() {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [x[,,]]
        Expression  [x[,,]]
            Indexer  [0...5)
                Variable  [x]
                TokenNode  [[ [1...2)]
                ArgumentList  [2...4)
                    MissingArgument  [{Missing}]
                        TokenNode  [, [2...3)]
                    MissingArgument  [{Missing}]
                        TokenNode  [, [3...4)]
                    StubArgument  [{Stub}]
                TokenNode  [] [4...5)]
";
            ParserTest.VerifyParse(expected, "x[,,]");
        }

        [Test]
        [Category.R.Parser]
        public void Indexer09() {
            string expected =
@"GlobalScope  [Global]

RightOperandExpected Token [1...2)
";
            ParserTest.VerifyParse(expected, "[]");
        }

        [Test]
        [Category.R.Parser]
        public void Indexer10() {
            string expected =
"GlobalScope  [Global]\r\n" +
"    ExpressionStatement  [colnames(data)[colnames(data)==\"old_name\"] <- \"new_name\"]\r\n" +
"        Expression  [colnames(data)[colnames(data)==\"old_name\"] <- \"new_name\"]\r\n" +
"            TokenOperator  [<- [43...45)]\r\n" +
"                Indexer  [0...42)\r\n" +
"                    FunctionCall  [0...14)\r\n" +
"                        Variable  [colnames]\r\n" +
"                        TokenNode  [( [8...9)]\r\n" +
"                        ArgumentList  [9...13)\r\n" +
"                            ExpressionArgument  [9...13)\r\n" +
"                                Expression  [data]\r\n" +
"                                    Variable  [data]\r\n" +
"                        TokenNode  [) [13...14)]\r\n" +
"                    TokenNode  [[ [14...15)]\r\n" +
"                    ArgumentList  [15...41)\r\n" +
"                        ExpressionArgument  [15...41)\r\n" +
"                            Expression  [colnames(data)==\"old_name\"]\r\n" +
"                                TokenOperator  [== [29...31)]\r\n" +
"                                    FunctionCall  [15...29)\r\n" +
"                                        Variable  [colnames]\r\n" +
"                                        TokenNode  [( [23...24)]\r\n" +
"                                        ArgumentList  [24...28)\r\n" +
"                                            ExpressionArgument  [24...28)\r\n" +
"                                                Expression  [data]\r\n" +
"                                                    Variable  [data]\r\n" +
"                                        TokenNode  [) [28...29)]\r\n" +
"                                    TokenNode  [== [29...31)]\r\n" +
"                                    StringValue  [\"old_name\" [31...41)]\r\n" +
"                    TokenNode  [] [41...42)]\r\n" +
"                TokenNode  [<- [43...45)]\r\n" +
"                StringValue  [\"new_name\" [46...56)]\r\n";

            ParserTest.VerifyParse(expected, "colnames(data)[colnames(data)==\"old_name\"] <- \"new_name\"");
        }

        [Test]
        [Category.R.Parser]
        public void Indexer11() {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [append(LETTERS[)]
        Expression  [append(LETTERS[)]
            FunctionCall  [0...16)
                Variable  [append]
                TokenNode  [( [6...7)]
                ArgumentList  [7...15)
                    ExpressionArgument  [7...15)
                        Expression  [LETTERS[]
                            Indexer  [7...15)
                                Variable  [LETTERS]
                                TokenNode  [[ [14...15)]
                TokenNode  [) [15...16)]

CloseSquareBracketExpected AfterToken [14...15)
";
            ParserTest.VerifyParse(expected, "append(LETTERS[)");
        }

        [Test]
        [Category.R.Parser]
        public void Indexer12() {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [(a())[x]]
        Expression  [(a())[x]]
            Indexer  [0...8)
                Group  [0...5)
                    TokenNode  [( [0...1)]
                    Expression  [a()]
                        FunctionCall  [1...4)
                            Variable  [a]
                            TokenNode  [( [2...3)]
                            TokenNode  [) [3...4)]
                    TokenNode  [) [4...5)]
                TokenNode  [[ [5...6)]
                ArgumentList  [6...7)
                    ExpressionArgument  [6...7)
                        Expression  [x]
                            Variable  [x]
                TokenNode  [] [7...8)]
";
            ParserTest.VerifyParse(expected, "(a())[x]");
        }

        [Test]
        [Category.R.Parser]
        public void Indexer13() {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [topfiles <- topfiles[file.info(topfiles, extra_cols = FALSE)$ctime\r\n <= .unpack.time]]
        Expression  [topfiles <- topfiles[file.info(topfiles, extra_cols = FALSE)$ctime\r\n <= .unpack.time]]
            TokenOperator  [<- [9...11)]
                Variable  [topfiles]
                TokenNode  [<- [9...11)]
                Indexer  [12...85)
                    Variable  [topfiles]
                    TokenNode  [[ [20...21)]
                    ArgumentList  [21...84)
                        ExpressionArgument  [21...84)
                            Expression  [file.info(topfiles, extra_cols = FALSE)$ctime\r\n <= .unpack.time]
                                TokenOperator  [<= [69...71)]
                                    TokenOperator  [$ [60...61)]
                                        FunctionCall  [21...60)
                                            Variable  [file.info]
                                            TokenNode  [( [30...31)]
                                            ArgumentList  [31...59)
                                                ExpressionArgument  [31...40)
                                                    Expression  [topfiles]
                                                        Variable  [topfiles]
                                                    TokenNode  [, [39...40)]
                                                NamedArgument  [41...59)
                                                    TokenNode  [extra_cols [41...51)]
                                                    TokenNode  [= [52...53)]
                                                    Expression  [FALSE]
                                                        LogicalValue  [FALSE [54...59)]
                                            TokenNode  [) [59...60)]
                                        TokenNode  [$ [60...61)]
                                        Variable  [ctime]
                                    TokenNode  [<= [69...71)]
                                    Variable  [.unpack.time]
                    TokenNode  [] [84...85)]
";
            ParserTest.VerifyParse(expected, "topfiles <- topfiles[file.info(topfiles, extra_cols = FALSE)$ctime\r\n <= .unpack.time]");
        }

        [Test]
        [Category.R.Parser]
        public void Indexer14() {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [x[[1]](c)]
        Expression  [x[[1]](c)]
            FunctionCall  [0...9)
                Indexer  [0...6)
                    Variable  [x]
                    TokenNode  [[[ [1...3)]
                    ArgumentList  [3...4)
                        ExpressionArgument  [3...4)
                            Expression  [1]
                                NumericalValue  [1 [3...4)]
                    TokenNode  []] [4...6)]
                TokenNode  [( [6...7)]
                ArgumentList  [7...8)
                    ExpressionArgument  [7...8)
                        Expression  [c]
                            Variable  [c]
                TokenNode  [) [8...9)]
";
            ParserTest.VerifyParse(expected, "x[[1]](c)");
        }

        [Test]
        [Category.R.Parser]
        public void Indexer15() {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [x[[1]][2]]
        Expression  [x[[1]][2]]
            Indexer  [0...9)
                Indexer  [0...6)
                    Variable  [x]
                    TokenNode  [[[ [1...3)]
                    ArgumentList  [3...4)
                        ExpressionArgument  [3...4)
                            Expression  [1]
                                NumericalValue  [1 [3...4)]
                    TokenNode  []] [4...6)]
                TokenNode  [[ [6...7)]
                ArgumentList  [7...8)
                    ExpressionArgument  [7...8)
                        Expression  [2]
                            NumericalValue  [2 [7...8)]
                TokenNode  [] [8...9)]
";
            ParserTest.VerifyParse(expected, "x[[1]][2]");
        }

        [Test]
        [Category.R.Parser]
        public void Indexer16() {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [x[1][2]]
        Expression  [x[1][2]]
            Indexer  [0...7)
                Indexer  [0...4)
                    Variable  [x]
                    TokenNode  [[ [1...2)]
                    ArgumentList  [2...3)
                        ExpressionArgument  [2...3)
                            Expression  [1]
                                NumericalValue  [1 [2...3)]
                    TokenNode  [] [3...4)]
                TokenNode  [[ [4...5)]
                ArgumentList  [5...6)
                    ExpressionArgument  [5...6)
                        Expression  [2]
                            NumericalValue  [2 [5...6)]
                TokenNode  [] [6...7)]
";
            ParserTest.VerifyParse(expected, "x[1][2]");
        }

        [Test]
        [Category.R.Parser]
        public void Indexer17() {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [x[[1]][[2]]]
        Expression  [x[[1]][[2]]]
            Indexer  [0...11)
                Indexer  [0...6)
                    Variable  [x]
                    TokenNode  [[[ [1...3)]
                    ArgumentList  [3...4)
                        ExpressionArgument  [3...4)
                            Expression  [1]
                                NumericalValue  [1 [3...4)]
                    TokenNode  []] [4...6)]
                TokenNode  [[[ [6...8)]
                ArgumentList  [8...9)
                    ExpressionArgument  [8...9)
                        Expression  [2]
                            NumericalValue  [2 [8...9)]
                TokenNode  []] [9...11)]
";
            ParserTest.VerifyParse(expected, "x[[1]][[2]]");
        }
        [Test]
        [Category.R.Parser]
        public void Indexer18() {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [x[-1][+1]]
        Expression  [x[-1][+1]]
            Indexer  [0...9)
                Indexer  [0...5)
                    Variable  [x]
                    TokenNode  [[ [1...2)]
                    ArgumentList  [2...4)
                        ExpressionArgument  [2...4)
                            Expression  [-1]
                                NumericalValue  [-1 [2...4)]
                    TokenNode  [] [4...5)]
                TokenNode  [[ [5...6)]
                ArgumentList  [6...8)
                    ExpressionArgument  [6...8)
                        Expression  [+1]
                            NumericalValue  [+1 [6...8)]
                TokenNode  [] [8...9)]
";
            ParserTest.VerifyParse(expected, "x[-1][+1]");
        }

        [Test]
        [Category.R.Parser]
        public void Indexer19() {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [x[a()]]
        Expression  [x[a()]]
            Indexer  [0...6)
                Variable  [x]
                TokenNode  [[ [1...2)]
                ArgumentList  [2...5)
                    ExpressionArgument  [2...5)
                        Expression  [a()]
                            FunctionCall  [2...5)
                                Variable  [a]
                                TokenNode  [( [3...4)]
                                TokenNode  [) [4...5)]
                TokenNode  [] [5...6)]
";
            ParserTest.VerifyParse(expected, "x[a()]");
        }

        [Test]
        [Category.R.Parser]
        public void Indexer20() {
            string expected =
@"GlobalScope  [Global]

RightOperandExpected AfterToken [3...4)
RightOperandExpected Token [4...5)
";
            ParserTest.VerifyParse(expected, "[][][]");
        }
    }
}
