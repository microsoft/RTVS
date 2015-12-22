using System.Diagnostics.CodeAnalysis;
using Microsoft.Common.Core.Tests.Utility;
using Microsoft.R.Core.Test.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.R.Core.Test.Parser {
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class ParseKnownConstantsTest : UnitTestBase {
        [TestMethod]
        [TestCategory("R.Parser")]
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

        [TestMethod]
        [TestCategory("R.Parser")]
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
