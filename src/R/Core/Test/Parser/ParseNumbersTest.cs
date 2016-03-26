// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.R.Core.Test.Utility;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.R.Core.Test.Parser {
    [ExcludeFromCodeCoverage]
    public class ParseNumbersTest {
        [Test]
        [Category.R.Parser]
        public void ParseNumbers01() {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [1e4L]
        Expression  [1e4L]
            NumericalValue  [1e4L [0...4)]
";
            ParserTest.VerifyParse(expected, "1e4L");
        }

        [Test]
        [Category.R.Parser]
        public void ParseNumbers02() {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [3600L]
        Expression  [3600L]
            NumericalValue  [3600L [0...5)]
";
            ParserTest.VerifyParse(expected, "3600L");
        }

        [Test]
        [Category.R.Parser]
        public void ParseNumbers03() {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [2.4L]
        Expression  [2.4L]
            NumericalValue  [2.4L [0...4)]
";
            ParserTest.VerifyParse(expected, "2.4L");
        }

        [Test]
        [Category.R.Parser]
        public void ParseNumbers04() {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [3600L + x/1.0e+5L]
        Expression  [3600L + x/1.0e+5L]
            TokenOperator  [+ [6...7)]
                NumericalValue  [3600L [0...5)]
                TokenNode  [+ [6...7)]
                TokenOperator  [/ [9...10)]
                    Variable  [x]
                    TokenNode  [/ [9...10)]
                    NumericalValue  [1.0e+5L [10...17)]
";
            ParserTest.VerifyParse(expected, "3600L + x/1.0e+5L");
        }

        [Test]
        [Category.R.Parser]
        public void ParseNumbers05() {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [0x10000]
        Expression  [0x10000]
            NumericalValue  [0x10000 [0...7)]
";
            ParserTest.VerifyParse(expected, "0x10000");
        }

        [Test]
        [Category.R.Parser]
        public void ParseNumbers06() {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [-0x10000]
        Expression  [-0x10000]
            NumericalValue  [-0x10000 [0...8)]
";
            ParserTest.VerifyParse(expected, "-0x10000");
        }

        [Test]
        [Category.R.Parser]
        public void ParseNumbers07() {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [- 0x10000]
        Expression  [- 0x10000]
            TokenOperator  [- [0...1)]
                TokenNode  [- [0...1)]
                NumericalValue  [0x10000 [2...9)]
";
            ParserTest.VerifyParse(expected, "- 0x10000");
        }

        [Test]
        [Category.R.Parser]
        public void ParseNumbers08() {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [0x10000L]
        Expression  [0x10000L]
            NumericalValue  [0x10000L [0...8)]
";
            ParserTest.VerifyParse(expected, "0x10000L");
        }

        [Test]
        [Category.R.Parser]
        public void ParseNumbers09() {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [0x8 + 0x10000i]
        Expression  [0x8 + 0x10000i]
            TokenOperator  [+ [4...5)]
                NumericalValue  [0x8 [0...3)]
                TokenNode  [+ [4...5)]
                ComplexValue  [0x10000i [6...14)]
";
            ParserTest.VerifyParse(expected, "0x8 + 0x10000i");
        }
    }
}
