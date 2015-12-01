using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.R.Actions.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.R.Actions.Test.Installation {
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class RInstallationTest {
        [TestMethod]
        public void RInstallation_Test01() {
            RInstallData data = RInstallation.GetInstallationData(null, 0, 0, 0, 0);
            Assert.AreEqual(RInstallStatus.UnsupportedVersion, data.Status);
            Assert.IsTrue(data.Path.StartsWith(@"C:\Program Files\R", StringComparison.OrdinalIgnoreCase));
        }

        [TestMethod]
        public void RInstallation_Test02() {
            RInstallData data = RInstallation.GetInstallationData(null, 3, 2, 3, 2);
            Assert.AreEqual(RInstallStatus.OK, data.Status);
            Assert.IsTrue(data.Version.Major >= 3);
            Assert.IsTrue(data.Version.Minor >= 2);
            Assert.IsTrue(data.Path.StartsWith(@"C:\Program Files\R", StringComparison.OrdinalIgnoreCase));
        }
    }
}
