// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.R.Core.Test.Utility;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.R.Core.Test.Parser {
    [ExcludeFromCodeCoverage]    
    public class ParseLoopsTest {
        [Test]
        [Category.R.Parser]
        public void ParseRepeatTest() {
            string expected =
@"GlobalScope  [Global]
    KeywordScopeStatement  []
        TokenNode  [repeat [0...6)]
        Scope  []
            TokenNode  [{ [7...8)]
            ExpressionStatement  [x <- x+1]
                Expression  [x <- x+1]
                    TokenOperator  [<- [11...13)]
                        Variable  [x]
                        TokenNode  [<- [11...13)]
                        TokenOperator  [+ [15...16)]
                            Variable  [x]
                            TokenNode  [+ [15...16)]
                            NumericalValue  [1 [16...17)]
            TokenNode  [} [18...19)]
";
            ParserTest.VerifyParse(expected, "repeat { x <- x+1 }");
        }

        [Test]
        [Category.R.Parser]
        public void ParseWhileTest1() {
            string expected =
@"GlobalScope  [Global]
    KeywordExpressionScopeStatement  []
        TokenNode  [while [0...5)]
        TokenNode  [( [5...6)]
        Expression  [a+b > c+d]
            TokenOperator  [> [10...11)]
                TokenOperator  [+ [7...8)]
                    Variable  [a]
                    TokenNode  [+ [7...8)]
                    Variable  [b]
                TokenNode  [> [10...11)]
                TokenOperator  [+ [13...14)]
                    Variable  [c]
                    TokenNode  [+ [13...14)]
                    Variable  [d]
        TokenNode  [) [15...16)]
        Scope  []
            TokenNode  [{ [17...18)]
            ExpressionStatement  [a <- a+1]
                Expression  [a <- a+1]
                    TokenOperator  [<- [21...23)]
                        Variable  [a]
                        TokenNode  [<- [21...23)]
                        TokenOperator  [+ [25...26)]
                            Variable  [a]
                            TokenNode  [+ [25...26)]
                            NumericalValue  [1 [26...27)]
            TokenNode  [} [28...29)]
";
            ParserTest.VerifyParse(expected, "while(a+b > c+d) { a <- a+1 }");
        }

        [Test]
        [Category.R.Parser]
        public void ParseWhileTest2() {
            string expected =
@"GlobalScope  [Global]
    KeywordExpressionScopeStatement  []
        TokenNode  [while [0...5)]
        TokenNode  [( [5...6)]
        TokenNode  [) [12...13)]
        Scope  []
            TokenNode  [{ [14...15)]
            ExpressionStatement  [a <- a+1]
                Expression  [a <- a+1]
                    TokenOperator  [<- [18...20)]
                        Variable  [a]
                        TokenNode  [<- [18...20)]
                        TokenOperator  [+ [22...23)]
                            Variable  [a]
                            TokenNode  [+ [22...23)]
                            NumericalValue  [1 [23...24)]
            TokenNode  [} [25...26)]

RightOperandExpected Token [12...13)
";
            ParserTest.VerifyParse(expected, "while(a+b > ) { a <- a+1 }");
        }

        [Test]
        [Category.R.Parser]
        public void ParseWhileTest3() {
            string expected =
@"GlobalScope  [Global]
    KeywordExpressionScopeStatement  []
        TokenNode  [while [0...5)]
        TokenNode  [( [5...6)]
        Expression  [TRUE]
            LogicalValue  [TRUE [6...10)]
        TokenNode  [) [10...11)]
        Scope  []
            TokenNode  [{ [12...13)]
            TokenNode  [} [19...20)]

RightOperandExpected Token [19...20)
";
            ParserTest.VerifyParse(expected, "while(TRUE) { a <- }");
        }

        [Test]
        [Category.R.Parser]
        public void ParseForTest1() {
            string expected =
@"GlobalScope  [Global]
    For  []
        TokenNode  [for [0...3)]
        TokenNode  [( [3...4)]
        EnumerableExpression  [4...10)
            Variable  [x]
            TokenNode  [in [6...8)]
            Expression  [y]
                Variable  [y]
        TokenNode  [) [10...11)]
        Scope  []
            TokenNode  [{ [12...13)]
            TokenNode  [} [14...15)]
";
            ParserTest.VerifyParse(expected, "for(x in y) { }");
        }
    }
}
