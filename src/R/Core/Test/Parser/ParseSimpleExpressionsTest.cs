// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.R.Core.Test.Utility;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.R.Core.Test.Parser {
    [ExcludeFromCodeCoverage]   
    public class ParseSimpleExpressionsTest {
        [Test]
        [Category.R.Parser]
        public void ParseSimpleExpressions01() {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [a*b+c]
        Expression  [a*b+c]
            TokenOperator  [+ [3...4)]
                TokenOperator  [* [1...2)]
                    Variable  [a]
                    TokenNode  [* [1...2)]
                    Variable  [b]
                TokenNode  [+ [3...4)]
                Variable  [c]
";
            ParserTest.VerifyParse(expected, "a*b+c");
        }

        [Test]
        [Category.R.Parser]
        public void ParseSimpleExpressions02() {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [a*b+c*d]
        Expression  [a*b+c*d]
            TokenOperator  [+ [3...4)]
                TokenOperator  [* [1...2)]
                    Variable  [a]
                    TokenNode  [* [1...2)]
                    Variable  [b]
                TokenNode  [+ [3...4)]
                TokenOperator  [* [5...6)]
                    Variable  [c]
                    TokenNode  [* [5...6)]
                    Variable  [d]
";
            ParserTest.VerifyParse(expected, "a*b+c*d");
        }

        [Test]
        [Category.R.Parser]
        public void ParseSimpleExpressions03() {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [a*(b+c)]
        Expression  [a*(b+c)]
            TokenOperator  [* [1...2)]
                Variable  [a]
                TokenNode  [* [1...2)]
                Group  [2...7)
                    TokenNode  [( [2...3)]
                    Expression  [b+c]
                        TokenOperator  [+ [4...5)]
                            Variable  [b]
                            TokenNode  [+ [4...5)]
                            Variable  [c]
                    TokenNode  [) [6...7)]
";
            ParserTest.VerifyParse(expected, "a*(b+c)");
        }

        [Test]
        [Category.R.Parser]
        public void ParseSimpleExpressions04() {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [((x))]
        Expression  [((x))]
            Group  [0...5)
                TokenNode  [( [0...1)]
                Expression  [(x)]
                    Group  [1...4)
                        TokenNode  [( [1...2)]
                        Expression  [x]
                            Variable  [x]
                        TokenNode  [) [3...4)]
                TokenNode  [) [4...5)]
";
            ParserTest.VerifyParse(expected, "((x))");
        }

        [Test]
        [Category.R.Parser]
        public void ParseSimpleExpressions05() {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [((x))+1]
        Expression  [((x))+1]
            TokenOperator  [+ [5...6)]
                Group  [0...5)
                    TokenNode  [( [0...1)]
                    Expression  [(x)]
                        Group  [1...4)
                            TokenNode  [( [1...2)]
                            Expression  [x]
                                Variable  [x]
                            TokenNode  [) [3...4)]
                    TokenNode  [) [4...5)]
                TokenNode  [+ [5...6)]
                NumericalValue  [1 [6...7)]
";
            ParserTest.VerifyParse(expected, "((x))+1");
        }

        [Test]
        [Category.R.Parser]
        public void ParseSimpleExpressions06() {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [(x)]
        Expression  [(x)]
            Group  [0...3)
                TokenNode  [( [0...1)]
                Expression  [x]
                    Variable  [x]
                TokenNode  [) [2...3)]
";
            ParserTest.VerifyParse(expected, "(x)");
        }

        [Test]
        [Category.R.Parser]
        public void ParseSimpleExpressions07() {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [x]
        Expression  [x]
            Variable  [x]
";
            ParserTest.VerifyParse(expected, "x");
        }

        [Test]
        [Category.R.Parser]
        public void ParseSimpleExpressions08() {
            string expected =
@"GlobalScope  [Global]
";
            ParserTest.VerifyParse(expected, "");
        }

        [Test]
        [Category.R.Parser]
        public void ParseSimpleExpressions09() {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [((c+1)/2.0)]
        Expression  [((c+1)/2.0)]
            Group  [0...11)
                TokenNode  [( [0...1)]
                Expression  [(c+1)/2.0]
                    TokenOperator  [/ [6...7)]
                        Group  [1...6)
                            TokenNode  [( [1...2)]
                            Expression  [c+1]
                                TokenOperator  [+ [3...4)]
                                    Variable  [c]
                                    TokenNode  [+ [3...4)]
                                    NumericalValue  [1 [4...5)]
                            TokenNode  [) [5...6)]
                        TokenNode  [/ [6...7)]
                        NumericalValue  [2.0 [7...10)]
                TokenNode  [) [10...11)]
";
            ParserTest.VerifyParse(expected, "((c+1)/2.0)");
        }

        [Test]
        [Category.R.Parser]
        public void ParseSimpleExpressions10() {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [FALSE & TRUE]
        Expression  [FALSE & TRUE]
            TokenOperator  [& [6...7)]
                LogicalValue  [FALSE [0...5)]
                TokenNode  [& [6...7)]
                LogicalValue  [TRUE [8...12)]
";
            ParserTest.VerifyParse(expected, "FALSE & TRUE");
        }

        [Test]
        [Category.R.Parser]
        public void ParseSimpleExpressions11() {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [x <- ...]
        Expression  [x <- ...]
            TokenOperator  [<- [2...4)]
                Variable  [x]
                TokenNode  [<- [2...4)]
                Variable  [...]
";
            ParserTest.VerifyParse(expected, "x <- ...");
        }
    }
}
