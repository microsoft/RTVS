// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.Languages.Core.Text;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.Languages.Core.Test.Text {
    [ExcludeFromCodeCoverage]
    public class TextHelperTest {
        [Test]
        [Category.Languages.Core]
        public void TextHelperTest_IsNewLineBeforePositionTest() {
            ITextProvider tp = new TextStream("01\n34\r678\r\nBC");

            tp.IsNewLineBeforePosition(0).Should().BeFalse();
            tp.IsNewLineBeforePosition(1).Should().BeFalse();
            tp.IsNewLineBeforePosition(2).Should().BeFalse();
            tp.IsNewLineBeforePosition(3).Should().BeTrue();
            tp.IsNewLineBeforePosition(4).Should().BeFalse();
            tp.IsNewLineBeforePosition(5).Should().BeFalse();
            tp.IsNewLineBeforePosition(6).Should().BeTrue();
            tp.IsNewLineBeforePosition(7).Should().BeFalse();
            tp.IsNewLineBeforePosition(8).Should().BeFalse();
            tp.IsNewLineBeforePosition(9).Should().BeFalse();
            tp.IsNewLineBeforePosition(10).Should().BeTrue();
            tp.IsNewLineBeforePosition(11).Should().BeTrue();
            tp.IsNewLineBeforePosition(12).Should().BeFalse();
        }

        [Test]
        [Category.Languages.Core]
        public void TextHelperTest_IsNewLineAfterPositionTest() {
            ITextProvider tp = new TextStream("0 \n3 \r 7 \r\n  ");

            TextHelper.IsNewLineAfterPosition(tp, 0).Should().BeFalse();
            TextHelper.IsNewLineAfterPosition(tp, 1).Should().BeTrue();
            TextHelper.IsNewLineAfterPosition(tp, 2).Should().BeTrue();
            TextHelper.IsNewLineAfterPosition(tp, 3).Should().BeFalse();
            TextHelper.IsNewLineAfterPosition(tp, 4).Should().BeTrue();
            TextHelper.IsNewLineAfterPosition(tp, 5).Should().BeTrue();
            TextHelper.IsNewLineAfterPosition(tp, 6).Should().BeFalse();
            TextHelper.IsNewLineAfterPosition(tp, 7).Should().BeFalse();
            TextHelper.IsNewLineAfterPosition(tp, 8).Should().BeTrue();
            TextHelper.IsNewLineAfterPosition(tp, 9).Should().BeTrue();
            TextHelper.IsNewLineAfterPosition(tp, 10).Should().BeTrue();
            TextHelper.IsNewLineAfterPosition(tp, 11).Should().BeFalse();
            TextHelper.IsNewLineAfterPosition(tp, 12).Should().BeFalse();
        }

        [Test]
        [Category.Languages.Core]
        public void TextHelperTest_IsWhitespaceOnlyBetweenPositionsTest() {
            ITextProvider tp = new TextStream("0 \n3 \r 7 \r\n    AB ");

            TextHelper.IsWhitespaceOnlyBetweenPositions(tp, 0, 1).Should().BeFalse();
            TextHelper.IsWhitespaceOnlyBetweenPositions(tp, 1, 2).Should().BeTrue();
            TextHelper.IsWhitespaceOnlyBetweenPositions(tp, 2, 5).Should().BeFalse();
            TextHelper.IsWhitespaceOnlyBetweenPositions(tp, 5, 10).Should().BeFalse();
            TextHelper.IsWhitespaceOnlyBetweenPositions(tp, tp.Length - 1, tp.Length).Should().BeTrue();
            TextHelper.IsWhitespaceOnlyBetweenPositions(tp, 100, 200).Should().BeTrue();
        }
    }
}
