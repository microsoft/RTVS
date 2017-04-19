// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.Languages.Core.Text;
using Microsoft.R.Editor.RData.BraceMatch;
using Microsoft.R.Editor.RData.ContentTypes;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.Editor.Mocks.Helpers;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.R.Editor.RData.Test.BraceMatch {
    [ExcludeFromCodeCoverage]
    public class RBraceMatchTest {
        [Test]
        [Category.Rd.BraceMatch]
        public void RdBraceMatch_MixedBraces() {
            ITextBuffer textBuffer;
            RdBraceMatcher bm = CreateBraceMatcher("\\latex[0]{foo} \\item{}{}", out textBuffer);

            int startPosition, endPosition;
            bool result;

            result = bm.GetBracesFromPosition(textBuffer.CurrentSnapshot, 0, false, out startPosition, out endPosition);
            result.Should().BeFalse();

            result = bm.GetBracesFromPosition(textBuffer.CurrentSnapshot, 6, false, out startPosition, out endPosition);
            result.Should().BeTrue();
            startPosition.Should().Be(6);
            endPosition.Should().Be(8);

            result = bm.GetBracesFromPosition(textBuffer.CurrentSnapshot, 7, false, out startPosition, out endPosition);
            result.Should().BeTrue();
            startPosition.Should().Be(6);
            endPosition.Should().Be(8);

            result = bm.GetBracesFromPosition(textBuffer.CurrentSnapshot, 9, false, out startPosition, out endPosition);
            result.Should().BeTrue();
            startPosition.Should().Be(9);
            endPosition.Should().Be(13);

            result = bm.GetBracesFromPosition(textBuffer.CurrentSnapshot, 13, false, out startPosition, out endPosition);
            result.Should().BeTrue();
            startPosition.Should().Be(9);
            endPosition.Should().Be(13);

            result = bm.GetBracesFromPosition(textBuffer.CurrentSnapshot, 14, false, out startPosition, out endPosition);
            result.Should().BeTrue();
            startPosition.Should().Be(9);
            endPosition.Should().Be(13);
        }

        [Test]
        [Category.Rd.BraceMatch]
        public void RdBraceMatch_SquareBrackets() {
            ITextBuffer textBuffer;
            RdBraceMatcher bm = CreateBraceMatcher("\\a[[b]]", out textBuffer);

            int startPosition, endPosition;
            bool result;

            result = bm.GetBracesFromPosition(textBuffer.CurrentSnapshot, 1, false, out startPosition, out endPosition);
            result.Should().BeFalse();

            result = bm.GetBracesFromPosition(textBuffer.CurrentSnapshot, 2, false, out startPosition, out endPosition);
            result.Should().BeTrue();
            startPosition.Should().Be(2);
            endPosition.Should().Be(6);

            result = bm.GetBracesFromPosition(textBuffer.CurrentSnapshot, 3, false, out startPosition, out endPosition);
            result.Should().BeTrue();
            startPosition.Should().Be(3);
            endPosition.Should().Be(5);

            result = bm.GetBracesFromPosition(textBuffer.CurrentSnapshot, 5, false, out startPosition, out endPosition);
            result.Should().BeTrue();
            startPosition.Should().Be(3);
            endPosition.Should().Be(5);

            result = bm.GetBracesFromPosition(textBuffer.CurrentSnapshot, 6, false, out startPosition, out endPosition);
            result.Should().BeTrue();
            startPosition.Should().Be(2);
            endPosition.Should().Be(6);
        }

        private RdBraceMatcher CreateBraceMatcher(string content, out ITextBuffer textBuffer) {
            ITextView tv = TextViewTestHelper.MakeTextView(content, RdContentTypeDefinition.ContentType, TextRange.EmptyRange);
            textBuffer = tv.TextBuffer;
            return new RdBraceMatcher(tv, tv.TextBuffer);
        }
    }
}
