// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.R.Core.Test.Utility;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.R.Core.Test.Parser {
    [ExcludeFromCodeCoverage]
    [Category.R.Parser]
    public class ParseUnaryTest {
        [Test]
        public void Unary01() {
            const string expected = 
@"GlobalScope  [Global]
    ExpressionStatement  [-a]
        Expression  [-a]
            TokenOperator  [- [0...1)]
                TokenNode  [- [0...1)]
                Variable  [a]
";
            ParserTest.VerifyParse(expected, "-a");
        }

        [Test]
        public void Unary02() {
            const string expected = 
@"GlobalScope  [Global]
    ExpressionStatement  [-a+b]
        Expression  [-a+b]
            TokenOperator  [+ [2...3)]
                TokenOperator  [- [0...1)]
                    TokenNode  [- [0...1)]
                    Variable  [a]
                TokenNode  [+ [2...3)]
                Variable  [b]
";
            ParserTest.VerifyParse(expected, "-a+b");
        }

        [Test]
        public void Unary03() {
            const string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [--a++b]
        Expression  [--a++b]
            TokenOperator  [+ [3...4)]
                TokenOperator  [- [0...1)]
                    TokenNode  [- [0...1)]
                    TokenOperator  [- [1...2)]
                        TokenNode  [- [1...2)]
                        Variable  [a]
                TokenNode  [+ [3...4)]
                TokenOperator  [+ [4...5)]
                    TokenNode  [+ [4...5)]
                    Variable  [b]
";
            ParserTest.VerifyParse(expected, "--a++b");
        }

        [Test]
        public void Unary04() {
            const string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [a--b^+3]
        Expression  [a--b^+3]
            TokenOperator  [- [1...2)]
                Variable  [a]
                TokenNode  [- [1...2)]
                TokenOperator  [- [2...3)]
                    TokenNode  [- [2...3)]
                    TokenOperator  [^ [4...5)]
                        Variable  [b]
                        TokenNode  [^ [4...5)]
                        NumericalValue  [+3 [5...7)]
";
            ParserTest.VerifyParse(expected, "a--b^+3");
        }

        [Test]
        public void MultipleUnary01() {
            const string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [!!!TRUE]
        Expression  [!!!TRUE]
            TokenOperator  [! [0...1)]
                TokenNode  [! [0...1)]
                Expression  [!!TRUE]
                    TokenOperator  [! [1...2)]
                        TokenNode  [! [1...2)]
                        Expression  [!TRUE]
                            TokenOperator  [! [2...3)]
                                TokenNode  [! [2...3)]
                                Expression  [TRUE]
                                    LogicalValue  [TRUE [3...7)]
";
            const string content = "!!!TRUE";
            ParserTest.VerifyParse(expected, content);
        }

        [Test]
        public void MultipleUnary02() {
            const string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [1-+-+-3]
        Expression  [1-+-+-3]
            TokenOperator  [- [1...2)]
                NumericalValue  [1 [0...1)]
                TokenNode  [- [1...2)]
                TokenOperator  [+ [2...3)]
                    TokenNode  [+ [2...3)]
                    TokenOperator  [- [3...4)]
                        TokenNode  [- [3...4)]
                        TokenOperator  [+ [4...5)]
                            TokenNode  [+ [4...5)]
                            NumericalValue  [-3 [5...7)]
";
            const string content = "1-+-+-3";

            ParserTest.VerifyParse(expected, content);
        }

        [Test]
        public void MultipleUnary03() {
            const string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [1/+-+-3]
        Expression  [1/+-+-3]
            TokenOperator  [/ [1...2)]
                NumericalValue  [1 [0...1)]
                TokenNode  [/ [1...2)]
                TokenOperator  [+ [2...3)]
                    TokenNode  [+ [2...3)]
                    TokenOperator  [- [3...4)]
                        TokenNode  [- [3...4)]
                        TokenOperator  [+ [4...5)]
                            TokenNode  [+ [4...5)]
                            NumericalValue  [-3 [5...7)]
";
            string content = "1/+-+-3";

            ParserTest.VerifyParse(expected, content);
        }

        [Test]
        public void MissingOperand() {
            const string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [a]
        Expression  [a]
            Variable  [a]

RightOperandExpected Token [6...7)
";
            const string content = "a - ---";

            ParserTest.VerifyParse(expected, content);
        }
    }
}
