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
    public static class ParserTest
    {
        public static void VerifyParse(string expected, string expression)
        {
            AstRoot ast = RParser.Parse(new TextStream(expression));
            ParserTest.CompareTrees(expected, ast);
         }

        public static void CompareTrees(string expected, AstRoot actualTree)
        {
            AstWriter astWriter = new AstWriter();
            string actual = astWriter.WriteTree(actualTree);
            
            string expectedLine, actualLine;
            int index;
            int result = BaselineCompare.CompareLines(expected, actual, out expectedLine, out actualLine, out index);

            result.Should().Be(0, "Line at {0} should be {1}, but found {2}, different at position {3}", result, expectedLine, actualLine, index);
        }
    }
}
