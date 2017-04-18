// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.R.Components.ContentTypes;
using Microsoft.R.Editor.Navigation.Text;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.Editor.Mocks;
using Microsoft.VisualStudio.Text;
using Xunit;

namespace Microsoft.R.Editor.Test.Navigation {
    [ExcludeFromCodeCoverage]
    [Category.R.Navigation]
    public class TextStructureTest {
        [CompositeTest]
        [InlineData("", 0, 0, 0)]
        [InlineData("x", 0, 0, 1)]
        [InlineData("x", 1, 0, 1)]
        [InlineData("ab", 0, 0, 2)]
        [InlineData("ab", 1, 0, 2)]
        [InlineData("a.b", 0, 0, 3)]
        [InlineData("a.b", 1, 0, 3)]
        [InlineData("a.b+c.d", 1, 0, 3)]
        [InlineData("'aa bb'", 2, 1, 2)]
        [InlineData("'aa.bb'", 2, 1, 2)]
        [InlineData("\"aa bb+\"", 5, 4, 2)]
        [InlineData("*`zzz`-", 2, 1, 5)]
        [InlineData("1.5+3.6", 2, 0, 3)]
        [InlineData("'22.11'", 2, 1, 2)]
        [InlineData("('a.b')", 6, 6, 1)]
        [InlineData("('a.b')", 5, 5, 1)]
        [InlineData("('')", 2, 2, 1)]
        [InlineData("'", 0, 0, 1)]
        [InlineData("'", 1, 0, 1)]
        [InlineData("#abc def", 1, 1, 3)]
        [InlineData("# abc.def", 2, 2, 3)]
        [InlineData("#abc", 0, 0, 1)]
        public void GetWordSpan(string content, int position, int start, int length) {
            var tb = new TextBufferMock(content, RContentTypeDefinition.ContentType);
            Span? span = RTextStructure.GetWordSpan(tb.CurrentSnapshot, position);
            if (span.HasValue) {
                span.Value.Start.Should().Be(start);
                span.Value.Length.Should().Be(length);
            } else {
                length.Should().Be(0);
                start.Should().Be(0);
            }
        }
    }
}
