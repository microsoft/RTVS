using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.R.Actions.Script;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.R.Actions.Test.Script
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class InstallPackagesTest
    {
        [TestMethod]
        public void InstallPackages_BaseTest()
        {
            bool result = InstallPackages.IsInstalled("base", Int32.MaxValue);
            Assert.IsTrue(result);
        }
    }
}
