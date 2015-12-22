using System;
using Microsoft.R.Actions.Utility;
using Xunit;

namespace Microsoft.R.Actions.Test.Installation {
    public class RInstallationTest {
        [Fact]
        [Trait("R.Install", "")]
        public void RInstallation_Test01() {
            RInstallData data = RInstallation.GetInstallationData(null, 0, 0, 0, 0);
            Assert.Equal(RInstallStatus.UnsupportedVersion, data.Status);
            Assert.True(data.Path.StartsWith(@"C:\Program Files\R", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        [Trait("R.Install", "")]
        public void RInstallation_Test02() {
            RInstallData data = RInstallation.GetInstallationData(null, 3, 2, 3, 2);
            Assert.Equal(RInstallStatus.OK, data.Status);
            Assert.True(data.Version.Major >= 3);
            Assert.True(data.Version.Minor >= 2);
            Assert.True(data.Path.StartsWith(@"C:\Program Files\R", StringComparison.OrdinalIgnoreCase));
        }
    }
}
