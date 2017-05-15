// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.Languages.Core.Text;
using Microsoft.R.Editor.Tree;
using Microsoft.UnitTests.Core.XUnit;
using Xunit;

namespace Microsoft.R.Editor.Test.Tree {
    [ExcludeFromCodeCoverage]
    [Category.R.EditorTree]
    public class TreeTextChangeTest {
        [CompositeTest]
        [InlineData(0, 0, 0, 0, 0, 0, 0, 0, 0)]
        // Changed range here is bigger than necessary but
        // in reality it will be limited by the tet buffer length.
        [InlineData(0, 1, 0, 0, 0, 1, 0, 1, 1)]
        [InlineData(0, 0, 1, 0, 1, 0, 0, 1, 1)]
        [InlineData(5, 10, 15, 0, 1, 0, 0, 10, 15)]
        [InlineData(0, 1, 0, 5, 10, 15, 0, 10, 15)]
        [InlineData(4, 6, 5, 5, 10, 15, 4, 10, 15)]
        public void Combine(
            int prevStart, int prevOldEnd, int prevNewEnd,
            int nextStart, int nextOldEnd, int nextNewEnd,
            int expectedStart, int expectedOldEnd, int expectedNewEnd) {

            var oldText = new TextStream(new string('a', 20), 0);
            var newText = new TextStream(new string('b', 20), 1);

            var tc1 = new TreeTextChange(prevStart, prevOldEnd - prevStart, prevNewEnd - prevStart, oldText, oldText);
            var tc2 = new TreeTextChange(nextStart, nextOldEnd - nextStart, nextNewEnd - nextStart, newText, newText);

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
