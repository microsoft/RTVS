// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.R.Core.Test.Utility;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.R.Core.Test.Parser {
    [ExcludeFromCodeCoverage]
    public class ParseSequenceTest {
        [Test]
        [Category.R.Parser]
        public void Selector1() {
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
            ParserTest.VerifyParse(expected, "-a$b");
        }

        [Test]
        [Category.R.Parser]
        public void Selector2() {
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
            ParserTest.VerifyParse(expected, "a$b()");
        }

        [Test]
        [Category.R.Parser]
        public void Selector3() {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [1:a()]
        Expression  [1:a()]
            TokenOperator  [: [1...2)]
                NumericalValue  [1 [0...1)]
                TokenNode  [: [1...2)]
                FunctionCall  [2...5)
                    Variable  [a]
                    TokenNode  [( [3...4)]
                    TokenNode  [) [4...5)]
";
            ParserTest.VerifyParse(expected, "1:a()");
        }

        [Test]
        [Category.R.Parser]
        public void Selector4() {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [x()$...]
        Expression  [x()$...]
            TokenOperator  [$ [3...4)]
                FunctionCall  [0...3)
                    Variable  [x]
                    TokenNode  [( [1...2)]
                    TokenNode  [) [2...3)]
                TokenNode  [$ [3...4)]
                Variable  [...]
";
            ParserTest.VerifyParse(expected, "x()$...");
        }
    }
}
