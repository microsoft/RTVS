// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentAssertions;
using Microsoft.R.LanguageServer.Text;
using Microsoft.UnitTests.Core.XUnit;
using Xunit;

namespace Microsoft.R.LanguageServer.Test.Text {
    [Category.VsCode.Editor]
    public class EditorSnapshotTest {
        [CompositeTest]
        [InlineData("", new [] {0}, new [] {0}, new [] {0} )]
        [InlineData("1", new[] { 0 }, new[] { 1 }, new[] { 0 })]
        [InlineData("1\n", new[] { 0, 2 }, new[] { 1, 0 }, new[] { 1, 0 })]
        [InlineData("1\r\n", new[] { 0, 3 }, new[] { 1, 0 }, new[] { 2, 0 })]
        [InlineData(" 1\r\n\t2\r\n", new[] { 0, 4, 8 }, new[] { 2, 2, 0 }, new[] { 2, 2, 0 })]
        public void Construction(string content, int[] expectedStarts, int[] expectedLengths, int[] expectedLineBreakLengths) {
            var b = new EditorBuffer(content, "R");
            var s = new EditorBufferSnapshot(b, content, 2);

            s.Version.Should().Be(2);
            s.GetText().Should().Be(content);
            s.LineCount.Should().Be(expectedStarts.Length);

            for(var i = 0; i < s.LineCount; i++) {
                var lineFromNumber = s.GetLineFromLineNumber(i);
                lineFromNumber.LineNumber.Should().Be(i);

                var lineStart = expectedStarts[i];
                var lineEnd = expectedStarts[i] + expectedLengths[i] + expectedLineBreakLengths[i];

                lineFromNumber.Start.Should().Be(lineStart);
                lineFromNumber.End.Should().Be(lineEnd);
                lineFromNumber.Length.Should().Be(lineEnd - lineStart);
                lineFromNumber.LineBreak.Length.Should().Be(expectedLineBreakLengths[i]);

                for (var j = lineStart; j < lineEnd; j++) {
                    var lineFromPosition = s.GetLineFromPosition(j);
                    lineFromPosition.Should().Be(lineFromNumber);
                }
            }
        }
    }
}
