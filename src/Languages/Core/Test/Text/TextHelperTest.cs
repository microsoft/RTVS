using Microsoft.Languages.Core.Text;
using Microsoft.UnitTests.Core.XUnit;
using Xunit;

namespace Microsoft.Languages.Core.Tests.Text {
    public class TextHelperTest {
        [Fact]
        [Trait("Category", "Languages.Core")]
        public void TextHelperTest_IsNewLineBeforePositionTest() {
            ITextProvider tp = new TextStream("01\n34\r678\r\nBC");

            Assert.False(tp.IsNewLineBeforePosition(0));
            Assert.False(tp.IsNewLineBeforePosition(1));
            Assert.False(tp.IsNewLineBeforePosition(2));
            Assert.True(tp.IsNewLineBeforePosition(3));
            Assert.False(tp.IsNewLineBeforePosition(4));
            Assert.False(tp.IsNewLineBeforePosition(5));
            Assert.True(tp.IsNewLineBeforePosition(6));
            Assert.False(tp.IsNewLineBeforePosition(7));
            Assert.False(tp.IsNewLineBeforePosition(8));
            Assert.False(tp.IsNewLineBeforePosition(9));
            Assert.True(tp.IsNewLineBeforePosition(10));
            Assert.True(tp.IsNewLineBeforePosition(11));
            Assert.False(tp.IsNewLineBeforePosition(12));
        }

        [Fact]
        [Trait("Category", "Languages.Core")]
        public void TextHelperTest_IsNewLineAfterPositionTest() {
            ITextProvider tp = new TextStream("0 \n3 \r 7 \r\n  ");

            Assert.False(TextHelper.IsNewLineAfterPosition(tp, 0));
            Assert.True(TextHelper.IsNewLineAfterPosition(tp, 1));
            Assert.True(TextHelper.IsNewLineAfterPosition(tp, 2));
            Assert.False(TextHelper.IsNewLineAfterPosition(tp, 3));
            Assert.True(TextHelper.IsNewLineAfterPosition(tp, 4));
            Assert.True(TextHelper.IsNewLineAfterPosition(tp, 5));
            Assert.False(TextHelper.IsNewLineAfterPosition(tp, 6));
            Assert.False(TextHelper.IsNewLineAfterPosition(tp, 7));
            Assert.True(TextHelper.IsNewLineAfterPosition(tp, 8));
            Assert.True(TextHelper.IsNewLineAfterPosition(tp, 9));
            Assert.True(TextHelper.IsNewLineAfterPosition(tp, 10));
            Assert.False(TextHelper.IsNewLineAfterPosition(tp, 11));
            Assert.False(TextHelper.IsNewLineAfterPosition(tp, 12));
        }

        [Fact]
        [Trait("Category", "Languages.Core")]
        public void TextHelperTest_IsWhitespaceOnlyBetweenPositionsTest() {
            ITextProvider tp = new TextStream("0 \n3 \r 7 \r\n    AB ");

            Assert.False(TextHelper.IsWhitespaceOnlyBetweenPositions(tp, 0, 1));
            Assert.True(TextHelper.IsWhitespaceOnlyBetweenPositions(tp, 1, 2));
            Assert.False(TextHelper.IsWhitespaceOnlyBetweenPositions(tp, 2, 5));
            Assert.False(TextHelper.IsWhitespaceOnlyBetweenPositions(tp, 5, 10));
            Assert.True(TextHelper.IsWhitespaceOnlyBetweenPositions(tp, tp.Length - 1, tp.Length));
            Assert.True(TextHelper.IsWhitespaceOnlyBetweenPositions(tp, 100, 200));
        }
    }
}
