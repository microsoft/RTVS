// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.R.Core.Test.Utility;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.R.Core.Test.Parser {
    [ExcludeFromCodeCoverage]
    public class ParseEmptyStatementTest {
        [Test]
        [Category.R.Parser]
        public void ParseEmptyStatementTest1() {
            string expected =
@"GlobalScope  [Global]
    EmptyStatement  [0...1)
        TokenNode  [; [0...1)]
    EmptyStatement  [1...2)
        TokenNode  [; [1...2)]
    EmptyStatement  [2...3)
        TokenNode  [; [2...3)]
";
            ParserTest.VerifyParse(expected, ";;;");
        }
    }
}
