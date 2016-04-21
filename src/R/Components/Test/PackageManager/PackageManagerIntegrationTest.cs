// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Common.Core;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Components.PackageManager.Model;
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
        private readonly string _repo1Path;
        private readonly string _libPath;
        private readonly string _lib2Path;
        private IRInteractiveWorkflow _workflow;

        public PackageManagerIntegrationTest(RComponentsMefCatalogFixture catalog, TestMethodFixture testMethod, TestFilesFixture testFiles) {
            _exportProvider = catalog.CreateExportProvider();
            _workflowProvider = _exportProvider.GetExportedValue<TestRInteractiveWorkflowProvider>();
            _testMethod = testMethod.MethodInfo;
            _repo1Path = testFiles.GetDestinationPath(Path.Combine("Repos", TestRepositories.Repo1));
            _libPath = Path.Combine(testFiles.GetDestinationPath("library"), _testMethod.Name);
            _lib2Path = Path.Combine(testFiles.GetDestinationPath("library2"), _testMethod.Name);
            Directory.CreateDirectory(_libPath);
            Directory.CreateDirectory(_lib2Path);
        }

        [Test]
        [Category.PackageManager]
        public async Task AvailablePackagesCranRepo() {
            var result = await _workflow.Packages.GetAvailablePackagesAsync();

            // Since this is coming from an internet repo where we don't control the data,
            // we only test a few of the values that are less likely to change over time.
            var abcPkg = result.Should().ContainSingle(pkg => pkg.Package == "abc").Which;
            abcPkg.Version.Length.Should().BeGreaterOrEqualTo(0);
            abcPkg.Depends.Should().Contain("abc.data");
            abcPkg.License.Should().Contain("GPL");
            abcPkg.Title.Should().Be("Tools for Approximate Bayesian Computation (ABC)");
            abcPkg.Description.Should().StartWith("Implements several ABC algorithms");
            abcPkg.Author.Should().Contain("Csillery Katalin");
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

            var rtvslib1Expected = TestPackages.RtvsLib1.Clone();
            rtvslib1Expected.Title = null;
            rtvslib1Expected.Built = null;
            rtvslib1Expected.Author = null;
            rtvslib1Expected.Description = null;
            rtvslib1Expected.Repository = $"file:///{_repo1Path.ToRPath()}/src/contrib";

            var rtvslib1Actual = result.SingleOrDefault(pkg => pkg.Package == TestPackages.RtvsLib1Description.Package);
            rtvslib1Actual.ShouldBeEquivalentTo(rtvslib1Expected);
        }

        [Test]
        [Category.PackageManager]
        public async Task InstalledPackagesDefault() {
            // Get the installed packages from the default locations
            var result = await _workflow.Packages.GetInstalledPackagesAsync();

            // Validate some of the base packages
            result.Should().NotBeEmpty();

            // Since we don't control this package, only spot check a few fields unlikely to change
            var graphics = result.Should().ContainSingle(pkg => pkg.Package == "graphics").Which;
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
                await SetLocalLibsAsync(eval, _libPath);
            }

            var result = await _workflow.Packages.GetInstalledPackagesAsync();

            // Since we can't remove Program Files folder from library paths,
            // we'll get some results, but nothing from the test folder.
            result.Should().NotBeEmpty().And.NotContain(pkg => pkg.LibPath == _libPath.ToRPath());
        }

        [Test]
        [Category.PackageManager]
        public async Task InstalledPackagesCustomLibPathOnePackage() {
            // Setup library path to point to the test folder, install a package into it
            using (var eval = await _workflow.RSession.BeginEvaluationAsync()) {
                await SetLocalRepoAsync(eval, _repo1Path);
                await SetLocalLibsAsync(eval, _libPath);
                await InstallPackageAsync(eval, TestPackages.RtvsLib1Description.Package, _libPath);
            }

            var result = await _workflow.Packages.GetInstalledPackagesAsync();
            ValidateRtvslib1Installed(result, _libPath);
        }

        [Test]
        [Category.PackageManager]
        public async Task InstalledPackagesMultiLibsSamePackage() {
            // Install the same package in 2 different libraries
            using (var eval = await _workflow.RSession.BeginEvaluationAsync()) {
                await SetLocalRepoAsync(eval, _repo1Path);
                await SetLocalLibsAsync(eval, _libPath);
                await InstallPackageAsync(eval, TestPackages.RtvsLib1Description.Package, _libPath);
                await SetLocalLibsAsync(eval, _lib2Path);
                await InstallPackageAsync(eval, TestPackages.RtvsLib1Description.Package, _lib2Path);
                await SetLocalLibsAsync(eval, _libPath, _lib2Path);
            }

            var result = await _workflow.Packages.GetInstalledPackagesAsync();
            ValidateRtvslib1Installed(result, _libPath);

            using (var eval = await _workflow.RSession.BeginEvaluationAsync()) {
                await SetLocalLibsAsync(eval, _lib2Path, _libPath);
            }

            result = await _workflow.Packages.GetInstalledPackagesAsync();
            ValidateRtvslib1Installed(result, _lib2Path);
        }

        private void ValidateRtvslib1Installed(IReadOnlyList<RPackage> pkgs, string libPath) {
            var rtvslib1Expected = TestPackages.RtvsLib1.Clone();
            rtvslib1Expected.LibPath = libPath.ToRPath();
            rtvslib1Expected.NeedsCompilation = null;

            var rtvslib1Actual = pkgs.Should().ContainSingle(pkg => pkg.Package == TestPackages.RtvsLib1Description.Package && pkg.LibPath == libPath.ToRPath()).Which;
            rtvslib1Actual.ShouldBeEquivalentTo(rtvslib1Expected);
        }

        [Test]
        [Category.PackageManager]
        public async Task InstallAndUninstallPackageSpecifiedLib() {
            using (var eval = await _workflow.RSession.BeginEvaluationAsync()) {
                await SetLocalRepoAsync(eval, _repo1Path);
            }

            await _workflow.Packages.InstallPackageAsync(TestPackages.RtvsLib1Description.Package, _libPath);

            using (var eval = await _workflow.RSession.BeginEvaluationAsync()) {
                await SetLocalLibsAsync(eval, _libPath);
            }

            var installed = await _workflow.Packages.GetInstalledPackagesAsync();
            ValidateRtvslib1Installed(installed, _libPath);

            await _workflow.Packages.UninstallPackageAsync(TestPackages.RtvsLib1Description.Package, _libPath);

            installed = await _workflow.Packages.GetInstalledPackagesAsync();
            installed.Should().NotContain(pkg => pkg.Package == TestPackages.RtvsLib1Description.Package && pkg.LibPath == _libPath.ToRPath());
        }

        [Test]
        [Category.PackageManager]
        public async Task InstallPackageDefaultLib() {
            using (var eval = await _workflow.RSession.BeginEvaluationAsync()) {
                await SetLocalRepoAsync(eval, _repo1Path);
                await SetLocalLibsAsync(eval, _libPath);
            }

            await _workflow.Packages.InstallPackageAsync(TestPackages.RtvsLib1Description.Package, null);

            var installed = await _workflow.Packages.GetInstalledPackagesAsync();
            ValidateRtvslib1Installed(installed, _libPath);
        }

        [Test]
        [Category.PackageManager]
        public async Task LoadAndUnloadPackage() {
            using (var eval = await _workflow.RSession.BeginEvaluationAsync()) {
                await SetLocalRepoAsync(eval, _repo1Path);
                await SetLocalLibsAsync(eval, _libPath);
                await InstallPackageAsync(eval, TestPackages.RtvsLib1Description.Package, _libPath);
            }

            await EvaluateCode("func1();", expectedError: "Error: could not find function \"func1\"");

            await _workflow.Packages.LoadPackageAsync(TestPackages.RtvsLib1Description.Package, null);

            await EvaluateCode("func1();", expectedResult: "func1");

            var loaded = await _workflow.Packages.GetLoadedPackagesAsync();
            loaded.Should().Contain("rtvslib1");

            await _workflow.Packages.UnloadPackageAsync(TestPackages.RtvsLib1Description.Package);

            await EvaluateCode("func1();", expectedError: "Error: could not find function \"func1\"");

            loaded = await _workflow.Packages.GetLoadedPackagesAsync();
            loaded.Should().NotContain("rtvslib1");
        }

        [Test]
        [Category.PackageManager]
        public async Task GetLoadedPackages() {
            using (var eval = await _workflow.RSession.BeginEvaluationAsync()) {
                await SetLocalRepoAsync(eval, _repo1Path);
                await SetLocalLibsAsync(eval, _libPath);
                await InstallPackageAsync(eval, TestPackages.RtvsLib1Description.Package, _libPath);
            }

            var results = await _workflow.Packages.GetLoadedPackagesAsync();
            results.Should().NotContain(new[] {
                "rtvslib1", ".GlobalEnv", "Autoloads",
            });
            results.Should().Contain(new[] {
                "stats", "graphics", "grDevices", "grDevices",
                "utils", "datasets", "methods", "base",
            });
        }

        [Test]
        [Category.PackageManager]
        public async Task LibraryPaths() {
            using (var eval = await _workflow.RSession.BeginEvaluationAsync()) {
                await SetLocalRepoAsync(eval, _repo1Path);
                await SetLocalLibsAsync(eval, _libPath);
            }

            var result = await _workflow.Packages.GetLibraryPathsAsync();
            result[0].Should().Be(_libPath.ToRPath());
        }

        [Test]
        [Category.PackageManager]
        public async Task GetPackageLockStateLockByRSession() {
            using (var eval = await _workflow.RSession.BeginEvaluationAsync()) {
                await SetLocalLibsAsync(eval, _libPath);
                await InstallPackageAsync(eval, "abn", _libPath);
            }

            await _workflow.Packages.LoadPackageAsync("abn", null);

            var pkgs = await _workflow.Packages.GetInstalledPackagesAsync();
            var abn = pkgs.Should().ContainSingle(pkg => pkg.Package == "abn").Which;
            var cairo = pkgs.Should().ContainSingle(pkg => pkg.Package == "Cairo").Which;

            _workflow.Packages.GetPackageLockState(abn.Package, abn.LibPath).Should().Be(PackageLockState.LockedByRSession);
            _workflow.Packages.GetPackageLockState(cairo.Package, cairo.LibPath).Should().Be(PackageLockState.LockedByRSession);
        }

        [Test]
        [Category.PackageManager]
        public async Task GetPackageLockStateUnlocked() {
            using (var eval = await _workflow.RSession.BeginEvaluationAsync()) {
                await SetLocalLibsAsync(eval, _libPath);
                await InstallPackageAsync(eval, "abn", _libPath);
            }

            var pkgs = await _workflow.Packages.GetInstalledPackagesAsync();
            var abn = pkgs.Should().ContainSingle(pkg => pkg.Package == "abn").Which;

            _workflow.Packages.GetPackageLockState(abn.Package, abn.LibPath).Should().Be(PackageLockState.Unlocked);
        }

        private async Task EvaluateCode(string code, string expectedResult = null, string expectedError = null) {
            using (var eval = await _workflow.RSession.BeginEvaluationAsync()) {
                var evalResult = await eval.EvaluateAsync(code, REvaluationKind.Normal);
                if (expectedResult != null) {
                    evalResult.StringResult.Trim().Should().Be(expectedResult.Trim());
                }
                if (expectedError != null) {
                    evalResult.Error.Trim().Should().Be(expectedError.Trim());
                }
            }
        }

        private async Task SetLocalRepoAsync(IRSessionEvaluation eval, string localRepoPath) {
            var code = $"options(repos=list(LOCAL=\"file:///{localRepoPath.ToRPath()}\"))";
            var evalResult = await eval.EvaluateAsync(code, REvaluationKind.Mutating);
        }

        private async Task SetLocalLibsAsync(IRSessionEvaluation eval, params string[] libPaths) {
            var paths = string.Join(",", libPaths.Select(p => p.ToRPath().ToRStringLiteral()));
            var code = $".libPaths(c({paths}))";
            var evalResult = await eval.EvaluateAsync(code, REvaluationKind.Normal);
        }

        private async Task InstallPackageAsync(IRSessionEvaluation eval, string packageName, string libPath) {
            var code = $"install.packages(\"{packageName}\", verbose=FALSE, quiet=TRUE)";
            var evalResult = await eval.EvaluateAsync(code, REvaluationKind.Normal);
            WaitForPackageInstalled(libPath, packageName);
        }

        private async Task<IRInteractiveWorkflow> CreateWorkflowAsync() {
            var workflow = _workflowProvider.GetOrCreate();
            await workflow.RSession.StartHostAsync(new RHostStartupInfo {
                Name = _testMethod.Name,
                RBasePath = RToolsSettings.Current.RBasePath,
                RHostCommandLineArguments = RToolsSettings.Current.RCommandLineArguments,
                CranMirrorName = RToolsSettings.Current.CranMirror,
            }, new RHostClientTestApp(), 50000);
            return workflow;
        }
        
        private static void WaitForPackageInstalled(string libPath, string packageName) {
            WaitForAllFilesExist(new[] {
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
            return filePaths.All(File.Exists);
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
