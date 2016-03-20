// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.R.Core.Test.Utility;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.R.Core.Test.Parser {
    [ExcludeFromCodeCoverage]
    public class ParseFunctionsTest {
        [Test]
        [Category.R.Parser]
        public void ParseFunctionsTest01() {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [a()]
        Expression  [a()]
            FunctionCall  [0...3)
                Variable  [a]
                TokenNode  [( [1...2)]
                TokenNode  [) [2...3)]
";
            ParserTest.VerifyParse(expected, "a()");
        }

        [Test]
        [Category.R.Parser]
        public void ParseFunctionsTest02() {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [a(1)]
        Expression  [a(1)]
            FunctionCall  [0...4)
                Variable  [a]
                TokenNode  [( [1...2)]
                ArgumentList  [2...3)
                    ExpressionArgument  [2...3)
                        Expression  [1]
                            NumericalValue  [1 [2...3)]
                TokenNode  [) [3...4)]
";
            ParserTest.VerifyParse(expected, "a(1)");
        }

        [Test]
        [Category.R.Parser]
        public void ParseFunctionsTest03() {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [a(1,2)]
        Expression  [a(1,2)]
            FunctionCall  [0...6)
                Variable  [a]
                TokenNode  [( [1...2)]
                ArgumentList  [2...5)
                    ExpressionArgument  [2...4)
                        Expression  [1]
                            NumericalValue  [1 [2...3)]
                        TokenNode  [, [3...4)]
                    ExpressionArgument  [4...5)
                        Expression  [2]
                            NumericalValue  [2 [4...5)]
                TokenNode  [) [5...6)]
";
            ParserTest.VerifyParse(expected, "a(1,2)");
        }

        [Test]
        [Category.R.Parser]
        public void ParseFunctionsTest04() {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [x(a, b=NA, c=NULL, ...)]
        Expression  [x(a, b=NA, c=NULL, ...)]
            FunctionCall  [0...23)
                Variable  [x]
                TokenNode  [( [1...2)]
                ArgumentList  [2...22)
                    ExpressionArgument  [2...4)
                        Expression  [a]
                            Variable  [a]
                        TokenNode  [, [3...4)]
                    NamedArgument  [5...10)
                        TokenNode  [b [5...6)]
                        TokenNode  [= [6...7)]
                        Expression  [NA]
                            MissingValue  [NA [7...9)]
                        TokenNode  [, [9...10)]
                    NamedArgument  [11...18)
                        TokenNode  [c [11...12)]
                        TokenNode  [= [12...13)]
                        Expression  [NULL]
                            NullValue  [NULL [13...17)]
                        TokenNode  [, [17...18)]
                    EllipsisArgument  [...]
                        TokenNode  [... [19...22)]
                TokenNode  [) [22...23)]
";
            ParserTest.VerifyParse(expected, "x(a, b=NA, c=NULL, ...)");
        }

        [Test]
        [Category.R.Parser]
        public void ParseFunctionsTest05() {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [x(,, ]
        Expression  [x(,, ]
            FunctionCall  [0...5)
                Variable  [x]
                TokenNode  [( [1...2)]
                ArgumentList  [2...4)
                    MissingArgument  [{Missing}]
                        TokenNode  [, [2...3)]
                    MissingArgument  [{Missing}]
                        TokenNode  [, [3...4)]
                    StubArgument  [{Stub}]

CloseBraceExpected AfterToken [3...4)
";
            ParserTest.VerifyParse(expected, "x(,, ");
        }

        [Test]
        [Category.R.Parser]
        public void ParseFunctionsTest06() {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [x(,, ]
        Expression  [x(,, ]
            FunctionCall  [0...5)
                Variable  [x]
                TokenNode  [( [1...2)]
                ArgumentList  [2...4)
                    MissingArgument  [{Missing}]
                        TokenNode  [, [2...3)]
                    MissingArgument  [{Missing}]
                        TokenNode  [, [3...4)]
                    StubArgument  [{Stub}]

CloseBraceExpected AfterToken [3...4)
UnexpectedToken AfterToken [3...4)
OpenBraceExpected AfterToken [5...8)
";
            ParserTest.VerifyParse(expected, "x(,, for");
        }

        [Test]
        [Category.R.Parser]
        public void ParseFunctionsTest07() {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [x(a=,b=)]
        Expression  [x(a=,b=)]
            FunctionCall  [0...8)
                Variable  [x]
                TokenNode  [( [1...2)]
                ArgumentList  [2...7)
                    NamedArgument  [2...5)
                        TokenNode  [a [2...3)]
                        TokenNode  [= [3...4)]
                        TokenNode  [, [4...5)]
                    NamedArgument  [5...7)
                        TokenNode  [b [5...6)]
                        TokenNode  [= [6...7)]
                TokenNode  [) [7...8)]
";
            ParserTest.VerifyParse(expected, "x(a=,b=)");
        }

        [Test]
        [Category.R.Parser]
        public void ParseFunctionsTest08() {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [x(a=,)]
        Expression  [x(a=,)]
            FunctionCall  [0...6)
                Variable  [x]
                TokenNode  [( [1...2)]
                ArgumentList  [2...5)
                    NamedArgument  [2...5)
                        TokenNode  [a [2...3)]
                        TokenNode  [= [3...4)]
                        TokenNode  [, [4...5)]
                    StubArgument  [{Stub}]
                TokenNode  [) [5...6)]
";
            ParserTest.VerifyParse(expected, "x(a=,)");
        }

        [Test]
        [Category.R.Parser]
        public void ParseFunctionsTest09() {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [x('s'=, 1.0='z')]
        Expression  [x('s'=, 1.0='z')]
            FunctionCall  [0...16)
                Variable  [x]
                TokenNode  [( [1...2)]
                ArgumentList  [2...15)
                    NamedArgument  [2...7)
                        TokenNode  ['s' [2...5)]
                        TokenNode  [= [5...6)]
                        TokenNode  [, [6...7)]
                    ErrorArgument  [8...15)
                        TokenNode  [1.0 [8...11)]
                        TokenNode  [= [11...12)]
                        TokenNode  ['z' [12...15)]
                TokenNode  [) [15...16)]

IndentifierExpected Token [8...11)
";
            ParserTest.VerifyParse(expected, "x(\'s\'=, 1.0='z')");
        }
    }
}
