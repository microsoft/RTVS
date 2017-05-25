// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.R.Core.Test.Utility;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.R.Core.Test.Parser {
    [ExcludeFromCodeCoverage]   
    public class ParseIfElseTest {
        [Test]
        [Category.R.Parser]
        public void ParseIfElseTest01() {
            string expected =
@"GlobalScope  [Global]
    If  []
        TokenNode  [if [0...2)]
        TokenNode  [( [2...3)]
        Expression  [x < y]
            TokenOperator  [< [5...6)]
                Variable  [x]
                TokenNode  [< [5...6)]
                Variable  [y]
        TokenNode  [) [8...9)]
        SimpleScope  [10...18)
            ExpressionStatement  [x <- x+1]
                Expression  [x <- x+1]
                    TokenOperator  [<- [12...14)]
                        Variable  [x]
                        TokenNode  [<- [12...14)]
                        TokenOperator  [+ [16...17)]
                            Variable  [x]
                            TokenNode  [+ [16...17)]
                            NumericalValue  [1 [17...18)]
";
            ParserTest.VerifyParse(expected, "if(x < y) x <- x+1");
        }

        [Test]
        [Category.R.Parser]
        public void ParseIfElseTest02() {
            string expected =
@"GlobalScope  [Global]
    If  []
        TokenNode  [if [0...2)]
        TokenNode  [( [2...3)]
        Expression  [x < y]
            TokenOperator  [< [5...6)]
                Variable  [x]
                TokenNode  [< [5...6)]
                Variable  [y]
        TokenNode  [) [8...9)]
        Scope  []
            TokenNode  [{ [10...11)]
            ExpressionStatement  [x <- x+1]
                Expression  [x <- x+1]
                    TokenOperator  [<- [14...16)]
                        Variable  [x]
                        TokenNode  [<- [14...16)]
                        TokenOperator  [+ [18...19)]
                            Variable  [x]
                            TokenNode  [+ [18...19)]
                            NumericalValue  [1 [19...20)]
            TokenNode  [} [21...22)]
";
            ParserTest.VerifyParse(expected, "if(x < y) { x <- x+1 }");
        }

        [Test]
        [Category.R.Parser]
        public void ParseIfElseTest03() {
            string expected =
@"GlobalScope  [Global]
    If  []
        TokenNode  [if [0...2)]
        TokenNode  [( [2...3)]
        Expression  [x < y]
            TokenOperator  [< [5...6)]
                Variable  [x]
                TokenNode  [< [5...6)]
                Variable  [y]
        TokenNode  [) [8...9)]
        Scope  []
            TokenNode  [{ [10...11)]
            ExpressionStatement  [x <- x+1]
                Expression  [x <- x+1]
                    TokenOperator  [<- [14...16)]
                        Variable  [x]
                        TokenNode  [<- [14...16)]
                        TokenOperator  [+ [18...19)]
                            Variable  [x]
                            TokenNode  [+ [18...19)]
                            NumericalValue  [1 [19...20)]
            TokenNode  [} [21...22)]
        KeywordScopeStatement  []
            TokenNode  [else [23...27)]
            Scope  []
                TokenNode  [{ [28...29)]
                ExpressionStatement  [x <- x + 2]
                    Expression  [x <- x + 2]
                        TokenOperator  [<- [32...34)]
                            Variable  [x]
                            TokenNode  [<- [32...34)]
                            TokenOperator  [+ [37...38)]
                                Variable  [x]
                                TokenNode  [+ [37...38)]
                                NumericalValue  [2 [39...40)]
                TokenNode  [} [41...42)]
";
            ParserTest.VerifyParse(expected, "if(x < y) { x <- x+1 } else { x <- x + 2 }");
        }

        [Test]
        [Category.R.Parser]
        public void ParseIfElseTest04() {
            string expected =
@"GlobalScope  [Global]
    If  []
        TokenNode  [if [0...2)]
        TokenNode  [( [2...3)]
        Expression  [x < y]
            TokenOperator  [< [5...6)]
                Variable  [x]
                TokenNode  [< [5...6)]
                Variable  [y]
        TokenNode  [) [8...9)]
        SimpleScope  [10...18)
            ExpressionStatement  [x <- x+1]
                Expression  [x <- x+1]
                    TokenOperator  [<- [12...14)]
                        Variable  [x]
                        TokenNode  [<- [12...14)]
                        TokenOperator  [+ [16...17)]
                            Variable  [x]
                            TokenNode  [+ [16...17)]
                            NumericalValue  [1 [17...18)]
        KeywordScopeStatement  []
            TokenNode  [else [19...23)]
            Scope  []
                TokenNode  [{ [24...25)]
                ExpressionStatement  [x <- x + 2]
                    Expression  [x <- x + 2]
                        TokenOperator  [<- [28...30)]
                            Variable  [x]
                            TokenNode  [<- [28...30)]
                            TokenOperator  [+ [33...34)]
                                Variable  [x]
                                TokenNode  [+ [33...34)]
                                NumericalValue  [2 [35...36)]
                TokenNode  [} [37...38)]
";
            ParserTest.VerifyParse(expected, "if(x < y) x <- x+1 else { x <- x + 2 }");
        }

        [Test]
        [Category.R.Parser]
        public void ParseIfElseTest05() {
            string expected =
@"GlobalScope  [Global]
    If  []
        TokenNode  [if [0...2)]
        TokenNode  [( [2...3)]
        Expression  [x < y]
            TokenOperator  [< [5...6)]
                Variable  [x]
                TokenNode  [< [5...6)]
                Variable  [y]
        TokenNode  [) [8...9)]
        SimpleScope  [10...18)
            ExpressionStatement  [x <- x+1]
                Expression  [x <- x+1]
                    TokenOperator  [<- [12...14)]
                        Variable  [x]
                        TokenNode  [<- [12...14)]
                        TokenOperator  [+ [16...17)]
                            Variable  [x]
                            TokenNode  [+ [16...17)]
                            NumericalValue  [1 [17...18)]
        KeywordScopeStatement  []
            TokenNode  [else [19...23)]
            SimpleScope  [24...34)
                ExpressionStatement  [x <- x + 2]
                    Expression  [x <- x + 2]
                        TokenOperator  [<- [26...28)]
                            Variable  [x]
                            TokenNode  [<- [26...28)]
                            TokenOperator  [+ [31...32)]
                                Variable  [x]
                                TokenNode  [+ [31...32)]
                                NumericalValue  [2 [33...34)]
";
            ParserTest.VerifyParse(expected, "if(x < y) x <- x+1 else x <- x + 2");
        }

        [Test]
        [Category.R.Parser]
        public void ParseIfElseTest06() {
            string expected =
@"GlobalScope  [Global]
    If  []
        TokenNode  [if [0...2)]
        TokenNode  [( [2...3)]
        Expression  [x < y]
            TokenOperator  [< [5...6)]
                Variable  [x]
                TokenNode  [< [5...6)]
                Variable  [y]
        TokenNode  [) [8...9)]
        SimpleScope  [12...20)
            ExpressionStatement  [x <- x+1]
                Expression  [x <- x+1]
                    TokenOperator  [<- [14...16)]
                        Variable  [x]
                        TokenNode  [<- [14...16)]
                        TokenOperator  [+ [18...19)]
                            Variable  [x]
                            TokenNode  [+ [18...19)]
                            NumericalValue  [1 [19...20)]
        KeywordScopeStatement  []
            TokenNode  [else [21...25)]
            Scope  []
                TokenNode  [{ [28...29)]
                ExpressionStatement  [x <- x + 2]
                    Expression  [x <- x + 2]
                        TokenOperator  [<- [32...34)]
                            Variable  [x]
                            TokenNode  [<- [32...34)]
                            TokenOperator  [+ [37...38)]
                                Variable  [x]
                                TokenNode  [+ [37...38)]
                                NumericalValue  [2 [39...40)]
                TokenNode  [} [41...42)]
";
            ParserTest.VerifyParse(expected, "if(x < y) \n x <- x+1 else \n { x <- x + 2 }");
        }

        [Test]
        [Category.R.Parser]
        public void ParseIfElseTest07() {
            string expected =
@"GlobalScope  [Global]
    If  []
        TokenNode  [if [0...2)]
        TokenNode  [( [2...3)]
        Expression  [x < y]
            TokenOperator  [< [5...6)]
                Variable  [x]
                TokenNode  [< [5...6)]
                Variable  [y]
        TokenNode  [) [8...9)]
        Scope  []
            TokenNode  [{ [10...11)]
            ExpressionStatement  [x <- x+1]
                Expression  [x <- x+1]
                    TokenOperator  [<- [14...16)]
                        Variable  [x]
                        TokenNode  [<- [14...16)]
                        TokenOperator  [+ [18...19)]
                            Variable  [x]
                            TokenNode  [+ [18...19)]
                            NumericalValue  [1 [19...20)]
            TokenNode  [} [21...22)]
    ExpressionStatement  [x <- x + 2]
        Expression  [x <- x + 2]
            TokenOperator  [<- [34...36)]
                Variable  [x]
                TokenNode  [<- [34...36)]
                TokenOperator  [+ [39...40)]
                    Variable  [x]
                    TokenNode  [+ [39...40)]
                    NumericalValue  [2 [41...42)]

UnexpectedToken Token [25...29)
";
            ParserTest.VerifyParse(expected, "if(x < y) { x <- x+1 } \n else \n x <- x + 2");
        }

        [Test]
        [Category.R.Parser]
        public void ParseIfElseTest08() {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [func(if(x < y) 1 \n else \n 2, a)]
        Expression  [func(if(x < y) 1 \n else \n 2, a)]
            FunctionCall  [0...31)
                Variable  [func]
                TokenNode  [( [4...5)]
                ArgumentList  [5...30)
                    ExpressionArgument  [5...28)
                        Expression  [if(x < y) 1 \n else \n 2]
                            InlineIf  []
                                TokenNode  [if [5...7)]
                                TokenNode  [( [7...8)]
                                Expression  [x < y]
                                    TokenOperator  [< [10...11)]
                                        Variable  [x]
                                        TokenNode  [< [10...11)]
                                        Variable  [y]
                                TokenNode  [) [13...14)]
                                SimpleScope  [15...16)
                                    ExpressionStatement  [1]
                                        Expression  [1]
                                            NumericalValue  [1 [15...16)]
                                KeywordScopeStatement  []
                                    TokenNode  [else [19...23)]
                                    SimpleScope  [26...27)
                                        ExpressionStatement  [2]
                                            Expression  [2]
                                                NumericalValue  [2 [26...27)]
                        TokenNode  [, [27...28)]
                    ExpressionArgument  [29...30)
                        Expression  [a]
                            Variable  [a]
                TokenNode  [) [30...31)]
";
            ParserTest.VerifyParse(expected, "func(if(x < y) 1 \n else \n 2, a)");
        }

        [Test]
        [Category.R.Parser]
        public void ParseIfElseTest09() {
            string expected =
@"GlobalScope  [Global]
    If  []
        TokenNode  [if [0...2)]
        TokenNode  [( [2...3)]
        Expression  [x < y]
            TokenOperator  [< [5...6)]
                Variable  [x]
                TokenNode  [< [5...6)]
                Variable  [y]
        TokenNode  [) [8...9)]
        SimpleScope  [10...11)
            ExpressionStatement  [1]
                Expression  [1]
                    NumericalValue  [1 [10...11)]
    ExpressionStatement  [2]
        Expression  [2]
            NumericalValue  [2 [19...20)]

UnexpectedToken Token [14...18)
";
            ParserTest.VerifyParse(expected, "if(x < y) 1 \n else 2");
        }

        [Test]
        [Category.R.Parser]
        public void ParseIfElseTest10() {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [x <- if(x < y) 1]
        Expression  [x <- if(x < y) 1]
            TokenOperator  [<- [2...4)]
                Variable  [x]
                TokenNode  [<- [2...4)]
                InlineIf  []
                    TokenNode  [if [5...7)]
                    TokenNode  [( [7...8)]
                    Expression  [x < y]
                        TokenOperator  [< [10...11)]
                            Variable  [x]
                            TokenNode  [< [10...11)]
                            Variable  [y]
                    TokenNode  [) [13...14)]
                    SimpleScope  [15...16)
                        ExpressionStatement  [1]
                            Expression  [1]
                                NumericalValue  [1 [15...16)]
    ExpressionStatement  [2]
        Expression  [2]
            NumericalValue  [2 [24...25)]

UnexpectedToken Token [19...23)
";
            ParserTest.VerifyParse(expected, "x <- if(x < y) 1 \n else 2");
        }

        [Test]
        [Category.R.Parser]
        public void ParseIfElseTest12() {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [x <- func(a = if(x < y) 1 \n else 2)]
        Expression  [x <- func(a = if(x < y) 1 \n else 2)]
            TokenOperator  [<- [2...4)]
                Variable  [x]
                TokenNode  [<- [2...4)]
                FunctionCall  [5...35)
                    Variable  [func]
                    TokenNode  [( [9...10)]
                    ArgumentList  [10...34)
                        NamedArgument  [10...34)
                            TokenNode  [a [10...11)]
                            TokenNode  [= [12...13)]
                            Expression  [if(x < y) 1 \n else 2]
                                InlineIf  []
                                    TokenNode  [if [14...16)]
                                    TokenNode  [( [16...17)]
                                    Expression  [x < y]
                                        TokenOperator  [< [19...20)]
                                            Variable  [x]
                                            TokenNode  [< [19...20)]
                                            Variable  [y]
                                    TokenNode  [) [22...23)]
                                    SimpleScope  [24...25)
                                        ExpressionStatement  [1]
                                            Expression  [1]
                                                NumericalValue  [1 [24...25)]
                                    KeywordScopeStatement  []
                                        TokenNode  [else [28...32)]
                                        SimpleScope  [33...34)
                                            ExpressionStatement  [2]
                                                Expression  [2]
                                                    NumericalValue  [2 [33...34)]
                    TokenNode  [) [34...35)]
";
            ParserTest.VerifyParse(expected, "x <- func(a = if(x < y) 1 \n else 2)");
        }

        [Test]
        [Category.R.Parser]
        public void ParseIfElseTest13() {
            string expected =
@"GlobalScope  [Global]
    Scope  []
        TokenNode  [{ [0...1)]
        If  []
            TokenNode  [if [1...3)]
            TokenNode  [( [4...5)]
            Expression  [x > 1]
                TokenOperator  [> [7...8)]
                    Variable  [x]
                    TokenNode  [> [7...8)]
                    NumericalValue  [1 [9...10)]
            TokenNode  [) [10...11)]
            SimpleScope  [17...23)
                ExpressionStatement  [x <- 1]
                    Expression  [x <- 1]
                        TokenOperator  [<- [19...21)]
                            Variable  [x]
                            TokenNode  [<- [19...21)]
                            NumericalValue  [1 [22...23)]
            KeywordScopeStatement  []
                TokenNode  [else [25...29)]

CloseCurlyBraceExpected AfterToken [25...29)
";
            ParserTest.VerifyParse(expected, "{if (x > 1)\r\n    x <- 1\r\nelse\n");
        }
    }
}
