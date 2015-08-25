using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Languages.Core.Test.Utility;
using Microsoft.R.Support.Help.Definitions;
using Microsoft.R.Support.Help.Packages;
using Microsoft.R.Support.Settings;
using Microsoft.R.Support.Test.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.R.Support.Test.Packages
{
    [TestClass]
    public class PackageIndexTest : UnitTestBase
    {
        [TestMethod]
        public void BuildPackageIndexTest()
        {
            RToolsSettings.ToolsSettings = new TestRToolsSettings();

            IEnumerable<IPackageInfo> basePackages = PackageIndex.BasePackages;
            string[] packageNames = new string[]
            {
                "base",
                "boot",
                "class",
                "cluster",
                "codetools",
                "compiler",
                "datasets",
                "foreign",
                "graphics",
                "grdevices",
                "grid",
                "kernsmooth",
                "lattice",
                "mass",
                "matrix",
                "methods",
                "mgcv",
                "nlme",
                "nnet",
                "parallel",
                "rpart",
                "spatial",
                "splines",
                "stats",
                "stats4",
                "survival",
                "tcltk",
                "tools",
                "translations",
                "utils",
             };

            Assert.AreEqual(30, basePackages.Count());

            string installPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), 
                @"R\R-3.2.0\library");

            int i = 0;
            foreach (IPackageInfo pi in basePackages)
            {
                Assert.AreEqual(packageNames[i], pi.Name);

                IPackageInfo pi1 = PackageIndex.GetPackageByName(pi.Name);
                Assert.IsNotNull(pi1);

                Assert.AreEqual(pi.Name, pi1.Name);

                i++;
            }
        }
    }
}
