using System;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.R.Actions.Script;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.R.Actions.Test.Script
{
    [ExcludeFromCodeCoverage]
    public class InstallPackagesTest
    {
        [Test]
        [Category.R.Package]
        public void InstallPackages_BaseTest()
        {
            bool result = InstallPackages.IsInstalled("base", Int32.MaxValue, null);
            result.Should().BeTrue();
        }
    }
}
