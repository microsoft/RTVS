// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.R.Editor.BraceMatch;
using Microsoft.R.Editor.Test.Utility;
using Microsoft.UnitTests.Core.Mef;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.R.Editor.Test.BraceMatch {
    [ExcludeFromCodeCoverage]
    [Category.R.BraceMatch]
    public class RBraceMatchTest {
        private readonly IExportProvider _exportProvider;

        public RBraceMatchTest(IExportProvider exportProvider, EditorTestFilesFixture testFiles) {
            _exportProvider = exportProvider;
        }

        [Test]
        public void RBraceMatch_CurlyBraces01() {
            ITextView tv = TextViewTest.MakeTextViewRealTextBuffer("a{\"{ }\"}b", _exportProvider);
            RBraceMatcher bm = new RBraceMatcher(tv, tv.TextBuffer);

            int startPosition, endPosition;
            bool result;

            result = bm.GetBracesFromPosition(tv.TextBuffer.CurrentSnapshot, 0, false, out startPosition, out endPosition);
            result.Should().BeFalse();

            result = bm.GetBracesFromPosition(tv.TextBuffer.CurrentSnapshot, 1, false, out startPosition, out endPosition);

            result.Should().BeTrue();
            startPosition.Should().Be(1);
            endPosition.Should().Be(7);

            result = bm.GetBracesFromPosition(tv.TextBuffer.CurrentSnapshot, 2, false, out startPosition, out endPosition);

            result.Should().BeTrue();
            startPosition.Should().Be(1);
            endPosition.Should().Be(7);

            result = bm.GetBracesFromPosition(tv.TextBuffer.CurrentSnapshot, 3, false, out startPosition, out endPosition);

            result.Should().BeFalse();
        }

        [Test]
        public void RBraceMatch_Braces() {
            ITextView tv = TextViewTest.MakeTextViewRealTextBuffer("a(\"( )\")b", _exportProvider);
            RBraceMatcher bm = new RBraceMatcher(tv, tv.TextBuffer);

            int startPosition, endPosition;
            bool result;

            result = bm.GetBracesFromPosition(tv.TextBuffer.CurrentSnapshot, 0, false, out startPosition, out endPosition);

            result.Should().BeFalse();

            result = bm.GetBracesFromPosition(tv.TextBuffer.CurrentSnapshot, 1, false, out startPosition, out endPosition);

            result.Should().BeTrue();
            startPosition.Should().Be(1);
            endPosition.Should().Be(7);

            result = bm.GetBracesFromPosition(tv.TextBuffer.CurrentSnapshot, 2, false, out startPosition, out endPosition);

            result.Should().BeTrue();
            startPosition.Should().Be(1);
            endPosition.Should().Be(7);

            result = bm.GetBracesFromPosition(tv.TextBuffer.CurrentSnapshot, 3, false, out startPosition, out endPosition);

            result.Should().BeFalse();
        }
        [Test]
        public void RBraceMatch_CurlyBraces02() {
            ITextView tv = TextViewTest.MakeTextViewRealTextBuffer("{{\"{ }\"}}", _exportProvider);
            RBraceMatcher bm = new RBraceMatcher(tv, tv.TextBuffer);

            int startPosition, endPosition;
            bool result;

            result = bm.GetBracesFromPosition(tv.TextBuffer.CurrentSnapshot, 0, false, out startPosition, out endPosition);

            result.Should().BeTrue();
            startPosition.Should().Be(0);
            endPosition.Should().Be(8);

            result = bm.GetBracesFromPosition(tv.TextBuffer.CurrentSnapshot, 1, false, out startPosition, out endPosition);

            result.Should().BeTrue();
            startPosition.Should().Be(1);
            endPosition.Should().Be(7);

            result = bm.GetBracesFromPosition(tv.TextBuffer.CurrentSnapshot, 3, false, out startPosition, out endPosition);

            result.Should().BeFalse();
        }

        [Test]
        public void RBraceMatch_MixedBraces() {
            ITextView tv = TextViewTest.MakeTextViewRealTextBuffer("{a[[b()]]}", _exportProvider);
            RBraceMatcher bm = new RBraceMatcher(tv, tv.TextBuffer);

            int startPosition, endPosition;
            bool result;

            result = bm.GetBracesFromPosition(tv.TextBuffer.CurrentSnapshot, 0, false, out startPosition, out endPosition);
            result.Should().BeTrue();
            startPosition.Should().Be(0);
            endPosition.Should().Be(9);

            result = bm.GetBracesFromPosition(tv.TextBuffer.CurrentSnapshot, 2, false, out startPosition, out endPosition);
            result.Should().BeTrue();
            startPosition.Should().Be(2);
            endPosition.Should().Be(8);

            result = bm.GetBracesFromPosition(tv.TextBuffer.CurrentSnapshot, 3, false, out startPosition, out endPosition);
            result.Should().BeTrue();
            startPosition.Should().Be(3);
            endPosition.Should().Be(7);

            result = bm.GetBracesFromPosition(tv.TextBuffer.CurrentSnapshot, 5, false, out startPosition, out endPosition);
            result.Should().BeTrue();
            startPosition.Should().Be(5);
            endPosition.Should().Be(6);

            result = bm.GetBracesFromPosition(tv.TextBuffer.CurrentSnapshot, 6, false, out startPosition, out endPosition);
            result.Should().BeTrue();
            startPosition.Should().Be(5);
            endPosition.Should().Be(6);

            result = bm.GetBracesFromPosition(tv.TextBuffer.CurrentSnapshot, 7, false, out startPosition, out endPosition);
            result.Should().BeTrue();
            startPosition.Should().Be(3);
            endPosition.Should().Be(7);

            result = bm.GetBracesFromPosition(tv.TextBuffer.CurrentSnapshot, 8, false, out startPosition, out endPosition);
            result.Should().BeTrue();
            startPosition.Should().Be(2);
            endPosition.Should().Be(8);

            result = bm.GetBracesFromPosition(tv.TextBuffer.CurrentSnapshot, 9, false, out startPosition, out endPosition);
            result.Should().BeTrue();
            startPosition.Should().Be(0);
            endPosition.Should().Be(9);
        }

    }
}
