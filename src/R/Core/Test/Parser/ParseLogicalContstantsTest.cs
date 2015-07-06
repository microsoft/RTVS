using Microsoft.R.Core.Test.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.R.Core.Test.Tokens
{
    [TestClass]
    public class ParseLogicalConstantsTest : TokenizeTestBase
    {
        [TestMethod]
        public void ParseLogicalConstantsTest1()
        {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [FALSE + T + F]
        Expression  [FALSE + T + F]
            TokenOperator  [+ [10...11]]
                TokenOperator  [+ [6...7]]
                    LogicalValue  [FALSE [0...5]]
                    TokenNode  [+ [6...7]]
                    LogicalValue  [T [8...9]]
                TokenNode  [+ [10...11]]
                LogicalValue  [F [12...13]]
";
            ParserTest.VerifyParse(expected, "FALSE + T + F");
        }
    }
}
