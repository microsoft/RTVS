using System;
using System.Globalization;
using Microsoft.Languages.Core.Test.Utility;
using Microsoft.Languages.Core.Text;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.Parser;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.R.Core.Test.Utility
{
    internal static class ParserTest
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
            int result = BaselineCompare.CompareLines(expected, actual, out expectedLine, out actualLine);

            Assert.AreEqual(0, result,
                String.Format(CultureInfo.InvariantCulture,
                    "\r\nDifferent at line {0}\r\nExpected: {1}\r\nActual: {2}", result, expectedLine, actualLine));
        }
    }
}
