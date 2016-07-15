// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Host.Client;
using Microsoft.R.Support.Help;
using Microsoft.R.Support.Help.Packages;
using Microsoft.R.Support.Settings;
using Microsoft.R.Support.Test.Utility;
using Microsoft.UnitTests.Core.Mef;
using Microsoft.UnitTests.Core.XUnit;
using Xunit;

namespace Microsoft.R.Support.Test.Packages {
    [ExcludeFromCodeCoverage]
    [Collection(CollectionNames.NonParallel)]
    public class PackageIndexTest {
        private readonly IExportProvider _exportProvider;
        private readonly ICoreShell _shell;
        private readonly IFunctionIndex _functionIndex;
        private readonly IRSessionProvider _sessionProvider;

        public PackageIndexTest(RSupportMefCatalogFixture catalogFixture) {
            _exportProvider = catalogFixture.CreateExportProvider();
            _shell = _exportProvider.GetExportedValue<ICoreShell>();
            _functionIndex = _exportProvider.GetExportedValue<IFunctionIndex>();
            _sessionProvider = _exportProvider.GetExportedValue<IRSessionProvider>();
        }

        [Test]
        [Category.R.Completion]
        public async Task BuildPackageIndexTest() {
            var packageIndex = new PackageIndex(_shell);
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

            using (var host = new IntelliSenseRHost(_shell, _sessionProvider)) {
                await host.CreateSessionAsync();
                await packageIndex.BuildIndexAsync(_functionIndex, host.Session);
            }

            foreach (var name in packageNames) {
                IPackageInfo pi = await packageIndex.GetPackageByNameAsync(name);
                pi.Should().NotBeNull();
                pi.Name.Should().Be(name);
            }
        }

        [Test]
        [Category.R.Completion]
        public async Task PackageDescriptionTest() {
            RToolsSettings.Current = new TestRToolsSettings();
            var packageIndex = new PackageIndex(_shell);
            using (var host = new IntelliSenseRHost(_shell, _sessionProvider)) {
                await host.CreateSessionAsync();
                await packageIndex.BuildIndexAsync(_functionIndex, host.Session);
            }
            IPackageInfo pi = await packageIndex.GetPackageByNameAsync("base");
            pi.Description.Should().Be("Base R functions.");
        }
    }
}
