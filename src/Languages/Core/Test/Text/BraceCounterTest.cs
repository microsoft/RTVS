using Microsoft.Languages.Core.Text;
using Microsoft.UnitTests.Core.XUnit;
using Xunit;

namespace Microsoft.Languages.Core.Tests.Text {
    public class BraceCounterTest {
        [Test]
        [Trait("Category", "Languages.Core")]
        public void BraceCounterTest_SingleBraces() {
            BraceCounter<char> braceCounter = new BraceCounter<char>(new char[] { '{', '}'});
            string testString = " {{ { } } } ";
            int[] expectedCount = new int[] {0, 1, 2, 2, 3, 3, 2, 2, 1, 1, 0, 0};
            for (int i = 0; i < testString.Length; i++) {
                braceCounter.CountBrace(testString[i]);
                Assert.Equal(expectedCount[i], braceCounter.Count);
            }
        }

        [Test]
        [Trait("Category", "Languages.Core")]
        public void BraceCounterTest_MultipleBraces() {
            BraceCounter<char> braceCounter = new BraceCounter<char>(new char[] { '{', '}', '[', ']' });
            string testString = " {[ { ] } } ";
            int[] expectedCount = new int[] { 0, 1, 2, 2, 3, 3, 2, 2, 1, 1, 0, 0 };
            for (int i = 0; i < testString.Length; i++) {
                braceCounter.CountBrace(testString[i]);
                Assert.Equal(expectedCount[i], braceCounter.Count);
            }
        }
    }
}
