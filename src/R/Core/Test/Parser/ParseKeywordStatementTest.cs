// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.R.Core.Test.Utility;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.R.Core.Test.Parser {
    [ExcludeFromCodeCoverage]   
    public class ParseKeywordStatement {
        [Test]
        [Category.R.Parser]
        public void ParseBreakTest1() {
            string expected =
@"GlobalScope  [Global]
    KeywordStatement  []
        TokenNode  [break [0...5)]
";
            ParserTest.VerifyParse(expected, "break");
        }

        [Test]
        [Category.R.Parser]
        public void ParseBreakTest2() {
            string expected =
@"GlobalScope  [Global]
    KeywordStatement  [;]
        TokenNode  [break [0...5)]
        TokenNode  [; [5...6)]
";
            ParserTest.VerifyParse(expected, "break;");
        }

        [Test]
        [Category.R.Parser]
        public void ParseNextTest1() {
            string expected =
@"GlobalScope  [Global]
    KeywordStatement  []
        TokenNode  [next [0...4)]
";
            ParserTest.VerifyParse(expected, "next");
        }

        [Test]
        [Category.R.Parser]
        public void ParseNextTest2() {
            string expected =
@"GlobalScope  [Global]
    KeywordStatement  [;]
        TokenNode  [next [0...4)]
        TokenNode  [; [4...5)]
";
            ParserTest.VerifyParse(expected, "next;");
        }

        [Test]
        [Category.R.Parser]
        public void ParseLibraryTest01() {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [library(abind)]
        Expression  [library(abind)]
            FunctionCall  [0...14)
                Variable  [library]
                TokenNode  [( [7...8)]
                ArgumentList  [8...13)
                    ExpressionArgument  [8...13)
                        Expression  [abind]
                            Variable  [abind]
                TokenNode  [) [13...14)]
";
            ParserTest.VerifyParse(expected, "library(abind)");
        }

        [Test]
        [Category.R.Parser]
        public void ParseLibraryTest02() {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [library('abind')]
        Expression  [library('abind')]
            FunctionCall  [0...16)
                Variable  [library]
                TokenNode  [( [7...8)]
                ArgumentList  [8...15)
                    ExpressionArgument  [8...15)
                        Expression  ['abind']
                            StringValue  ['abind' [8...15)]
                TokenNode  [) [15...16)]
";
            ParserTest.VerifyParse(expected, "library('abind')");
        }

        [Test]
        [Category.R.Parser]
        public void ParseLibraryTest03() {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [library()]
        Expression  [library()]
            FunctionCall  [0...9)
                Variable  [library]
                TokenNode  [( [7...8)]
                TokenNode  [) [8...9)]
";
            ParserTest.VerifyParse(expected, "library()");
        }

        [Test]
        [Category.R.Parser]
        public void ParseReturnTest01() {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [return()]
        Expression  [return()]
            FunctionCall  [0...8)
                Variable  [return]
                TokenNode  [( [6...7)]
                TokenNode  [) [7...8)]
";
            ParserTest.VerifyParse(expected, "return()");
        }

        [Test]
        [Category.R.Parser]
        public void ParseTypeofTest() {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [typeof(1)]
        Expression  [typeof(1)]
            FunctionCall  [0...9)
                Variable  [typeof]
                TokenNode  [( [6...7)]
                ArgumentList  [7...8)
                    ExpressionArgument  [7...8)
                        Expression  [1]
                            NumericalValue  [1 [7...8)]
                TokenNode  [) [8...9)]
";
            ParserTest.VerifyParse(expected, "typeof(1)");
        }

        [Test]
        [Category.R.Parser]
        public void ParseSwitchTest() {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [switch(1,2)]
        Expression  [switch(1,2)]
            FunctionCall  [0...11)
                Variable  [switch]
                TokenNode  [( [6...7)]
                ArgumentList  [7...10)
                    ExpressionArgument  [7...9)
                        Expression  [1]
                            NumericalValue  [1 [7...8)]
                        TokenNode  [, [8...9)]
                    ExpressionArgument  [9...10)
                        Expression  [2]
                            NumericalValue  [2 [9...10)]
                TokenNode  [) [10...11)]
";
            ParserTest.VerifyParse(expected, "switch(1,2)");
        }
    }
}
