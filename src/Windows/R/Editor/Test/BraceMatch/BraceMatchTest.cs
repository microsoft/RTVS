// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.Common.Core.Services;
using Microsoft.R.Editor.BraceMatch;
using Microsoft.R.Editor.Test.Utility;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.R.Editor.Test.BraceMatch {
    [ExcludeFromCodeCoverage]
    [Category.R.BraceMatch]
    public class RBraceMatchTest {
        private readonly IServiceContainer _services;

        public RBraceMatchTest(IServiceContainer services) {
            _services = services;
        }

        [Test]
        public void RBraceMatch_CurlyBraces01() {
            var tv = TextViewTest.MakeTextViewRealTextBuffer("a{\"{ }\"}b", _services).As<ITextView>();
            var bm = new RBraceMatcher(tv, tv.TextBuffer);

            var result = bm.GetBracesFromPosition(tv.TextBuffer.CurrentSnapshot, 0, false, out int startPosition, out int endPosition);
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
            var tv = TextViewTest.MakeTextViewRealTextBuffer("a(\"( )\")b", _services).As<ITextView>();
            var bm = new RBraceMatcher(tv, tv.TextBuffer);

            var result = bm.GetBracesFromPosition(tv.TextBuffer.CurrentSnapshot, 0, false, out int startPosition, out int endPosition);
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
            var tv = TextViewTest.MakeTextViewRealTextBuffer("{{\"{ }\"}}", _services).As<ITextView>();
            var bm = new RBraceMatcher(tv, tv.TextBuffer);

            var result = bm.GetBracesFromPosition(tv.TextBuffer.CurrentSnapshot, 0, false, out int startPosition, out int endPosition);
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
            var tv = TextViewTest.MakeTextViewRealTextBuffer("{a[[b()]]}", _services).As<ITextView>();
            var bm = new RBraceMatcher(tv, tv.TextBuffer);

            var result = bm.GetBracesFromPosition(tv.TextBuffer.CurrentSnapshot, 0, false, out int startPosition, out int endPosition);
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
