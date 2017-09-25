// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.R.Core.Test.Utility;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.R.Core.Test.Parser {
    [ExcludeFromCodeCoverage]
    [Category.R.Parser]
    public class ParseExpressionsTest {
        [Test]
        public void ParseExpressions01() {
            const string expected = @"GlobalScope  [Global]
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
        public void ParseListExpression01() {
            const string expected = @"GlobalScope  [Global]
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
        public void ParseExpressionSequence01() {
            const string expected = @"GlobalScope  [Global]
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

            var content =
@"a <- 1*b
  (c+1)";
            ParserTest.VerifyParse(expected, content);
        }

        [Test]
        public void ParseExpressionSequence02() {
            const string expected = @"GlobalScope  [Global]
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

            var content =
@"a <- 1*b[1]
  (c+1)";
            ParserTest.VerifyParse(expected, content);
        }

        [Test]
        public void ParseExpressionSequence03() {
            const string expected = @"GlobalScope  [Global]
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

            var content =
@"a <- 1*b[[1]]
  (c+1)";
            ParserTest.VerifyParse(expected, content);
        }

        [Test]
        public void ParseMultipleTilde() {
            const string expected = @"GlobalScope  [Global]
    ExpressionStatement  [x ~ ~ ~ y]
        Expression  [x ~ ~ ~ y]
            TokenOperator  [~ [2...3)]
                Variable  [x]
                TokenNode  [~ [2...3)]
                TokenOperator  [~ [4...5)]
                    TokenNode  [~ [4...5)]
                    TokenOperator  [~ [6...7)]
                        TokenNode  [~ [6...7)]
                        Variable  [y]
";
            var content = "x ~ ~ ~ y";

            ParserTest.VerifyParse(expected, content);
        }

        [Test]
        public void ParseLongFloats() {
            const string expected = @"GlobalScope  [Global]
    ExpressionStatement  [sec <- function(x) {\r\n    3600L * floor(x / 1e4L) + 60 * floor(x %% 1e4L / 1e2L) + x %% 1e2 - 86400L * (x > 200000L)\r\n}]
        Expression  [sec <- function(x) {\r\n    3600L * floor(x / 1e4L) + 60 * floor(x %% 1e4L / 1e2L) + x %% 1e2 - 86400L * (x > 200000L)\r\n}]
            TokenOperator  [<- [4...6)]
                Variable  [sec]
                TokenNode  [<- [4...6)]
                FunctionDefinition  [7...119)
                    TokenNode  [function [7...15)]
                    TokenNode  [( [15...16)]
                    ArgumentList  [16...17)
                        ExpressionArgument  [16...17)
                            Expression  [x]
                                Variable  [x]
                    TokenNode  [) [17...18)]
                    Scope  []
                        TokenNode  [{ [19...20)]
                        ExpressionStatement  [3600L * floor(x / 1e4L) + 60 * floor(x %% 1e4L / 1e2L) + x %% 1e2 - 86400L * (x > 200000L)]
                            Expression  [3600L * floor(x / 1e4L) + 60 * floor(x %% 1e4L / 1e2L) + x %% 1e2 - 86400L * (x > 200000L)]
                                TokenOperator  [- [92...93)]
                                    TokenOperator  [+ [81...82)]
                                        TokenOperator  [+ [50...51)]
                                            TokenOperator  [* [32...33)]
                                                NumericalValue  [3600L [26...31)]
                                                TokenNode  [* [32...33)]
                                                FunctionCall  [34...49)
                                                    Variable  [floor]
                                                    TokenNode  [( [39...40)]
                                                    ArgumentList  [40...48)
                                                        ExpressionArgument  [40...48)
                                                            Expression  [x / 1e4L]
                                                                TokenOperator  [/ [42...43)]
                                                                    Variable  [x]
                                                                    TokenNode  [/ [42...43)]
                                                                    NumericalValue  [1e4L [44...48)]
                                                    TokenNode  [) [48...49)]
                                            TokenNode  [+ [50...51)]
                                            TokenOperator  [* [55...56)]
                                                NumericalValue  [60 [52...54)]
                                                TokenNode  [* [55...56)]
                                                FunctionCall  [57...80)
                                                    Variable  [floor]
                                                    TokenNode  [( [62...63)]
                                                    ArgumentList  [63...79)
                                                        ExpressionArgument  [63...79)
                                                            Expression  [x %% 1e4L / 1e2L]
                                                                TokenOperator  [/ [73...74)]
                                                                    TokenOperator  [%% [65...67)]
                                                                        Variable  [x]
                                                                        TokenNode  [%% [65...67)]
                                                                        NumericalValue  [1e4L [68...72)]
                                                                    TokenNode  [/ [73...74)]
                                                                    NumericalValue  [1e2L [75...79)]
                                                    TokenNode  [) [79...80)]
                                        TokenNode  [+ [81...82)]
                                        TokenOperator  [%% [85...87)]
                                            Variable  [x]
                                            TokenNode  [%% [85...87)]
                                            NumericalValue  [1e2 [88...91)]
                                    TokenNode  [- [92...93)]
                                    TokenOperator  [* [101...102)]
                                        NumericalValue  [86400L [94...100)]
                                        TokenNode  [* [101...102)]
                                        Group  [103...116)
                                            TokenNode  [( [103...104)]
                                            Expression  [x > 200000L]
                                                TokenOperator  [> [106...107)]
                                                    Variable  [x]
                                                    TokenNode  [> [106...107)]
                                                    NumericalValue  [200000L [108...115)]
                                            TokenNode  [) [115...116)]
                        TokenNode  [} [118...119)]
";
            var content =
@"sec <- function(x) {
    3600L * floor(x / 1e4L) + 60 * floor(x %% 1e4L / 1e2L) + x %% 1e2 - 86400L * (x > 200000L)
}";
            ParserTest.VerifyParse(expected, content);
        }

        [Test]
        public void ParsePipe() {
            const string expected = @"GlobalScope  [Global]
    ExpressionStatement  [a() %>% b]
        Expression  [a() %>% b]
            TokenOperator  [%>% [4...7)]
                FunctionCall  [0...3)
                    Variable  [a]
                    TokenNode  [( [1...2)]
                    TokenNode  [) [2...3)]
                TokenNode  [%>% [4...7)]
                Variable  [b]
";
            ParserTest.VerifyParse(expected, @"a() %>% b");
        }

        [Test]
        public void ParseBangExpression01() {
            const string expected = @"GlobalScope  [Global]
    ExpressionStatement  [a + !b + c]
        Expression  [a + !b + c]
            TokenOperator  [+ [2...3)]
                Variable  [a]
                TokenNode  [+ [2...3)]
                TokenOperator  [! [4...5)]
                    TokenNode  [! [4...5)]
                    Expression  [b + c]
                        TokenOperator  [+ [7...8)]
                            Variable  [b]
                            TokenNode  [+ [7...8)]
                            Variable  [c]
";
            ParserTest.VerifyParse(expected, @"a + !b + c");
        }
        [Test]

        public void ParseBangExpression02() {
            const string expected = @"GlobalScope  [Global]
    ExpressionStatement  [a + !(b +\n c)]
        Expression  [a + !(b +\n c)]
            TokenOperator  [+ [2...3)]
                Variable  [a]
                TokenNode  [+ [2...3)]
                TokenOperator  [! [4...5)]
                    TokenNode  [! [4...5)]
                    Expression  [(b +\n c)]
                        Group  [5...13)
                            TokenNode  [( [5...6)]
                            Expression  [b +\n c]
                                TokenOperator  [+ [8...9)]
                                    Variable  [b]
                                    TokenNode  [+ [8...9)]
                                    Variable  [c]
                            TokenNode  [) [12...13)]
";
            ParserTest.VerifyParse(expected, "a + !(b +\n c)");
        }

        [Test]
        public void ParseBangExpression03() {
            const string expected = @"GlobalScope  [Global]
    If  []
        TokenNode  [if [0...2)]
        TokenNode  [( [2...3)]
        Expression  [!a()]
            TokenOperator  [! [3...4)]
                TokenNode  [! [3...4)]
                Expression  [a()]
                    FunctionCall  [4...7)
                        Variable  [a]
                        TokenNode  [( [5...6)]
                        TokenNode  [) [6...7)]
        TokenNode  [) [7...8)]
        Scope  []
            TokenNode  [{ [9...10)]
            TokenNode  [} [11...12)]
";
            ParserTest.VerifyParse(expected, "if(!a()) { }");
        }
        [Test]
        public void ParseBangExpression04() {
            const string expected = @"GlobalScope  [Global]
    ExpressionStatement  [a <- !b]
        Expression  [a <- !b]
            TokenOperator  [<- [2...4)]
                Variable  [a]
                TokenNode  [<- [2...4)]
                TokenOperator  [! [5...6)]
                    TokenNode  [! [5...6)]
                    Expression  [b]
                        Variable  [b]
    If  []
        TokenNode  [if [8...10)]
        TokenNode  [( [10...11)]
        Expression  [TRUE]
            LogicalValue  [TRUE [11...15)]
        TokenNode  [) [15...16)]
        Scope  []
            TokenNode  [{ [17...18)]
            TokenNode  [} [19...20)]
";
            ParserTest.VerifyParse(expected, "a <- !b\nif(TRUE) { }");
        }
    }
}
