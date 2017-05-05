// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor.Text;
using Microsoft.R.Components.ContentTypes;
using Microsoft.R.Editor.Tree;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.Editor.Mocks;
using Xunit;
using TextChange = Microsoft.R.Editor.Tree.TextChange;

namespace Microsoft.R.Editor.Test.Tree {
    [ExcludeFromCodeCoverage]
    [Category.R.EditorTree]
    public class TextChangesTest {
        [CompositeTest]
        [InlineData(0, 0, 0, 0, 0, 0, 0, 0, 0)]
        // Changed range here is bigger than necessary but
        // in reality it will be limited by the tet buffer length.
        [InlineData(0, 1, 0, 0, 0, 1, 0, 1, 2)] 
        [InlineData(0, 0, 1, 0, 1, 0, 0, 0, 1)]
        [InlineData(5, 10, 15, 0, 1, 0, 0, 10, 15)]
        [InlineData(0, 1, 0, 5, 10, 15, 0, 11, 16)]
        [InlineData(4, 6, 5, 5, 10, 15, 4, 11, 16)]
        public void TextChangeCombine(
            int prevStart, int prevOldEnd, int prevNewEnd,
            int nextStart, int nextOldEnd, int nextNewEnd,
            int expectedStart, int expectedOldEnd, int expectedNewEnd) {

            var tc1 = new TextChange {
                Start = prevStart,
                OldEnd = prevOldEnd,
                NewEnd = prevNewEnd
            };

            var tc2 = new TextChange {
                Start = nextStart,
                OldEnd = nextOldEnd,
                NewEnd = nextNewEnd
            };

            tc1.OldLength.Should().Be(prevOldEnd - prevStart);
            tc1.NewLength.Should().Be(prevNewEnd - prevStart);
            tc1.OldRange.Start.Should().Be(prevStart);
            tc1.OldRange.End.Should().Be(prevOldEnd);
            tc1.NewRange.Start.Should().Be(prevStart);
            tc1.NewRange.End.Should().Be(prevNewEnd);

            tc1.Combine(tc2);
            tc1.Start.Should().Be(expectedStart);
            tc1.OldEnd.Should().Be(expectedOldEnd);
            tc1.NewEnd.Should().Be(expectedNewEnd);
        }
    }
}
