using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.R.Actions.Utility;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.R.Actions.Test.Installation {
    [ExcludeFromCodeCoverage]
    public class RInstallationTest {
        [Test]
        [Category.R.Install]
        public void RInstallation_Test01() {
            RInstallData data = RInstallation.GetInstallationData(null, 0, 0, 0, 0);
            data.Status.Should().Be(RInstallStatus.UnsupportedVersion);
            data.Path.Should().StartWithEquivalent(@"C:\Program Files\R");
        }

        [Test]
        [Category.R.Install]
        public void RInstallation_Test02() {
            RInstallData data = RInstallation.GetInstallationData(null, 3, 2, 3, 2);
            data.Status.Should().Be(RInstallStatus.OK);
            data.Version.Major.Should().BeGreaterOrEqualTo(3);
            data.Version.Minor.Should().BeGreaterOrEqualTo(2);
            data.Path.Should().StartWithEquivalent(@"C:\Program Files\R");
        }
    }
}
