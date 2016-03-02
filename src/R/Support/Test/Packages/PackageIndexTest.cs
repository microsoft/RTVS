// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using FluentAssertions;
using Microsoft.Common.Core;
using Microsoft.R.Support.Help.Definitions;
using Microsoft.R.Support.Help.Packages;
using Microsoft.R.Support.Settings;
using Microsoft.R.Support.Test.Utility;
using Microsoft.UnitTests.Core.XUnit;
using Xunit;

namespace Microsoft.R.Support.Test.Packages {
    [ExcludeFromCodeCoverage]
    public class PackageIndexTest {
        [Test]
        [Category.R.Completion]
        public void BuildPackageIndexTest() {
            IEnumerable<IPackageInfo> basePackages = PackageIndex.BasePackages.AsList();
            string[] packageNames = {
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

            foreach (var name in packageNames) {
                IPackageInfo info = basePackages.FirstOrDefault(x => x.Name == name);
                info.Should().NotBeNull();

                IPackageInfo pi1 = PackageIndex.GetPackageByName(info.Name);
                pi1.Should().NotBeNull();

                pi1.Name.Should().Be(info.Name);
            }
        }

        [Test]
        [Category.R.Completion]
        public void PackageDescriptionTest() {
            RToolsSettings.Current = new TestRToolsSettings();

            IEnumerable<IPackageInfo> basePackages = PackageIndex.BasePackages;

            IPackageInfo pi = PackageIndex.GetPackageByName("base");
            pi.Description.Should().Be("Base R functions.");
        }

        [Test]
        [Category.R.Completion]
        public void UserPackagesIndex_Test01() {
            RToolsSettings.Current = new TestRToolsSettings();

            // make it broken and check that index doesn't throw
            UserPackagesCollection.RLibraryPath = "NonExistentFolder";
            string installPath = UserPackagesCollection.GetInstallPath();
            string userDocumentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            installPath.Should().Be(Path.Combine(userDocumentsPath, UserPackagesCollection.RLibraryPath));

            var collection = new UserPackagesCollection();
            collection.Packages.Should().NotBeNull();

            IEnumerator en = collection.Packages.GetEnumerator();
            en.Should().NotBeNull();
            en.MoveNext().Should().BeFalse();
            en.Current.Should().BeNull();
        }
    }
}
