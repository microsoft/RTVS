// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.Common.Core.Test.Utility;
using Microsoft.Languages.Core.Text;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.Parser;
using Microsoft.R.Core.Utility;

namespace Microsoft.R.Core.Test.Utility {
    [ExcludeFromCodeCoverage]
    public static class ParserTest {
        public static void VerifyParse(string expected, string expression) {
            var ast = RParser.Parse(new TextStream(expression));
            CompareTrees(expected, ast);
        }

        public static void CompareTrees(string expected, AstRoot actualTree) {
            var astWriter = new AstWriter();
            var actual = astWriter.WriteTree(actualTree);

            var result = BaselineCompare.CompareLines(expected, actual, out var expectedLine, out var actualLine, out var index);
            result.Should().Be(0, "Line at {0} should be {1}, but found {2}, different at position {3}", result, expectedLine, actualLine, index);
        }
    }
}
