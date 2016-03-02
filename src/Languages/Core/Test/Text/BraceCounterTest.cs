// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.Languages.Core.Text;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.Languages.Core.Test.Text {
    [ExcludeFromCodeCoverage]
    public class BraceCounterTest {
        [Test]
        [Category.Languages.Core]
        public void BraceCounterTest_SingleBraces() {
            BraceCounter<char> braceCounter = new BraceCounter<char>(new[] { '{', '}'});
            string testString = " {{ { } } } ";
            int[] expectedCount = {0, 1, 2, 2, 3, 3, 2, 2, 1, 1, 0, 0};
            for (var i = 0; i < testString.Length; i++) {
                braceCounter.CountBrace(testString[i]);
                braceCounter.Count.Should().Be(expectedCount[i]);
            }
        }

        [Test]
        [Category.Languages.Core]
        public void BraceCounterTest_MultipleBraces() {
            BraceCounter<char> braceCounter = new BraceCounter<char>(new[] { '{', '}', '[', ']' });
            string testString = " {[ { ] } } ";
            int[] expectedCount = { 0, 1, 2, 2, 3, 3, 2, 2, 1, 1, 0, 0 };
            for (var i = 0; i < testString.Length; i++) {
                braceCounter.CountBrace(testString[i]);
                braceCounter.Count.Should().Be(expectedCount[i]);
            }
        }
    }
}
