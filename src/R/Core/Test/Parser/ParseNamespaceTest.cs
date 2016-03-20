// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.R.Core.Test.Utility;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.R.Core.Test.Parser {
    [ExcludeFromCodeCoverage]
    public class ParseNamespaceTest {
        [Test]
        [Category.R.Parser]
        public void Namespace01() {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [a::b+c]
        Expression  [a::b+c]
            TokenOperator  [+ [4...5)]
                TokenOperator  [:: [1...3)]
                    Variable  [a]
                    TokenNode  [:: [1...3)]
                    Variable  [b]
                TokenNode  [+ [4...5)]
                Variable  [c]
";
            ParserTest.VerifyParse(expected, "a::b+c");
        }

        [Test]
        [Category.R.Parser]
        public void Namespace02() {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [a::b$c]
        Expression  [a::b$c]
            TokenOperator  [$ [4...5)]
                TokenOperator  [:: [1...3)]
                    Variable  [a]
                    TokenNode  [:: [1...3)]
                    Variable  [b]
                TokenNode  [$ [4...5)]
                Variable  [c]
";
            ParserTest.VerifyParse(expected, "a::b$c");
        }

        [Test]
        [Category.R.Parser]
        public void Namespace03() {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [a::b[1]$c]
        Expression  [a::b[1]$c]
            TokenOperator  [$ [7...8)]
                Indexer  [0...7)
                    TokenOperator  [:: [1...3)]
                        Variable  [a]
                        TokenNode  [:: [1...3)]
                        Variable  [b]
                    TokenNode  [[ [4...5)]
                    ArgumentList  [5...6)
                        ExpressionArgument  [5...6)
                            Expression  [1]
                                NumericalValue  [1 [5...6)]
                    TokenNode  [] [6...7)]
                TokenNode  [$ [7...8)]
                Variable  [c]
";
            ParserTest.VerifyParse(expected, "a::b[1]$c");
        }

        [Test]
        [Category.R.Parser]
        public void Namespace04() {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [a::b@c[1]]
        Expression  [a::b@c[1]]
            Indexer  [0...9)
                TokenOperator  [@ [4...5)]
                    TokenOperator  [:: [1...3)]
                        Variable  [a]
                        TokenNode  [:: [1...3)]
                        Variable  [b]
                    TokenNode  [@ [4...5)]
                    Variable  [c]
                TokenNode  [[ [6...7)]
                ArgumentList  [7...8)
                    ExpressionArgument  [7...8)
                        Expression  [1]
                            NumericalValue  [1 [7...8)]
                TokenNode  [] [8...9)]
";
            ParserTest.VerifyParse(expected, "a::b@c[1]");
        }

        [Test]
        [Category.R.Parser]
        public void Namespace05() {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [a:::b::c]
        Expression  [a:::b::c]
            TokenOperator  [:: [5...7)]
                TokenOperator  [::: [1...4)]
                    Variable  [a]
                    TokenNode  [::: [1...4)]
                    Variable  [b]
                TokenNode  [:: [5...7)]
                Variable  [c]
";
            ParserTest.VerifyParse(expected, "a:::b::c");
        }
    }
}
