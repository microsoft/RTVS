// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Linq;
using FluentAssertions;
using Microsoft.Common.Core;
using Microsoft.UnitTests.Core.Linux;
using Microsoft.UnitTests.Core.XUnit;
using Xunit;

namespace Microsoft.R.Interpreters.Linux.Test {
    [Category.Linux]
    public class RInstallationTest {
        [Fact]
        public void RInstallationBasicTest() {
            var fs = new TestFileSystem();
            var rInstallation = new RInstallation(fs);
            var installs = rInstallation.GetCompatibleEngines();
            installs.Count().Should().Be(2);

            // test MRO
            installs.Should().ContainSingle(i => i.Name.StartsWithOrdinal("Microsoft")).Which.Version.Should().Be(new Version(3, 3, 3));

            // test CRAN R
            installs.Should().ContainSingle(i => i.Name.StartsWithOrdinal("CRAN R")).Which.Version.Should().Be(new Version(3, 2, 3, 4));
        }

        [Fact]
        public void CreateInterpreterInfoTest() {
            // This test is valid only on Linux
            var fs = new TestFileSystem(false);
            var rInstallation = new RInstallation(fs);
            var mro = rInstallation.CreateInfo("MRO", "/usr/lib64/microsoft-r/3.3/lib64/R");
            mro.Version.Should().Be(new Version(3, 3, 3));
        }
    }
}
