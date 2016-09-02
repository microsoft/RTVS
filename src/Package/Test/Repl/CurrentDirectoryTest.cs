// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.R.Components.ConnectionManager;
using Microsoft.R.Components.History;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Components.InteractiveWorkflow.Implementation;
using Microsoft.R.Components.PackageManager;
using Microsoft.R.Components.Plots;
using Microsoft.R.Components.Test.Fakes.Trackers;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Session;
using Microsoft.R.Host.Client.Test.Fixtures;
using Microsoft.R.Support.Settings;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.R.Package.Repl.Commands;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Package.Test.Mocks;
using Microsoft.VisualStudio.R.Package.Test.Utility;
using Xunit;

namespace Microsoft.VisualStudio.R.Package.Test.Repl {
    [ExcludeFromCodeCoverage]
    [Collection(CollectionNames.NonParallel)]
    public class CurrentDirectoryTest : IDisposable {
        private readonly IRInteractiveWorkflow _interactiveWorkflow;
        private readonly IRSessionProvider _sessionProvider;

        public CurrentDirectoryTest() {
            _sessionProvider = new RSessionProvider();

            var connectionsProvider = VsAppShell.Current.ExportProvider.GetExportedValue<IConnectionManagerProvider>();
            var historyProvider = VsAppShell.Current.ExportProvider.GetExportedValue<IRHistoryProvider>();
            var packagesProvider = VsAppShell.Current.ExportProvider.GetExportedValue<IRPackageManagerProvider>();
            var plotsProvider = VsAppShell.Current.ExportProvider.GetExportedValue<IRPlotManagerProvider>();
            var activeTextViewTracker = new ActiveTextViewTrackerMock(string.Empty, string.Empty);
            var debuggerModeTracker = new TestDebuggerModeTracker();
            _interactiveWorkflow = new RInteractiveWorkflow(
                _sessionProvider, connectionsProvider, historyProvider, packagesProvider, plotsProvider, activeTextViewTracker,
                debuggerModeTracker, VsAppShell.Current, RToolsSettings.Current, null, () => { });
        }

        public void Dispose() {
            _interactiveWorkflow.Dispose();
            _sessionProvider.Dispose();
        }

        [Test]
        [Category.Repl]
        public void DefaultDirectoryTest() {
            string myDocs = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            string actual;
            using (var script = new VsRHostScript(_sessionProvider)) {
                var cmd = new WorkingDirectoryCommand(_interactiveWorkflow);
                cmd.InitializationTask.Wait();
                cmd.UserDirectory.Should().BeEquivalentTo(myDocs);
                actual = script.Session.GetRWorkingDirectoryAsync().Result;
            };

            actual.Should().Be(myDocs);
        }

        [Test]
        [Category.Repl]
        public void SetDirectoryTest() {
            string dir = "c:\\";
            string actual;
            using (new VsRHostScript(_sessionProvider)) {
                var cmd = new WorkingDirectoryCommand(_interactiveWorkflow);
                cmd.InitializationTask.Wait();
                cmd.SetDirectory(dir).Wait();
                actual = _interactiveWorkflow.RSession.GetRWorkingDirectoryAsync().Result;
            }

            actual.Should().Be(dir);
        }

        [Test]
        [Category.Repl]
        public void GetFriendlyNameTest01() {
            string actual;
            using (new VsRHostScript(_sessionProvider)) {
                var cmd = new WorkingDirectoryCommand(_interactiveWorkflow);
                cmd.InitializationTask.Wait();
                actual = cmd.GetFriendlyDirectoryName(Environment.GetFolderPath(Environment.SpecialFolder.Personal));
            };

            actual.Should().Be("~");
        }

        [Test]
        [Category.Repl]
        public void GetFriendlyNameTest02() {
            string actual;
            using (new VsRHostScript(_sessionProvider)) {
                var cmd = new WorkingDirectoryCommand(_interactiveWorkflow);
                cmd.InitializationTask.Wait();
                actual = cmd.GetFriendlyDirectoryName("c:\\");
            };

            actual.Should().Be("c:/");
        }

        [Test]
        [Category.Repl]
        public void GetFullPathNameTest() {
            string dir;
            using (new VsRHostScript(_sessionProvider)) {
                var cmd = new WorkingDirectoryCommand(_interactiveWorkflow);
                cmd.InitializationTask.Wait();
                dir = cmd.GetFullPathName("~");
            }

            string actual = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            actual.Should().Be(dir);
        }
    }
}
