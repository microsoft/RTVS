// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.R.Core.Test.Utility;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.R.Core.Test.Parser {
    [ExcludeFromCodeCoverage]
    
    public class ParseStringContstantsTest {
        [Test]
        [Category.R.Parser]
        public void ParseStringContstantsTest1() {
            string expected =
"GlobalScope  [Global]\r\n" +
"    ExpressionStatement  [\"str\" + 'abc']\r\n" +
"        Expression  [\"str\" + 'abc']\r\n" +
"            TokenOperator  [+ [6...7)]\r\n" +
"                StringValue  [\"str\" [0...5)]\r\n" +
"                TokenNode  [+ [6...7)]\r\n" +
"                StringValue  ['abc' [8...13)]\r\n";

            ParserTest.VerifyParse(expected, "\"str\" + 'abc'");
        }
    }
}
