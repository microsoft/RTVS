// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.Languages.Editor.Text;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.Editor.Mocks;
using Microsoft.VisualStudio.Text;
using NSubstitute;

namespace Microsoft.Languages.Editor.Test.Helpers {
    [ExcludeFromCodeCoverage]
    [Category.Languages.Core]
    public class HelpersTest {
        [Test]
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
    }
}