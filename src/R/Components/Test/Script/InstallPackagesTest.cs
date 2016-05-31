// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.Common.Core.Install;
using Microsoft.R.Components.Script;
using Microsoft.UnitTests.Core.XUnit;
using Xunit;

namespace Microsoft.R.Components.Test.Script {
    [ExcludeFromCodeCoverage]
    [Collection(CollectionNames.NonParallel)]
    public class InstallPackagesTest
    {
        [Test]
        [Category.R.Package]
        public void InstallPackages_BaseTest()
        {
            RInstallData data = RInstallation.GetInstallationData(string.Empty,
                SupportedRVersionList.MinMajorVersion, SupportedRVersionList.MinMinorVersion,
                SupportedRVersionList.MaxMajorVersion, SupportedRVersionList.MaxMinorVersion);

            data.Status.Should().Be(RInstallStatus.OK);
            bool result = InstallPackages.IsInstalled("base", Int32.MaxValue, data.Path);
            result.Should().BeTrue();
        }
    }
}
