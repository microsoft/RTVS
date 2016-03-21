// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.R.Core.Test.Utility;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.R.Core.Test.Parser {
    [ExcludeFromCodeCoverage]
    public class ParseExponentTest {
        [Test]
        [Category.R.Parser]
        public void Exponent1() {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [a^b]
        Expression  [a^b]
            TokenOperator  [^ [1...2)]
                Variable  [a]
                TokenNode  [^ [1...2)]
                Variable  [b]
";
            ParserTest.VerifyParse(expected, "a^b");
        }

        [Test]
        [Category.R.Parser]
        public void Exponent2() {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [a^b^c]
        Expression  [a^b^c]
            TokenOperator  [^ [1...2)]
                Variable  [a]
                TokenNode  [^ [1...2)]
                TokenOperator  [^ [3...4)]
                    Variable  [b]
                    TokenNode  [^ [3...4)]
                    Variable  [c]
";
            ParserTest.VerifyParse(expected, "a^b^c");
        }

        [Test]
        [Category.R.Parser]
        public void Exponent3() {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [a^(b+c)]
        Expression  [a^(b+c)]
            TokenOperator  [^ [1...2)]
                Variable  [a]
                TokenNode  [^ [1...2)]
                Group  [2...7)
                    TokenNode  [( [2...3)]
                    Expression  [b+c]
                        TokenOperator  [+ [4...5)]
                            Variable  [b]
                            TokenNode  [+ [4...5)]
                            Variable  [c]
                    TokenNode  [) [6...7)]
";
            ParserTest.VerifyParse(expected, "a^(b+c)");
        }

        [Test]
        [Category.R.Parser]
        public void Exponent4() {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [a^b::c]
        Expression  [a^b::c]
            TokenOperator  [^ [1...2)]
                Variable  [a]
                TokenNode  [^ [1...2)]
                TokenOperator  [:: [3...5)]
                    Variable  [b]
                    TokenNode  [:: [3...5)]
                    Variable  [c]
";
            ParserTest.VerifyParse(expected, "a^b::c");
        }

        [Test]
        [Category.R.Parser]
        public void Exponent5() {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [a^b^c^d]
        Expression  [a^b^c^d]
            TokenOperator  [^ [1...2)]
                Variable  [a]
                TokenNode  [^ [1...2)]
                TokenOperator  [^ [3...4)]
                    Variable  [b]
                    TokenNode  [^ [3...4)]
                    TokenOperator  [^ [5...6)]
                        Variable  [c]
                        TokenNode  [^ [5...6)]
                        Variable  [d]
";
            ParserTest.VerifyParse(expected, "a^b^c^d");
        }

        [Test]
        [Category.R.Parser]
        public void Exponent6() {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [(a^b)^c^d]
        Expression  [(a^b)^c^d]
            TokenOperator  [^ [5...6)]
                Group  [0...5)
                    TokenNode  [( [0...1)]
                    Expression  [a^b]
                        TokenOperator  [^ [2...3)]
                            Variable  [a]
                            TokenNode  [^ [2...3)]
                            Variable  [b]
                    TokenNode  [) [4...5)]
                TokenNode  [^ [5...6)]
                TokenOperator  [^ [7...8)]
                    Variable  [c]
                    TokenNode  [^ [7...8)]
                    Variable  [d]
";
            ParserTest.VerifyParse(expected, "(a^b)^c^d");
        }
    }
}
