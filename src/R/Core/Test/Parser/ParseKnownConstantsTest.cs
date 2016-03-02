// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.R.Core.Test.Utility;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.R.Core.Test.Parser {
    [ExcludeFromCodeCoverage]    
    public class ParseKnownConstantsTest {
        [Test]
        [Category.R.Parser]
        public void ParseKnownContstantsTest01() {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [NULL + NA]
        Expression  [NULL + NA]
            TokenOperator  [+ [5...6)]
                NullValue  [NULL [0...4)]
                TokenNode  [+ [5...6)]
                MissingValue  [NA [7...9)]
";

            ParserTest.VerifyParse(expected, "NULL + NA");
        }

        [Test]
        [Category.R.Parser]
        public void ParseKnownContstantsTest02() {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [Inf + NaN]
        Expression  [Inf + NaN]
            TokenOperator  [+ [4...5)]
                NumericalValue  [Inf [0...3)]
                TokenNode  [+ [4...5)]
                NumericalValue  [NaN [6...9)]
";

            ParserTest.VerifyParse(expected, "Inf + NaN");
        }
    }
}
