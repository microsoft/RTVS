// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.R.Package.ProjectSystem;
using Microsoft.VisualStudio.R.Package.ProjectSystem.PropertyPages;

namespace Microsoft.VisualStudio.R.Package.Test.ProjectSystem.PropertyPages {
    [ExcludeFromCodeCoverage]
    public class RunPageViewModelTest {
        [Test]
        [Category.Project]
        public async Task SingleConfiguration() {
            // Prepare the model
            var debugConfigProps = new TestProjectProperties(false, "script.R", "-debug");

            // Prepare the view model
            var vm = new RunPageViewModel(new IRProjectProperties[] { debugConfigProps });

            // Initialize the view model from the model
            await vm.Initialize();

            // Verify view model is initialized correctly
            vm.ResetReplOnRun.Should().Be(false);
            vm.StartupFile.Should().Be("script.R");
            vm.CommandLineArgs.Should().Be("-debug");

            bool dirty = false;
            vm.PropertyChanged += (object sender, PropertyChangedEventArgs e) => {
                if (!vm.IgnoreEvents) {
                    dirty = true;
                }
            };

            // Simulate a view model change from UI
            vm.ResetReplOnRun = true;
            dirty.Should().Be(true);

            // Save the view model into the model
            await vm.Save();

            // Verify model has been updated
            debugConfigProps.ShouldBeEquivalentTo(new TestProjectProperties(true, "script.R", "-debug"));
        }

        [Test]
        [Category.Project]
        public async Task MultiConfigurationsChangeConflictingResetReplOnRun() {
            // Prepare the model
            var debugConfigProps = new TestProjectProperties(false, "script.R", "-debug");
            var releaseConfigProps = new TestProjectProperties(true, "script.R", "-release");

            // Prepare the view model
            var vm = new RunPageViewModel(new IRProjectProperties[] { debugConfigProps, releaseConfigProps });

            // Initialize the view model from the model
            await vm.Initialize();

            // Verify view model is initialized correctly
            vm.ResetReplOnRun.Should().Be(PropertyPageViewModel.DifferentBoolOptions);
            vm.StartupFile.Should().Be("script.R");
            vm.CommandLineArgs.Should().Be(PropertyPageViewModel.DifferentStringOptions);

            bool dirty = false;
            vm.PropertyChanged += (object sender, PropertyChangedEventArgs e) => {
                if (!vm.IgnoreEvents) {
                    dirty = true;
                }
            };

            // Simulate a view model change from UI
            vm.ResetReplOnRun = true;
            dirty.Should().Be(true);

            // Save the view model into the model
            await vm.Save();

            // Verify model has been updated
            debugConfigProps.ShouldBeEquivalentTo(new TestProjectProperties(true, "script.R", "-debug"));
            releaseConfigProps.ShouldBeEquivalentTo(new TestProjectProperties(true, "script.R", "-release"));
        }

        [Test]
        [Category.Project]
        public async Task MultiConfigurationsChangeConflictingStartupFile() {
            // Prepare the model
            var debugConfigProps = new TestProjectProperties(false, "debug.R", "-option");
            var releaseConfigProps = new TestProjectProperties(true, "release.R", "-option");

            // Prepare the view model
            var vm = new RunPageViewModel(new IRProjectProperties[] { debugConfigProps, releaseConfigProps });

            // Initialize the view model from the model
            await vm.Initialize();

            // Verify view model is initialized correctly
            vm.ResetReplOnRun.Should().Be(PropertyPageViewModel.DifferentBoolOptions);
            vm.StartupFile.Should().Be(PropertyPageViewModel.DifferentStringOptions);
            vm.CommandLineArgs.Should().Be("-option");

            bool dirty = false;
            vm.PropertyChanged += (object sender, PropertyChangedEventArgs e) => {
                if (!vm.IgnoreEvents) {
                    dirty = true;
                }
            };

            // Simulate a view model change from UI
            vm.StartupFile = "common.R";
            dirty.Should().Be(true);

            // Save the view model into the model
            await vm.Save();

            // Verify model has been updated
            debugConfigProps.ShouldBeEquivalentTo(new TestProjectProperties(false, "common.R", "-option"));
            releaseConfigProps.ShouldBeEquivalentTo(new TestProjectProperties(true, "common.R", "-option"));
        }

        class TestProjectProperties : IRProjectProperties {
            public bool ResetReplOnRun { get; set; }
            public string StartupFile { get; set; }
            public string CommandLineArgs { get; set; }
            public string SettingsFile { get; set; }
            public string RemoteProjectPath { get; set; }
            public string FileFilter { get; set; }
            public bool TransferProjectOnRun { get; set; }
            public IEnumerable<string> RFilePaths { get; set; }
            public string ProjectName { get; set; }

            public TestProjectProperties(bool resetReplOnRun, string startupFile, string commandLineArgs) {
                ResetReplOnRun = resetReplOnRun;
                StartupFile = startupFile;
                CommandLineArgs = commandLineArgs;
            }

            public Task<bool> GetResetReplOnRunAsync() => Task.FromResult(ResetReplOnRun);
            public Task<string> GetStartupFileAsync() => Task.FromResult(StartupFile);
            public Task<string> GetCommandLineArgsAsync() => Task.FromResult(CommandLineArgs);
            public Task<string> GetSettingsFileAsync() => Task.FromResult(SettingsFile);
            public Task<string> GetRemoteProjectPathAsync() => Task.FromResult(RemoteProjectPath);
            public Task<string> GetFileFilterAsync() => Task.FromResult(RemoteProjectPath);
            public Task<bool> GetTransferProjectOnRunAsync() => Task.FromResult(TransferProjectOnRun);
            public IEnumerable<string> GetRFilePaths() => RFilePaths;
            public string GetProjectName() => ProjectName;

            public Task SetResetReplOnRunAsync(bool val) {
                ResetReplOnRun = val;
                return Task.CompletedTask;
            }

            public Task SetStartupFileAsync(string val) {
                StartupFile = val;
                return Task.CompletedTask;
            }
            public Task SetCommandLineArgsAsync(string val) {
                CommandLineArgs = val;
                return Task.CompletedTask;
            }

            public Task SetSettingsFileAsync(string file) {
                SettingsFile = file;
                return Task.CompletedTask;
            }

            public Task SetRemoteProjectPathAsync(string remoteProjectPath) {
                RemoteProjectPath = remoteProjectPath;
                return Task.CompletedTask;
            }

            public Task SetFileFilterAsync(string fileTransferFilter) {
                FileFilter = fileTransferFilter;
                return Task.CompletedTask;
            }

            public Task SetTransferProjectOnRunAsync(bool val) {
                TransferProjectOnRun = val;
                return Task.CompletedTask;
            }
        }
    }
}
