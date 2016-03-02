// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.Languages.Editor.EditorHelpers;
using Microsoft.R.Components.Extensions;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.Editor.Mocks;
using Microsoft.VisualStudio.Text;
using NSubstitute;

namespace Microsoft.Languages.Editor.Test.Helpers {
    [ExcludeFromCodeCoverage]
    public class HelpersTest {
        [Test]
        [Category.Languages.Core]
        public void TextChangeExtentTest() {
            var actual = new TextChangeExtent(1, 2, 3);
            actual.Start.Should().Be(1);
            actual.OldEnd.Should().Be(2);
            actual.NewEnd.Should().Be(3);
        }

        [Test]
        [Category.Languages.Core]
        public void GetTextDocumentTest() {
            var tb = new TextBufferMock(string.Empty, "R");

            var s = Substitute.For<ITextDocument>();
            tb.Properties.AddProperty(typeof(ITextDocument), s);

            var td = tb.GetTextDocument();
            td.Should().NotBeNull();
            td.Should().Be(s);
            td.Should().BeAssignableTo<ITextDocument>();
        }

        [Test]
        [Category.Languages.Core]
        public void GetLineColumnFromPositionTest() {
            var tb = new TextBufferMock("a\r\nb", "R");

            int line, col;
            tb.GetLineColumnFromPosition(0, out line, out col);
            line.Should().Be(0);
            col.Should().Be(0);

            tb.GetLineColumnFromPosition(1, out line, out col);
            line.Should().Be(0);
            col.Should().Be(1);

            tb.GetLineColumnFromPosition(3, out line, out col);
            line.Should().Be(1);
            col.Should().Be(0);
        }

        [Test]
        [Category.Languages.Core]
        public void GetPositionFromLineColumnTest() {
            var tb = new TextBufferMock("a\r\nb", "R");

            int? position;
            position = tb.GetPositionFromLineColumn(0, 0);
            position.Should().Be(0);

            position = tb.GetPositionFromLineColumn(1, 0);
            position.Should().Be(3);

            position = tb.GetPositionFromLineColumn(100, 100);
            position.Should().NotHaveValue();
        }

        [Test]
        [Category.Languages.Core]
        public void IsSignatureHelpBufferTest() {
            var tb = new TextBufferMock(string.Empty, "R");
            tb.IsSignatureHelpBuffer().Should().BeFalse();

            tb = new TextBufferMock(string.Empty, "R Signature Help");
            tb.IsSignatureHelpBuffer().Should().BeTrue();
        }

        [Test]
        [Category.Languages.Core]
        public void IsContentEqualsOrdinalTest() {
            var tb = new TextBufferMock("abc abc", "R");

            tb.IsContentEqualsOrdinal(
                new TrackingSpanMock(tb, new Span(0, 3), SpanTrackingMode.EdgePositive, TrackingFidelityMode.Forward),
                new TrackingSpanMock(tb, new Span(4, 3), SpanTrackingMode.EdgePositive, TrackingFidelityMode.Forward)
                ).Should().BeTrue();

            tb.IsContentEqualsOrdinal(
                new TrackingSpanMock(tb, new Span(0, 4), SpanTrackingMode.EdgePositive, TrackingFidelityMode.Forward),
                new TrackingSpanMock(tb, new Span(4, 3), SpanTrackingMode.EdgePositive, TrackingFidelityMode.Forward)
                ).Should().BeFalse();

            tb.IsContentEqualsOrdinal(
                new TrackingSpanMock(tb, new Span(0, 4), SpanTrackingMode.EdgePositive, TrackingFidelityMode.Forward),
                new TrackingSpanMock(tb, new Span(1, 4), SpanTrackingMode.EdgePositive, TrackingFidelityMode.Forward)
                ).Should().BeFalse();
        }
    }
}