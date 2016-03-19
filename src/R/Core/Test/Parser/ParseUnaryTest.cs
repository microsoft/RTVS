// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.R.Core.Test.Utility;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.R.Core.Test.Parser {
    [ExcludeFromCodeCoverage]
    public class ParseUnaryTest {
        [Test]
        [Category.R.Parser]
        public void Unary1() {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [-a$b]
        Expression  [-a$b]
            TokenOperator  [- [0...1)]
                TokenNode  [- [0...1)]
                TokenOperator  [$ [2...3)]
                    Variable  [a]
                    TokenNode  [$ [2...3)]
                    Variable  [b]
";
            ParserTest.VerifyParse(expected, "-a");
        }

        [Test]
        [Category.R.Parser]
        public void Unary2() {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [a$b()]
        Expression  [a$b()]
            FunctionCall  [0...5)
                TokenOperator  [$ [1...2)]
                    Variable  [a]
                    TokenNode  [$ [1...2)]
                    Variable  [b]
                TokenNode  [( [3...4)]
                TokenNode  [) [4...5)]
";
            ParserTest.VerifyParse(expected, "--a++b");
        }

        [Test]
        [Category.R.Parser]
        public void Unary3() {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [a$b()]
        Expression  [a$b()]
            FunctionCall  [0...5)
                TokenOperator  [$ [1...2)]
                    Variable  [a]
                    TokenNode  [$ [1...2)]
                    Variable  [b]
                TokenNode  [( [3...4)]
                TokenNode  [) [4...5)]
";
            ParserTest.VerifyParse(expected, "a--b^+3");
        }
    }
}
