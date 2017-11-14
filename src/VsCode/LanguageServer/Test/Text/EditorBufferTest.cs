// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Linq;
using FluentAssertions;
using Microsoft.Common.Core;
using Microsoft.Languages.Core.Text;
using Microsoft.R.LanguageServer.Text;
using Microsoft.UnitTests.Core.XUnit;
using Xunit;

namespace Microsoft.R.LanguageServer.Test.Text {
    [Category.VsCode.Editor]
    public class EditorBufferTest {
        [CompositeTest]
        [InlineData("", 0, "abc")]
        [InlineData("ac", 1, "b")]
        [InlineData("ab", 2, "c")]
        [InlineData("ab", 2, "\nc\n")]
        public void Insert(string content, int position, string insert) {
            var b = new EditorBuffer(content, "R");
            b.Insert(position, insert);
            var s = b.CurrentSnapshot;
            s.Version.Should().Be(1);

            var expected = content.Substring(0, position) + insert + content.Substring(position);
            s.GetText().Should().Be(expected);
        }

        [CompositeTest]
        [InlineData("", 0, "\n")]
        [InlineData("ac", 1, "\nb")]
        [InlineData("ab", 2, "c\n")]
        [InlineData("ab", 2, "\nc\n")]
        public void InsertLines(string content, int position, string insert) {
            var b = new EditorBuffer(content, "R");
            b.Insert(position, insert);
            var s = b.CurrentSnapshot;
            s.Version.Should().Be(1);

            var expected = content.Substring(0, position) + insert + content.Substring(position);
            var count = expected.Count(ch => ch.IsLineBreak()) + 1;
            s.LineCount.Should().Be(count);
        }

        [CompositeTest]
        [InlineData("", 0, 0)]
        [InlineData("a\nc", 1, 1)]
        [InlineData("ab\r\n", 2, 2)]
        [InlineData("\nab", 0, 1)]
        public void DeleteLines(string content, int position, int delete) {
            var b = new EditorBuffer(content, "R");
            b.Delete(new TextRange(position, delete));
            var s = b.CurrentSnapshot;
            s.Version.Should().Be(1);

            var expected = content.Substring(0, position) + content.Substring(position + delete);
            var count = expected.Count(ch => ch.IsLineBreak()) + 1;
            s.LineCount.Should().Be(count);
        }
    }
}
