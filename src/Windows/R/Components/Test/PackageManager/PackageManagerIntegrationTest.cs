// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Common.Core.Services;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Components.PackageManager.Model;
using Microsoft.R.Components.Settings;
using Microsoft.R.Components.Test.Fakes.InteractiveWindow;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Test.Script;
using Microsoft.UnitTests.Core.Threading;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.UnitTests.Core.XUnit.MethodFixtures;
using Xunit;

namespace Microsoft.R.Components.Test.PackageManager {
    [ExcludeFromCodeCoverage]
    [Category.PackageManager]
    public class PackageManagerIntegrationTest : IAsyncLifetime {
        private readonly IServiceContainer _services;
        private readonly IRInteractiveWorkflowVisualProvider _workflowProvider;
        private readonly string _repoPath;
        private readonly string _libPath;
        private readonly string _lib2Path;
        private IRInteractiveWorkflow _workflow;

        public PackageManagerIntegrationTest(IServiceContainer services, TestMethodFixture testMethod, TestFilesFixture testFiles) {
            _services = services;
            _workflowProvider = _services.GetService<TestRInteractiveWorkflowProvider>();
            _repoPath = TestRepositories.GetRepoPath(testFiles);
            _libPath = Path.Combine(testFiles.LibraryDestinationPath, testMethod.MethodInfo.Name);
            _lib2Path = Path.Combine(testFiles.Library2DestinationPath, testMethod.MethodInfo.Name);
            Directory.CreateDirectory(_libPath);
            Directory.CreateDirectory(_lib2Path);
        }

        [Test]
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
        public async Task AvailablePackagesLocalRepo() {
            await TestRepositories.SetLocalRepoAsync(_workflow.RSession, _repoPath);

            var result = await _workflow.Packages.GetAvailablePackagesAsync();
            result.Should().HaveCount(3);

            var rtvslib1Expected =  TestPackages.CreateRtvsLib1();
            rtvslib1Expected.Title = null;
            rtvslib1Expected.Built = null;
            rtvslib1Expected.Author = null;
            rtvslib1Expected.Description = null;
            rtvslib1Expected.Repository = $"file:///{_repoPath.ToRPath()}/src/contrib";

            var rtvslib1Actual = result.SingleOrDefault(pkg => pkg.Package == TestPackages.RtvsLib1Description.Package);
            rtvslib1Actual.ShouldBeEquivalentTo(rtvslib1Expected);
        }

        [Test]
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
        public async Task InstalledPackagesCustomLibPathNoPackages() {
            // Setup library path to point to the test folder, don't install anything in it
            await TestLibraries.SetLocalLibsAsync(_workflow.RSession, _libPath);

            var result = await _workflow.Packages.GetInstalledPackagesAsync();

            // Since we can't remove Program Files folder from library paths,
            // we'll get some results, but nothing from the test folder.
            result.Should().NotBeEmpty().And.NotContain(pkg => pkg.LibPath == _libPath.ToRPath());
        }

        [Test]
        public async Task InstalledPackagesCustomLibPathOnePackage() {
            // Setup library path to point to the test folder, install a package into it
            await TestRepositories.SetLocalRepoAsync(_workflow.RSession, _repoPath);
            await TestLibraries.SetLocalLibsAsync(_workflow.RSession, _libPath);
            await InstallPackageAsync(_workflow.RSession, TestPackages.RtvsLib1Description.Package, _libPath);

            var result = await _workflow.Packages.GetInstalledPackagesAsync();
            ValidateRtvslib1Installed(result, _libPath);
        }

        [Test]
        public async Task InstalledPackagesMultiLibsSamePackage() {
            // Install the same package in 2 different libraries
            await TestRepositories.SetLocalRepoAsync(_workflow.RSession, _repoPath);
            await TestLibraries.SetLocalLibsAsync(_workflow.RSession, _libPath);
            await InstallPackageAsync(_workflow.RSession, TestPackages.RtvsLib1Description.Package, _libPath);
            await TestLibraries.SetLocalLibsAsync(_workflow.RSession, _lib2Path);
            await InstallPackageAsync(_workflow.RSession, TestPackages.RtvsLib1Description.Package, _lib2Path);
            await TestLibraries.SetLocalLibsAsync(_workflow.RSession, _libPath, _lib2Path);

            var result = await _workflow.Packages.GetInstalledPackagesAsync();
            ValidateRtvslib1Installed(result, _libPath);

            await TestLibraries.SetLocalLibsAsync(_workflow.RSession, _lib2Path, _libPath);

            result = await _workflow.Packages.GetInstalledPackagesAsync();
            ValidateRtvslib1Installed(result, _lib2Path);
        }

        private void ValidateRtvslib1Installed(IReadOnlyList<RPackage> pkgs, string libPath) {
            var rtvslib1Expected = TestPackages.CreateRtvsLib1();
            rtvslib1Expected.LibPath = libPath.ToRPath();
            rtvslib1Expected.NeedsCompilation = null;

            var rtvslib1Actual = pkgs.Should().ContainSingle(pkg => pkg.Package == TestPackages.RtvsLib1Description.Package && pkg.LibPath == libPath.ToRPath()).Which;
            rtvslib1Actual.ShouldBeEquivalentTo(rtvslib1Expected);
        }

        [Test]
        public async Task InstallAndUninstallPackageSpecifiedLib() {
            await TestRepositories.SetLocalRepoAsync(_workflow.RSession, _repoPath);

            await _workflow.Packages.InstallPackageAsync(TestPackages.RtvsLib1Description.Package, _libPath);

            await TestLibraries.SetLocalLibsAsync(_workflow.RSession, _libPath);

            var installed = await _workflow.Packages.GetInstalledPackagesAsync();
            ValidateRtvslib1Installed(installed, _libPath);

            await _workflow.Packages.UninstallPackageAsync(TestPackages.RtvsLib1Description.Package, _libPath);

            installed = await _workflow.Packages.GetInstalledPackagesAsync();
            installed.Should().NotContain(pkg => pkg.Package == TestPackages.RtvsLib1Description.Package && pkg.LibPath == _libPath.ToRPath());
        }

        [Test]
        public async Task InstallPackageDefaultLib() {
            await TestRepositories.SetLocalRepoAsync(_workflow.RSession, _repoPath);
            await TestLibraries.SetLocalLibsAsync(_workflow.RSession, _libPath);

            await _workflow.Packages.InstallPackageAsync(TestPackages.RtvsLib1Description.Package, null);

            var installed = await _workflow.Packages.GetInstalledPackagesAsync();
            ValidateRtvslib1Installed(installed, _libPath);
        }

        [Test]
        public async Task LoadAndUnloadPackage() {
            await TestRepositories.SetLocalRepoAsync(_workflow.RSession, _repoPath);
            await TestLibraries.SetLocalLibsAsync(_workflow.RSession, _libPath);
            await InstallPackageAsync(_workflow.RSession, TestPackages.RtvsLib1Description.Package, _libPath);

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
        public async Task GetLoadedPackages() {
            await TestRepositories.SetLocalRepoAsync(_workflow.RSession, _repoPath);
            await TestLibraries.SetLocalLibsAsync(_workflow.RSession, _libPath);
            await InstallPackageAsync(_workflow.RSession, TestPackages.RtvsLib1Description.Package, _libPath);

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
        public async Task LibraryPaths() {
            await TestRepositories.SetLocalRepoAsync(_workflow.RSession, _repoPath);
            await TestLibraries.SetLocalLibsAsync(_workflow.RSession, _libPath);

            var result = await _workflow.Packages.GetLibraryPathAsync();
            result.Should().Be(_libPath.ToRPath());
        }

        [Test]
        public async Task GetPackageLockStateLockByRSession() {
            await TestLibraries.SetLocalLibsAsync(_workflow.RSession, _libPath);
            await InstallPackageAsync(_workflow.RSession, "abn", _libPath);

            await _workflow.Packages.LoadPackageAsync("abn", null);

            var pkgs = await _workflow.Packages.GetInstalledPackagesAsync();
            var abn = pkgs.Should().ContainSingle(pkg => pkg.Package == "abn").Which;
            var cairo = pkgs.Should().ContainSingle(pkg => pkg.Package == "Cairo").Which;

            var abnState = await _workflow.Packages.GetPackageLockStateAsync(abn.Package, abn.LibPath);
            abnState.Should().Be(PackageLockState.LockedByRSession);
            var cairoState = await _workflow.Packages.GetPackageLockStateAsync(cairo.Package, cairo.LibPath);
            cairoState.Should().Be(PackageLockState.LockedByRSession);
        }

        [Test]
        public async Task GetPackageLockStateUnlocked() {
            await TestLibraries.SetLocalLibsAsync(_workflow.RSession, _libPath);
            await InstallPackageAsync(_workflow.RSession, "abn", _libPath);

            var pkgs = await _workflow.Packages.GetInstalledPackagesAsync();
            var abn = pkgs.Should().ContainSingle(pkg => pkg.Package == "abn").Which;

            var abnState = await _workflow.Packages.GetPackageLockStateAsync(abn.Package, abn.LibPath);
            abnState.Should().Be(PackageLockState.Unlocked);
        }

        private async Task EvaluateCode(string code, string expectedResult = null, string expectedError = null) {
            var evalResult = await _workflow.RSession.EvaluateAsync(code, REvaluationKind.Normal);
            if (expectedResult != null) {
                evalResult.Result.ToObject<string>().Trim().Should().Be(expectedResult.Trim());
            }
            if (expectedError != null) {
                evalResult.Error.Trim().Should().Be(expectedError.Trim());
            }
        }

        private async Task InstallPackageAsync(IRExpressionEvaluator eval, string packageName, string libPath) {
            var code = $"install.packages(\"{packageName}\", verbose=FALSE, quiet=TRUE)";
            var evalResult = await eval.EvaluateAsync(code, REvaluationKind.Normal);
            WaitForPackageInstalled(libPath, packageName);
        }

        private async Task<IRInteractiveWorkflow> CreateWorkflowAsync() {
            var workflow = UIThreadHelper.Instance.Invoke(() => _workflowProvider.GetOrCreate());
            var settings = _services.GetService<IRSettings>();
            await workflow.RSessions.TrySwitchBrokerAsync(nameof(PackageManagerIntegrationTest));
            await workflow.RSession.EnsureHostStartedAsync(new RHostStartupInfo (settings.CranMirror, codePage: settings.RCodePage), new RHostClientTestApp(), 50000);
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

        public async Task InitializeAsync() {
            _workflow = await CreateWorkflowAsync();
        }

        public Task DisposeAsync() {
            return Task.CompletedTask;
        }
    }
}
