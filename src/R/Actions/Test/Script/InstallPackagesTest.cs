using System;
using Microsoft.R.Actions.Script;
using Xunit;

namespace Microsoft.R.Actions.Test.Script {
    public class InstallPackagesTest {
        [Fact]
        [Trait("R.Packages", "")]
        public void InstallPackages_BaseTest() {
            bool result = InstallPackages.IsInstalled("base", Int32.MaxValue, null);
            Assert.True(result);
        }
    }
}
