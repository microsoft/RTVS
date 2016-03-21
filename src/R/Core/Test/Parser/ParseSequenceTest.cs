// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.R.Core.Test.Utility;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.R.Core.Test.Parser {
    [ExcludeFromCodeCoverage]
    public class ParseSelectorsTest {
        [Test]
        [Category.R.Parser]
        public void Sequence1() {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [-1:2]
        Expression  [-1:2]
            TokenOperator  [: [2...3)]
                NumericalValue  [-1 [0...2)]
                TokenNode  [: [2...3)]
                NumericalValue  [2 [3...4)]
";
            ParserTest.VerifyParse(expected, "-1:2");
        }

        [Test]
        [Category.R.Parser]
        public void Sequence2() {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [1:2^3]
        Expression  [1:2^3]
            TokenOperator  [: [1...2)]
                NumericalValue  [1 [0...1)]
                TokenNode  [: [1...2)]
                TokenOperator  [^ [3...4)]
                    NumericalValue  [2 [2...3)]
                    TokenNode  [^ [3...4)]
                    NumericalValue  [3 [4...5)]
";
            ParserTest.VerifyParse(expected, "1:2^3");
        }

        [Test]
        [Category.R.Parser]
        public void Sequence3() {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [1:2+3]
        Expression  [1:2+3]
            TokenOperator  [+ [3...4)]
                TokenOperator  [: [1...2)]
                    NumericalValue  [1 [0...1)]
                    TokenNode  [: [1...2)]
                    NumericalValue  [2 [2...3)]
                TokenNode  [+ [3...4)]
                NumericalValue  [3 [4...5)]
";
            ParserTest.VerifyParse(expected, "1:2+3");
        }

        [Test]
        [Category.R.Parser]
        public void Sequence4() {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [-a:b]
        Expression  [-a:b]
            TokenOperator  [: [2...3)]
                TokenOperator  [- [0...1)]
                    TokenNode  [- [0...1)]
                    Variable  [a]
                TokenNode  [: [2...3)]
                Variable  [b]
";
            ParserTest.VerifyParse(expected, "-a:b");
        }

        [Test]
        [Category.R.Parser]
        public void Sequence5() {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [a^b:c]
        Expression  [a^b:c]
            TokenOperator  [: [3...4)]
                TokenOperator  [^ [1...2)]
                    Variable  [a]
                    TokenNode  [^ [1...2)]
                    Variable  [b]
                TokenNode  [: [3...4)]
                Variable  [c]
";
            ParserTest.VerifyParse(expected, "a^b:c");
        }
    }
}
