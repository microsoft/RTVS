// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.R.Core.Test.Utility;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.R.Core.Test.Parser {
    [ExcludeFromCodeCoverage]
    public class ParsePrecedenceTest {
        [Test]
        [Category.R.Parser]
        public void EqualLeftPrecedence1() {
            string expected =
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
        [Category.R.Parser]
        public void EqualLeftPrecedence2() {
            string expected =
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
