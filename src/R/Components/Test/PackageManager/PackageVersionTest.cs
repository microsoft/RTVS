// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentAssertions;
using Microsoft.R.Components.PackageManager.Model;
using Microsoft.UnitTests.Core.XUnit;
using Xunit;

namespace Microsoft.R.Components.Test.PackageManager {
    public class PackageVersionTest {
        [CompositeTest]
        [Category.PackageManager]
        [InlineData("1.0", "1.1", -1)]
        [InlineData("1.1", "1.0", 1)]
        [InlineData("1.1", "1.1", 0)]
        [InlineData("2", "2.0", 0)]
        [InlineData("2", "2.0.0", 0)]
        [InlineData("2", "2.0-0", 0)]
        [InlineData("2.1", "2-1", 0)]
        [InlineData("2.1.0", "2-1", 0)]
        [InlineData("2.1.10", "2-1-1", 1)]
        [InlineData("2.1", "2-1.1", -1)]
        [InlineData("2.2", "2.10", -1)]
        [InlineData(null, null, 0)]
        [InlineData(null, "2.10", -1)]
        [InlineData("2.10", null, 1)]
        [InlineData("", "", 0)]
        [InlineData("foo", "bar", 0)]
        public void CompareVersion(string a, string b, int expected) {
            new RPackageVersion(a).CompareTo(new RPackageVersion(b)).Should().Be(expected);
        }
    }
}
