// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.UnitTests.Core.XUnit;
using Xunit;

namespace Microsoft.R.Editor.Test {
    [ExcludeFromCodeCoverage]
    [Category.R.Editor]
    public sealed class StringExtensionsTest {
        [CompositeTest]
        [InlineData(null, null)]
        [InlineData("", "")]
        [InlineData(" ", " ")]
        [InlineData("foo", "foo")]
        [InlineData("a b c", "`a b c`")]
        [InlineData("`", "`")]
        [InlineData("{}", "`{}`")]
        public void BacktickName(string name, string expected) 
            => name.BacktickName().Should().Be(expected);

        [CompositeTest]
        [InlineData("", 0, 0)]
        [InlineData(" { }", 1, 3)]
        [InlineData("  {{} ", 2, 4)]
        [InlineData("{{()}}", 0, 6)]
        [InlineData("{x{a({b})c}x}", 0, 13)]
        [InlineData("(x{a({b})c}x)", 2, 9)]
        public void GetScopeBlockRange(string content, int start, int length) {
            var range = content.GetScopeBlockRange();
            range.Start.Should().Be(start);
            range.Length.Should().Be(length);
        }
    }
}