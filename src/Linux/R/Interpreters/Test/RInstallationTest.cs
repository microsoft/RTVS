// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Linq;
using FluentAssertions;
using Microsoft.Common.Core;
using Microsoft.UnitTests.Core.Linux;
using Xunit;

namespace Microsoft.R.Interpreters.Linux.Test {
    public class RInstallationTest {
        [Fact]
        public void RInstallationBasicTest() {
            var fs = new TestFileSystem();
            var rInstallation = new RInstallation(fs);
            var installs = rInstallation.GetCompatibleEngines();
            installs.Count().Should().Be(2);

            // test MRO
            var mro = installs.Where(i => i.Name.StartsWithOrdinal("Microsoft"));
            mro.Count().Should().Be(1);
            var mroInterpreter = mro.First();
            mroInterpreter.Version.Should().Be(new Version(3, 3, 3));

            // test CRAN R
            var cranR = installs.Where(i => i.Name.StartsWithOrdinal("R "));
            cranR.Count().Should().Be(1);
            var cranRInterpreter = cranR.First();
            cranRInterpreter.Version.Should().Be(new Version(3, 2, 3, 4));
        }

        [Fact]
        public void CreateInterpreterInfoTest() {
            var fs = new TestFileSystem(false);
            var rInstallation = new RInstallation(fs);
            var mro = rInstallation.CreateInfo("MRO", "/usr/lib64/microsoft-r/3.3/lib64/R");
            mro.Version.Should().Be(new Version(3, 3, 3));
        }
    }
}
