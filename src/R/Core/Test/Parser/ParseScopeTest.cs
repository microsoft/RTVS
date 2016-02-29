// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.R.Core.Test.Utility;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.R.Core.Test.Parser {
    [ExcludeFromCodeCoverage]
    public class ParseScopeTest {
        [Test]
        [Category.R.Parser]
        public void ParseScopeTest01() {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [x <- as.matrix(x)]
        Expression  [x <- as.matrix(x)]
            TokenOperator  [<- [2...4)]
                Variable  [x]
                TokenNode  [<- [2...4)]
                FunctionCall  [5...17)
                    Variable  [as.matrix]
                    TokenNode  [( [14...15)]
                    ArgumentList  [15...16)
                        ExpressionArgument  [15...16)
                            Expression  [x]
                                Variable  [x]
                    TokenNode  [) [16...17)]
    ExpressionStatement  [y <- as.matrix(y)]
        Expression  [y <- as.matrix(y)]
            TokenOperator  [<- [22...24)]
                Variable  [y]
                TokenNode  [<- [22...24)]
                FunctionCall  [25...37)
                    Variable  [as.matrix]
                    TokenNode  [( [34...35)]
                    ArgumentList  [35...36)
                        ExpressionArgument  [35...36)
                            Expression  [y]
                                Variable  [y]
                    TokenNode  [) [36...37)]
";
            ParserTest.VerifyParse(expected, "x <- as.matrix(x) \n y <- as.matrix(y)");
        }

        [Test]
        [Category.R.Parser]
        public void ParseScopeTest02() {
            string expected =
@"GlobalScope  [Global]
    Scope  []
        TokenNode  [{ [0...1)]
        Scope  []
            TokenNode  [{ [1...2)]
            TokenNode  [} [2...3)]

CloseCurlyBraceExpected AfterToken [2...3)
";
            ParserTest.VerifyParse(expected, "{{}");
        }

        [Test]
        [Category.R.Parser]
        public void ParseScopeTest03() {
            string expected =
@"GlobalScope  [Global]
    Scope  []
        TokenNode  [{ [0...1)]

CloseCurlyBraceExpected AfterToken [0...1)
";
            ParserTest.VerifyParse(expected, "{");
        }

        [Test]
        [Category.R.Parser]
        public void ParseScopeTest04() {
            string expected =
@"GlobalScope  [Global]
    Scope  []
        TokenNode  [{ [0...1)]
        TokenNode  [} [1...2)]
";
            ParserTest.VerifyParse(expected, "{}");
        }
    }
}
