// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.R.Core.Test.Utility;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.R.Core.Test.Parser {
    [ExcludeFromCodeCoverage]
    public class ParseComplexNumbersTest {
        [Test]
        [Category.R.Parser]
        public void ParseComplexNumbers01() {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [(1i+2)/(1e2+.1i)]
        Expression  [(1i+2)/(1e2+.1i)]
            TokenOperator  [/ [6...7)]
                Group  [0...6)
                    TokenNode  [( [0...1)]
                    Expression  [1i+2]
                        TokenOperator  [+ [3...4)]
                            ComplexValue  [1i [1...3)]
                            TokenNode  [+ [3...4)]
                            NumericalValue  [2 [4...5)]
                    TokenNode  [) [5...6)]
                TokenNode  [/ [6...7)]
                Group  [7...16)
                    TokenNode  [( [7...8)]
                    Expression  [1e2+.1i]
                        ComplexValue  [1e2+.1i [8...15)]
                    TokenNode  [) [15...16)]
";
            ParserTest.VerifyParse(expected, "(1i+2)/(1e2+.1i)");
        }

        [Test]
        [Category.R.Parser]
        public void ParseHexComplexNumbers01() {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [0xAi]
        Expression  [0xAi]
            ComplexValue  [0xAi [0...4)]
";
            ParserTest.VerifyParse(expected, "0xAi");
        }

        [Test]
        [Category.R.Parser]
        public void ParseHexComplexNumbers02() {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [0x8+0x10000i]
        Expression  [0x8+0x10000i]
            ComplexValue  [0x8+0x10000i [0...12)]
";
            ParserTest.VerifyParse(expected, "0x8+0x10000i");
        }

        [Test]
        [Category.R.Parser]
        public void ParseHexComplexNumbers03() {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [0x8L + 0x10000i]
        Expression  [0x8L + 0x10000i]
            TokenOperator  [+ [5...6)]
                NumericalValue  [0x8L [0...4)]
                TokenNode  [+ [5...6)]
                ComplexValue  [0x10000i [7...15)]
";
            ParserTest.VerifyParse(expected, "0x8L + 0x10000i");
        }

        [Test]
        [Category.R.Parser]
        public void ParseHexComplexNumbers04() {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [1.1L+0xAi]
        Expression  [1.1L+0xAi]
            ComplexValue  [1.1L+0xAi [0...9)]
";
            ParserTest.VerifyParse(expected, "1.1L+0xAi");
        }
    }
}
