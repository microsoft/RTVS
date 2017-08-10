// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.R.Core.Test.Utility;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.R.Core.Test.Parser {
    [ExcludeFromCodeCoverage]
    [Category.R.Parser]
    public class ParsePrecedenceTest {
        [Test]
        public void Precedence01() {
            const string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [a && !b]
        Expression  [a && !b]
            TokenOperator  [&& [2...4)]
                Variable  [a]
                TokenNode  [&& [2...4)]
                TokenOperator  [! [5...6)]
                    TokenNode  [! [5...6)]
                    Expression  [b]
                        Variable  [b]
";
            ParserTest.VerifyParse(expected, "a && !b");
        }

        [Test]
        public void EqualLeftPrecedence1() {
            const string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [a+b+c]
        Expression  [a+b+c]
            TokenOperator  [+ [3...4)]
                TokenOperator  [+ [1...2)]
                    Variable  [a]
                    TokenNode  [+ [1...2)]
                    Variable  [b]
                TokenNode  [+ [3...4)]
                Variable  [c]
";
            ParserTest.VerifyParse(expected, "a+b+c");
        }

        [Test]
        public void EqualLeftPrecedence2() {
            const string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [a-b+c]
        Expression  [a-b+c]
            TokenOperator  [+ [3...4)]
                TokenOperator  [- [1...2)]
                    Variable  [a]
                    TokenNode  [- [1...2)]
                    Variable  [b]
                TokenNode  [+ [3...4)]
                Variable  [c]
";
            ParserTest.VerifyParse(expected, "a-b+c");
        }
    }
}
