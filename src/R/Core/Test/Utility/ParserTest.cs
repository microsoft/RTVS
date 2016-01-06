using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using FluentAssertions;
using Microsoft.Common.Core.Test.Utility;
using Microsoft.Languages.Core.Text;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.Parser;
using Microsoft.R.Core.Utility;
using Xunit;

namespace Microsoft.R.Core.Test.Utility {
    [ExcludeFromCodeCoverage]
    public static class ParserTest
    {
        public static AstRoot VerifyParse(string expected, string expression)
        {
            AstRoot ast = RParser.Parse(new TextStream(expression));
            ParserTest.CompareTrees(expected, ast);
            return ast;
        }

        public static void CompareTrees(string expected, AstRoot actualTree)
        {
            AstWriter astWriter = new AstWriter();
            string actual = astWriter.WriteTree(actualTree);
            
            string expectedLine, actualLine;
            int result = BaselineCompare.CompareLines(expected, actual, out expectedLine, out actualLine);

            result.Should().Be(0, "Line at {0} should be {1}, but found {2}", result, expectedLine, actualLine);
        }
    }
}
