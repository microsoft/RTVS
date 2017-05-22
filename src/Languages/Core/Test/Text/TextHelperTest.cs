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

            tp.IsNewLineAfterPosition(0).Should().BeFalse();
            tp.IsNewLineAfterPosition(1).Should().BeTrue();
            tp.IsNewLineAfterPosition(2).Should().BeTrue();
            tp.IsNewLineAfterPosition(3).Should().BeFalse();
            tp.IsNewLineAfterPosition(4).Should().BeTrue();
            tp.IsNewLineAfterPosition(5).Should().BeTrue();
            tp.IsNewLineAfterPosition(6).Should().BeFalse();
            tp.IsNewLineAfterPosition(7).Should().BeFalse();
            tp.IsNewLineAfterPosition(8).Should().BeTrue();
            tp.IsNewLineAfterPosition(9).Should().BeTrue();
            tp.IsNewLineAfterPosition(10).Should().BeTrue();
            tp.IsNewLineAfterPosition(11).Should().BeFalse();
            tp.IsNewLineAfterPosition(12).Should().BeFalse();
        }

        [Test]
        [Category.Languages.Core]
        public void TextHelperTest_IsWhitespaceOnlyBetweenPositionsTest() {
            ITextProvider tp = new TextStream("0 \n3 \r 7 \r\n    AB ");

            tp.IsWhitespaceOnlyBetweenPositions(0, 1).Should().BeFalse();
            tp.IsWhitespaceOnlyBetweenPositions(1, 2).Should().BeTrue();
            tp.IsWhitespaceOnlyBetweenPositions(2, 5).Should().BeFalse();
            tp.IsWhitespaceOnlyBetweenPositions(5, 10).Should().BeFalse();
            tp.IsWhitespaceOnlyBetweenPositions(tp.Length - 1, tp.Length).Should().BeTrue();
            tp.IsWhitespaceOnlyBetweenPositions(100, 200).Should().BeTrue();
        }
    }
}
