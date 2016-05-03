// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.R.Core.Test.Utility;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.R.Core.Test.Parser {
    [ExcludeFromCodeCoverage]  
    public class ParseMultilineExpressionsTest {
        [Test]
        [Category.R.Parser]
        public void ParseMultilineExpressionsTest01() {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [x+ \n 1 * ( \r\n a + b)]
        Expression  [x+ \n 1 * ( \r\n a + b)]
            TokenOperator  [+ [1...2)]
                Variable  [x]
                TokenNode  [+ [1...2)]
                TokenOperator  [* [7...8)]
                    NumericalValue  [1 [5...6)]
                    TokenNode  [* [7...8)]
                    Group  [9...20)
                        TokenNode  [( [9...10)]
                        Expression  [a + b]
                            TokenOperator  [+ [16...17)]
                                Variable  [a]
                                TokenNode  [+ [16...17)]
                                Variable  [b]
                        TokenNode  [) [19...20)]
";
            ParserTest.VerifyParse(expected, "x+ \n 1 * ( \r\n a + b)");
        }

        [Test]
        [Category.R.Parser]
        public void ParseMultilineExpressionsTest02() {
            string expected =
@"GlobalScope  [Global]
    KeywordStatement  []
        TokenNode  [break [0...5)]
    EmptyStatement  [8...9)
        TokenNode  [; [8...9)]
";
            ParserTest.VerifyParse(expected, "break \n ;");
        }

        [Test]
        [Category.R.Parser]
        public void ParseMultilineExpressionsTest03() {
            string expected =
@"GlobalScope  [Global]
    For  []
        TokenNode  [for [0...3)]
        TokenNode  [( [7...8)]
        EnumerableExpression  [8...18)
            Variable  [x]
            TokenNode  [in [12...14)]
            Expression  [y]
                Variable  [y]
        TokenNode  [) [18...19)]
        Scope  []
            TokenNode  [{ [22...23)]
            TokenNode  [} [26...27)]
";
            ParserTest.VerifyParse(expected, "for \r\n (x \n in \n y) \n { \n }");
        }

        [Test]
        [Category.R.Parser]
        public void ParseMultilineExpressionsTest04() {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [x + 1]
        Expression  [x + 1]
            TokenOperator  [+ [2...3)]
                Variable  [x]
                TokenNode  [+ [2...3)]
                NumericalValue  [1 [4...5)]
    ExpressionStatement  [+ 2]
        Expression  [+ 2]
            TokenOperator  [+ [8...9)]
                TokenNode  [+ [8...9)]
                NumericalValue  [2 [10...11)]
";
            ParserTest.VerifyParse(expected, "x + 1 \n + 2");
        }

        [Test]
        [Category.R.Parser]
        public void ParseMultilineExpressionsTest05() {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [(x\n || y)]
        Expression  [(x\n || y)]
            Group  [0...9)
                TokenNode  [( [0...1)]
                Expression  [x\n || y]
                    TokenOperator  [|| [4...6)]
                        Variable  [x]
                        TokenNode  [|| [4...6)]
                        Variable  [y]
                TokenNode  [) [8...9)]
";
            ParserTest.VerifyParse(expected, "(x\n || y)");
        }

        [Test]
        [Category.R.Parser]
        public void ParseMultilineExpressionsTest06() {
            string expected =
@"GlobalScope  [Global]
    If  []
        TokenNode  [if [0...2)]
        TokenNode  [( [2...3)]
        Expression  [x\n || y]
            TokenOperator  [|| [6...8)]
                Variable  [x]
                TokenNode  [|| [6...8)]
                Variable  [y]
        TokenNode  [) [10...11)]
";
            ParserTest.VerifyParse(expected, "if(x\n || y)");
        }

        [Test]
        [Category.R.Parser]
        public void ParseMultilineExpressionsTest07() {
            string expected =
@"GlobalScope  [Global]
    KeywordExpressionScopeStatement  []
        TokenNode  [while [0...5)]
        TokenNode  [( [5...6)]
        Expression  [x\n || \ny]
            TokenOperator  [|| [9...11)]
                Variable  [x]
                TokenNode  [|| [9...11)]
                Variable  [y]
        TokenNode  [) [14...15)]
";
            ParserTest.VerifyParse(expected, "while(x\n || \ny)");
        }

        [Test]
        [Category.R.Parser]
        public void ParseMultilineExpressionsTest08() {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [x <-\nfunction() { }]
        Expression  [x <-\nfunction() { }]
            TokenOperator  [<- [2...4)]
                Variable  [x]
                TokenNode  [<- [2...4)]
                FunctionDefinition  [5...19)
                    TokenNode  [function [5...13)]
                    TokenNode  [( [13...14)]
                    TokenNode  [) [14...15)]
                    Scope  []
                        TokenNode  [{ [16...17)]
                        TokenNode  [} [18...19)]
";
            ParserTest.VerifyParse(expected, "x <-\nfunction() { }");
        }

    }
}
