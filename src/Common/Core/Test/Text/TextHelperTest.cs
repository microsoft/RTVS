using System;
using Microsoft.Languages.Core.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Languages.Core.Test.Text
{
    [TestClass]
    public class TextHelperTest
    {
        [TestMethod]
        public void TextHelperTest_IsNewLineBeforePositionTest()
        {
            ITextProvider tp = new TextStream("01\n34\r678\r\nBC");

            Assert.IsFalse(tp.IsNewLineBeforePosition(0));
            Assert.IsFalse(tp.IsNewLineBeforePosition(1));
            Assert.IsFalse(tp.IsNewLineBeforePosition(2));
            Assert.IsTrue(tp.IsNewLineBeforePosition(3));
            Assert.IsFalse(tp.IsNewLineBeforePosition(4));
            Assert.IsFalse(tp.IsNewLineBeforePosition(5));
            Assert.IsTrue(tp.IsNewLineBeforePosition(6));
            Assert.IsFalse(tp.IsNewLineBeforePosition(7));
            Assert.IsFalse(tp.IsNewLineBeforePosition(8));
            Assert.IsFalse(tp.IsNewLineBeforePosition(9));
            Assert.IsTrue(tp.IsNewLineBeforePosition(10));
            Assert.IsTrue(tp.IsNewLineBeforePosition(11));
            Assert.IsFalse(tp.IsNewLineBeforePosition(12));
        }

        [TestMethod]
        public void TextHelperTest_IsNewLineAfterPositionTest()
        {
            ITextProvider tp = new TextStream("0 \n3 \r 7 \r\n  ");

            Assert.IsFalse(TextHelper.IsNewLineAfterPosition(tp, 0));
            Assert.IsTrue(TextHelper.IsNewLineAfterPosition(tp, 1));
            Assert.IsTrue(TextHelper.IsNewLineAfterPosition(tp, 2));
            Assert.IsFalse(TextHelper.IsNewLineAfterPosition(tp, 3));
            Assert.IsTrue(TextHelper.IsNewLineAfterPosition(tp, 4));
            Assert.IsTrue(TextHelper.IsNewLineAfterPosition(tp, 5));
            Assert.IsFalse(TextHelper.IsNewLineAfterPosition(tp, 6));
            Assert.IsFalse(TextHelper.IsNewLineAfterPosition(tp, 7));
            Assert.IsTrue(TextHelper.IsNewLineAfterPosition(tp, 8));
            Assert.IsTrue(TextHelper.IsNewLineAfterPosition(tp, 9));
            Assert.IsTrue(TextHelper.IsNewLineAfterPosition(tp, 10));
            Assert.IsFalse(TextHelper.IsNewLineAfterPosition(tp, 11));
            Assert.IsFalse(TextHelper.IsNewLineAfterPosition(tp, 12));
        }

        [TestMethod]
        public void TextHelperTest_IsWhitespaceOnlyBetweenPositionsTest()
        {
            ITextProvider tp = new TextStream("0 \n3 \r 7 \r\n    AB ");

            Assert.IsFalse(TextHelper.IsWhitespaceOnlyBetweenPositions(tp, 0, 1));
            Assert.IsTrue(TextHelper.IsWhitespaceOnlyBetweenPositions(tp, 1, 2));
            Assert.IsFalse(TextHelper.IsWhitespaceOnlyBetweenPositions(tp, 2, 5));
            Assert.IsFalse(TextHelper.IsWhitespaceOnlyBetweenPositions(tp, 5, 10));
            Assert.IsTrue(TextHelper.IsWhitespaceOnlyBetweenPositions(tp, tp.Length-1, tp.Length));
            Assert.IsTrue(TextHelper.IsWhitespaceOnlyBetweenPositions(tp, 100, 200));
        }
    }
}
