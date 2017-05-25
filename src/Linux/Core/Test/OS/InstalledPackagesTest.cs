// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Linq;
using FluentAssertions;
using Microsoft.Common.Core.OS;
using Microsoft.UnitTests.Core.Linux;
using Microsoft.UnitTests.Core.XUnit;
using Xunit;

namespace Microsoft.Common.Core.Linux.Test {
    [Category.Linux]
     public class InstalledPackagesTest {
        [Fact]
        public void ReadInstalledPackagesTest() {
            var fs = new TestFileSystem();
            var packages = InstalledPackageInfo.GetPackages(fs);
            packages.Count().Should().Be(2183); // number of installed packages in the status file

            // Test for MRO
            var mro = packages.Where(p => p.PackageName.StartsWithOrdinal("microsoft-r-open-mro"));
            mro.Count().Should().Be(1);
            var mroFiles = mro.First().GetPackageFiles(fs);
            mroFiles.Count().Should().BeGreaterThan(0);
            var mroLib = mroFiles.Where(f => f.EndsWithOrdinal("/libR.so"));
            mroLib.Count().Should().BeGreaterThan(0);

            //Test for CRAN R
            var cranR = packages.Where(p => p.PackageName.EqualsOrdinal("r-base-core"));
            cranR.Count().Should().Be(1);
            var cranRFiles = mro.First().GetPackageFiles(fs);
            cranRFiles.Count().Should().BeGreaterThan(0);
            var cranRLib = mroFiles.Where(f => f.EndsWithOrdinal("/libR.so"));
            cranRLib.Count().Should().BeGreaterThan(0);
        }
    }
}
