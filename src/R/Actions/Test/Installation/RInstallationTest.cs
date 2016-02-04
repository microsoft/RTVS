using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.R.Actions.Utility;
using Microsoft.UnitTests.Core.XUnit;
using Xunit;

namespace Microsoft.R.Actions.Test.Installation {
    [ExcludeFromCodeCoverage]
    public class RInstallationTest {
        [Test]
        [Category.R.Install]
        public void RInstallation_Test01() {
            RInstallData data = RInstallation.GetInstallationData(null, 0, 0, 0, 0);
            Assert.True(data.Status == RInstallStatus.PathNotSpecified || data.Status == RInstallStatus.UnsupportedVersion);
        }

        [Test]
        [Category.R.Install]
        public void RInstallation_Test02() {
            RInstallData data = RInstallation.GetInstallationData(null, 3, 2, 3, 2, useRegistry: true);
            data.Status.Should().Be(RInstallStatus.OK);
            data.Version.Major.Should().BeGreaterOrEqualTo(3);
            data.Version.Minor.Should().BeGreaterOrEqualTo(2);
            data.Path.Should().StartWithEquivalent(@"C:\Program Files");
            data.Path.Should().Contain("R-");
        }
    }
}
