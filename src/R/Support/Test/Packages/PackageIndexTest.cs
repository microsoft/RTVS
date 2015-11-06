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
            RToolsSettings.Current = new TestRToolsSettings();
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
                "grDevices",
                "grid",
                "KernSmooth",
                "lattice",
                "MASS",
                "Matrix",
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

            string installPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                @"R\R-3.2.2\library");

            int i = 0;
            foreach (string name in packageNames)
            {
                IPackageInfo info = basePackages.FirstOrDefault((x) => x.Name == name);
                Assert.IsNotNull(info);

                IPackageInfo pi1 = PackageIndex.GetPackageByName(info.Name);
                Assert.IsNotNull(pi1);

                Assert.AreEqual(info.Name, pi1.Name);

                i++;
            }
        }

        [TestMethod]
        public void PackageDescriptionTest()
        {
            RToolsSettings.Current = new TestRToolsSettings();
            EditorShell.SetShell(TestEditorShell.Create(RSupportTestCompositionCatalog.Current));

            IEnumerable<IPackageInfo> basePackages = PackageIndex.BasePackages;

            IPackageInfo pi = PackageIndex.GetPackageByName("base");
            Assert.AreEqual("Base R functions.", pi.Description);
        }

        [TestMethod]
        public void UserPackagesIndex_Test01()
        {
            RToolsSettings.Current = new TestRToolsSettings();

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
