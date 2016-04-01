// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.R.Core.Test.Utility;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.R.Core.Test.Parser {
    [ExcludeFromCodeCoverage]
    public class ParseExpressionErrorsTest {
        [Test]
        [Category.R.Parser]
        public void ParseIncompleteExpressionTest01() {
            string expected =
@"GlobalScope  [Global]

RightOperandExpected Token [0...1)
";
            ParserTest.VerifyParse(expected, "+");
        }

        [Test]
        [Category.R.Parser]
        public void ParseIncompleteExpressionTest02() {
            string expected =
@"GlobalScope  [Global]

RightOperandExpected Token [1...2)
";
            ParserTest.VerifyParse(expected, "x+");
        }

        [Test]
        [Category.R.Parser]
        public void ParseIncompleteExpressionTest03() {
            string expected =
@"GlobalScope  [Global]

OperatorExpected Token [2...3)
";
            ParserTest.VerifyParse(expected, "a b");
        }

        [Test]
        [Category.R.Parser]
        public void ParseMismatchBracesTest01() {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [(]
        Expression  [(]
            Group  [0...1)
                TokenNode  [( [0...1)]

CloseBraceExpected AfterToken [0...1)
";
            ParserTest.VerifyParse(expected, "(");
        }

        [Test]
        [Category.R.Parser]
        public void ParseMismatchBracesTest02() {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [((x)]
        Expression  [((x)]
            Group  [0...4)
                TokenNode  [( [0...1)]
                Expression  [(x)]
                    Group  [1...4)
                        TokenNode  [( [1...2)]
                        Expression  [x]
                            Variable  [x]
                        TokenNode  [) [3...4)]

CloseBraceExpected AfterToken [3...4)
";
            ParserTest.VerifyParse(expected, "((x)");
        }

        [Test]
        [Category.R.Parser]
        public void ParseMismatchBracesTest03() {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [(]
        Expression  [(]
            Group  [0...1)
                TokenNode  [( [0...1)]

RightOperandExpected Token [2...3)
CloseBraceExpected AfterToken [2...3)
";
            ParserTest.VerifyParse(expected, "(x+");
        }

        [Test]
        [Category.R.Parser]
        public void ParseMismatchBracesTest04() {
            string expected =
@"GlobalScope  [Global]

RightOperandExpected Token [6...7)
";
            ParserTest.VerifyParse(expected, "(a+b)+)");
        }

        [Test]
        [Category.R.Parser]
        public void ParseMissingAssignmentTest01() {
            string expected =
@"GlobalScope  [Global]

OperatorExpected Token [2...10)
FunctionBodyExpected Token [12...13)
";
            ParserTest.VerifyParse(expected, "x function(a)");
        }

        [Test]
        [Category.R.Parser]
        public void ParseIncompleteOperatorTest01() {
            string expected =
@"GlobalScope  [Global]

RightOperandExpected Token [8...9)
";
            ParserTest.VerifyParse(expected, "y <- 2.5*");
        }

        [Test]
        [Category.R.Parser]
        public void ParseMissingOperatorTest01() {
            string expected =
@"GlobalScope  [Global]

OperatorExpected Token [3...4)
";
            ParserTest.VerifyParse(expected, "a()b");
        }

        [Test]
        [Category.R.Parser]
        public void ParseMissingOperatorTest02() {
            string expected =
@"GlobalScope  [Global]

OperatorExpected Token [2...3)
";
            ParserTest.VerifyParse(expected, "a b");
        }

        [Test]
        [Category.R.Parser]
        public void ParseMissingListOperand01() {
            string content =
@"
    fitted.zeros <- xzero * z$ }
";
            string expected =
@"GlobalScope  [Global]

RightOperandExpected Token [33...34)
UnexpectedToken Token [33...34)
";

            ParserTest.VerifyParse(expected, content);
        }
    }
}
