using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Microsoft.Languages.Core.Test.Utility;
using Microsoft.Languages.Editor.Shell;
using Microsoft.Languages.Editor.Tests.Shell;
using Microsoft.R.Support.Help.Definitions;
using Microsoft.R.Support.Help.Packages;
using Microsoft.R.Support.Settings;
using Microsoft.R.Support.Test.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.R.Support.Test.Packages
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class PackageIndexTest : UnitTestBase
    {
        [TestMethod]
        public void BuildPackageIndexTest()
        {
            RToolsSettings.ToolsSettings = new TestRToolsSettings();
            EditorShell.SetShell(TestEditorShell.Create(RSupportTestCompositionCatalog.Current));

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

        [TestMethod]
        public void PackageDescriptionTest()
        {
            RToolsSettings.ToolsSettings = new TestRToolsSettings();
            EditorShell.SetShell(TestEditorShell.Create(RSupportTestCompositionCatalog.Current));

            IEnumerable<IPackageInfo> basePackages = PackageIndex.BasePackages;

            IPackageInfo pi = PackageIndex.GetPackageByName("base");
            Assert.AreEqual("Base R functions.", pi.Description);
        }

        [TestMethod]
        public void UserPackagesIndex_Test01()
        {
            RToolsSettings.ToolsSettings = new TestRToolsSettings();

            // make it broken and check that index doesn't throw
            UserPackagesCollection.RLibraryPath = "NonExistentFolder";
            string installPath = UserPackagesCollection.GetInstallPath();
            string userDocumentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            Assert.AreEqual(Path.Combine(userDocumentsPath, UserPackagesCollection.RLibraryPath), installPath);

            var collection = new UserPackagesCollection();
            Assert.IsNotNull(collection.Packages);

            IEnumerator en = collection.Packages.GetEnumerator();
            Assert.IsNotNull(en);
            Assert.IsFalse(en.MoveNext());
            Assert.IsNull(en.Current);
        }
    }
}
