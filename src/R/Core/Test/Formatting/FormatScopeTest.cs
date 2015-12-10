using System.Diagnostics.CodeAnalysis;
using Microsoft.Languages.Core.Test.Utility;
using Microsoft.R.Core.Formatting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.R.Core.Test.Formatting {
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class FormatScopeTest : UnitTestBase {
        [TestMethod]
        [TestCategory("R.Formatting")]
        public void Formatter_EmptyFileTest() {
            RFormatter f = new RFormatter();
            string s = f.Format(string.Empty);
            Assert.AreEqual(0, s.Length);
        }

        [TestMethod]
        [TestCategory("R.Formatting")]
        public void Formatter_FormatRandom01() {
            RFormatter f = new RFormatter();
            string original = "a   b 1.  2 Inf\tNULL";

            string actual = f.Format(original);

            Assert.AreEqual(@"a b 1. 2 Inf NULL", actual);
        }

        [TestMethod]
        [TestCategory("R.Formatting")]
        public void Formatter_StatementTest01() {
            RFormatter f = new RFormatter();
            string actual = f.Format("x<-2");
            string expected =
@"x <- 2";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [TestCategory("R.Formatting")]
        public void Formatter_FormatSimpleScopesTest01() {
            RFormatter f = new RFormatter();
            string actual = f.Format("{{}}");
            string expected =
@"{
  { }
}";
            Assert.AreEqual(expected, actual);
        }
    }
}
