using System.Diagnostics.CodeAnalysis;
using Microsoft.Languages.Core.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Languages.Core.Test.Text {
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class BraceCounterTest {
        [TestMethod]
        [TestCategory("Languages.Core")]
        public void BraceCounterTest_SingleBraces() {
            BraceCounter<char> braceCounter = new BraceCounter<char>(new char[] { '{', '}'});
            string testString = " {{ { } } } ";
            int[] expectedCount = new int[] {0, 1, 2, 2, 3, 3, 2, 2, 1, 1, 0, 0};
            for (int i = 0; i < testString.Length; i++) {
                braceCounter.CountBrace(testString[i]);
                Assert.AreEqual(expectedCount[i], braceCounter.Count);
            }
        }

        [TestMethod]
        [TestCategory("Languages.Core")]
        public void BraceCounterTest_MultipleBraces() {
            BraceCounter<char> braceCounter = new BraceCounter<char>(new char[] { '{', '}', '[', ']' });
            string testString = " {[ { ] } } ";
            int[] expectedCount = new int[] { 0, 1, 2, 2, 3, 3, 2, 2, 1, 1, 0, 0 };
            for (int i = 0; i < testString.Length; i++) {
                braceCounter.CountBrace(testString[i]);
                Assert.AreEqual(expectedCount[i], braceCounter.Count);
            }
        }
    }
}
