using System.Diagnostics.CodeAnalysis;
using Microsoft.Languages.Core.Test.Utility;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.Parser;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.R.Core.Test.Parser
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class ParseCommentsTest : UnitTestBase
    {
        [TestMethod]
        [TestCategory("R.Parser")]
        public void ParseCommentsTest01()
        {
            AstRoot ast = RParser.Parse("#Not");
            Assert.AreEqual(1, ast.Comments.Count);
            Assert.AreEqual(0, ast.Comments[0].Start);
            Assert.AreEqual(4, ast.Comments[0].Length);

            Assert.IsFalse(ast.Comments.Contains(0));
            Assert.IsTrue(ast.Comments.Contains(1));
            Assert.IsTrue(ast.Comments.Contains(4));
        }
    }
}
