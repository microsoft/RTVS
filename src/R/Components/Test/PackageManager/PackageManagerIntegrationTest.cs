// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Components.Test.Fakes.InteractiveWindow;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Test.Script;
using Microsoft.R.Support.Settings;
using Microsoft.UnitTests.Core.XUnit;
using Xunit;

namespace Microsoft.R.Components.Test.PackageManager {
    public class PackageManagerIntegrationTest : IDisposable, IAsyncLifetime {
        private readonly ExportProvider _exportProvider;
        private readonly TestRInteractiveWorkflowProvider _workflowProvider;
        private readonly MethodInfo _testMethod;
        private readonly TestFilesFixture _testFiles;
        private readonly string _repo1Path;
        private readonly string _libPath;
        private IRInteractiveWorkflow _workflow;

        public PackageManagerIntegrationTest(RComponentsMefCatalogFixture catalog, TestMethodFixture testMethod, TestFilesFixture testFiles) {
            _exportProvider = catalog.CreateExportProvider();
            _workflowProvider = _exportProvider.GetExportedValue<TestRInteractiveWorkflowProvider>();
            _testMethod = testMethod.MethodInfo;
            _testFiles = testFiles;
            _workflowProvider.HostClientApp = new RHostClientTestApp();
            _repo1Path = _testFiles.GetDestinationPath(Path.Combine("Repos", TestRepositories.Repo1));
            _libPath = Path.Combine(_testFiles.GetDestinationPath("library"), _testMethod.Name);
            Directory.CreateDirectory(_libPath);
        }

        [Test]
        [Category.PackageManager]
        public async Task AvailablePackagesCranRepo() {
            var result = await _workflow.Packages.GetAvailablePackagesAsync();

            result.Should().NotBeEmpty();

            var abcPkg = result.SingleOrDefault(pkg => pkg.Package == "abc");
            abcPkg.Should().NotBeNull();
            abcPkg.Version.Length.Should().BeGreaterOrEqualTo(0);
            abcPkg.Depends.IndexOf("abc.data").Should().BeGreaterOrEqualTo(0);
            abcPkg.License.IndexOf("GPL").Should().BeGreaterOrEqualTo(0);
            abcPkg.NeedsCompilation.Should().Be("no");
        }

        [Test]
        [Category.PackageManager]
        public async Task AvailablePackagesLocalRepo() {
            using (var eval = await _workflow.RSession.BeginEvaluationAsync()) {
                await SetLocalRepoAsync(eval, _repo1Path);
            }

            var result = await _workflow.Packages.GetAvailablePackagesAsync();
            result.Should().HaveCount(3);

            var rtvslib1Pkg = result.SingleOrDefault(pkg => pkg.Package == TestPackages.RtvsLib1.Package);
            rtvslib1Pkg.Should().NotBeNull();
            rtvslib1Pkg.Version.Should().Be(TestPackages.RtvsLib1.Version);
            rtvslib1Pkg.Depends.Should().Be(TestPackages.RtvsLib1.Depends);
            rtvslib1Pkg.License.Should().Be(TestPackages.RtvsLib1.License);
            rtvslib1Pkg.NeedsCompilation.Should().Be("no");
            rtvslib1Pkg.Enhances.Should().BeNull();

            // These fields are not currently provided by the server's PACKAGES
            rtvslib1Pkg.Author.Should().BeNull();
            rtvslib1Pkg.Title.Should().BeNull();
            rtvslib1Pkg.Built.Should().BeNull();
        }

        [Test]
        [Category.PackageManager]
        public async Task InstalledPackagesDefault() {
            // Get the installed packages from the default locations
            var result = await _workflow.Packages.GetInstalledPackagesAsync();

            // Validate some of the base packages
            result.Should().NotBeEmpty();

            var graphics = result.SingleOrDefault(pkg => pkg.Package == "graphics");
            graphics.Should().NotBeNull();
            graphics.LibPath.Should().NotBeNullOrWhiteSpace();
            graphics.Priority.Should().Be("base");
            graphics.Depends.Should().BeNull();
            graphics.Imports.Should().Be("grDevices");
            graphics.LinkingTo.Should().BeNull();
            graphics.Suggests.Should().BeNull();
            graphics.Title.Should().Be("The R Graphics Package");
            graphics.Author.Should().Be("R Core Team and contributors worldwide");
        }

        [Test]
        [Category.PackageManager]
        public async Task InstalledPackagesCustomLibPathNoPackages() {
            // Setup library path to point to the test folder, don't install anything in it
            using (var eval = await _workflow.RSession.BeginEvaluationAsync()) {
                await SetLocalLibAsync(eval, _libPath);
            }

            var result = await _workflow.Packages.GetInstalledPackagesAsync();

            // Since we can't remove Program Files folder from library paths,
            // we'll get some results, but nothing from the test folder.
            result.Should().NotBeEmpty();
            result.Where(pkg => pkg.LibPath == _libPath.ToRPath()).Should().BeEmpty();
        }

        [Test]
        [Category.PackageManager]
        public async Task InstalledPackagesCustomLibPathOnePackage() {
            // Setup library path to point to the test folder, install a package into it
            using (var eval = await _workflow.RSession.BeginEvaluationAsync()) {
                await SetLocalRepoAsync(eval, _repo1Path);
                await SetLocalLibAsync(eval, _libPath);
                await InstallPackageAsync(eval, TestPackages.RtvsLib1.Package, _libPath);
            }

            var result = await _workflow.Packages.GetInstalledPackagesAsync();

            result.Should().NotBeEmpty();

            var rtvslib1Pkg = result.SingleOrDefault(pkg => pkg.Package == TestPackages.RtvsLib1.Package && pkg.LibPath == _libPath.ToRPath());
            rtvslib1Pkg.Should().NotBeNull();
            rtvslib1Pkg.Version.Should().Be(TestPackages.RtvsLib1.Version);
            rtvslib1Pkg.Depends.Should().Be(TestPackages.RtvsLib1.Depends);
            rtvslib1Pkg.License.Should().Be(TestPackages.RtvsLib1.License);
            rtvslib1Pkg.Built.Should().Be(TestPackages.RtvsLib1.Built);
            rtvslib1Pkg.NeedsCompilation.Should().Be(null);
            rtvslib1Pkg.Author.Should().Be(TestPackages.RtvsLib1.Author);
            rtvslib1Pkg.Enhances.Should().Be(null);
            rtvslib1Pkg.Title.Should().Be(TestPackages.RtvsLib1.Title);
        }

        private async Task SetLocalRepoAsync(IRSessionEvaluation eval, string localRepoPath) {
            var code = string.Format("options(repos=list(LOCAL=\"file:///{0}\"))", localRepoPath.ToRPath());
            var evalResult = await eval.EvaluateAsync(code);
        }

        private async Task SetLocalLibAsync(IRSessionEvaluation eval, string libPath) {
            var code = string.Format(".libPaths(\"{0}\")", libPath.ToRPath());
            var evalResult = await eval.EvaluateAsync(code);
        }

        private async Task InstallPackageAsync(IRSessionEvaluation eval, string packageName, string libPath) {
            var code = string.Format("install.packages(\"{0}\", verbose=FALSE, quiet=TRUE)", packageName);
            var evalResult = await eval.EvaluateAsync(code);
            WaitForPackageInstalled(libPath, packageName);
        }

        private async Task<IRInteractiveWorkflow> CreateWorkflowAsync() {
            var workflow = _workflowProvider.GetOrCreate();
            await workflow.RSession.StartHostAsync(new RHostStartupInfo {
                Name = _testMethod.Name,
                RBasePath = RToolsSettings.Current.RBasePath,
                RHostCommandLineArguments = RToolsSettings.Current.RCommandLineArguments,
                CranMirrorName = RToolsSettings.Current.CranMirror,
            }, 50000);
            return workflow;
        }

        private static void WaitForPackageInstalled(string libPath, string packageName) {
            WaitForAllFilesExist(new string[] {
                Path.Combine(libPath, packageName, "DESCRIPTION"),
                Path.Combine(libPath, packageName, "NAMESPACE"),
            });
        }

        private static void WaitForAllFilesExist(string[] filePaths, int timeoutMs = 5000) {
            var startTime = DateTime.Now;
            var timeout = TimeSpan.FromMilliseconds(timeoutMs);
            while (!AllFilesExist(filePaths) && (DateTime.Now - startTime) < timeout) {
                Thread.Sleep(100);
            }

            AllFilesExist(filePaths).Should().BeTrue();
        }

        private static bool AllFilesExist(string[] filePaths) {
            foreach (var filePath in filePaths) {
                if (!File.Exists(filePath)) {
                    return false;
                }
            }
            return true;
        }

        public void Dispose() {
            (_exportProvider as IDisposable)?.Dispose();
        }

        public async Task InitializeAsync() {
            _workflow = await CreateWorkflowAsync();
        }

        public Task DisposeAsync() {
            return Task.CompletedTask;
        }
    }
}
