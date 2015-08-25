using System.Diagnostics.CodeAnalysis;
using Microsoft.Languages.Core.Test.Utility;
using Microsoft.R.Core.Test.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.R.Core.Test.Parser
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class ParseStringContstantsTest : UnitTestBase
    {
        [TestMethod]
        public void ParseStringContstantsTest1()
        {
            string expected =
"GlobalScope  [Global]\r\n" +
"    ExpressionStatement  [\"str\" + 'abc']\r\n" +
"        Expression  [\"str\" + 'abc']\r\n" +
"            TokenOperator  [+ [6...7]]\r\n" +
"                StringValue  [\"str\" [0...5]]\r\n" +
"                TokenNode  [+ [6...7]]\r\n" +
"                StringValue  ['abc' [8...13]]\r\n";

            ParserTest.VerifyParse(expected, "\"str\" + 'abc'");
        }
    }
}
