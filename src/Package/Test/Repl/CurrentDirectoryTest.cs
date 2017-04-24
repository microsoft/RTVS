// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Components.ConnectionManager;
using Microsoft.R.Components.History;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Components.InteractiveWorkflow.Implementation;
using Microsoft.R.Components.PackageManager;
using Microsoft.R.Components.Plots;
using Microsoft.R.Components.Test.Fakes.Trackers;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Session;
using Microsoft.UnitTests.Core.Threading;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.R.Package.Repl.Commands;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Package.Test.Mocks;
using Microsoft.VisualStudio.R.Package.Test.Utility;
using Xunit;

namespace Microsoft.VisualStudio.R.Package.Test.Repl {
    [ExcludeFromCodeCoverage]
    [Collection(CollectionNames.NonParallel)]
    public class CurrentDirectoryTest : IAsyncLifetime {
        private readonly IRInteractiveWorkflowVisual _interactiveWorkflow;
        private readonly IRSessionProvider _sessionProvider;

        public CurrentDirectoryTest() {
            var connectionsProvider = VsAppShell.Current.GetService<IConnectionManagerProvider>();
            var historyProvider = VsAppShell.Current.GetService<IRHistoryProvider>();
            var packagesProvider = VsAppShell.Current.GetService<IRPackageManagerProvider>();
            var plotsProvider = VsAppShell.Current.GetService<IRPlotManagerProvider>();
            var activeTextViewTracker = new ActiveTextViewTrackerMock(string.Empty, string.Empty);
            var debuggerModeTracker = new TestDebuggerModeTracker();
            _interactiveWorkflow = UIThreadHelper.Instance.Invoke(() => new RInteractiveWorkflow(
                connectionsProvider, historyProvider, packagesProvider, plotsProvider, activeTextViewTracker,
                debuggerModeTracker, VsAppShell.Current));

            _sessionProvider = _interactiveWorkflow.RSessions;
        }

        public async Task InitializeAsync() {
            await _interactiveWorkflow.Connections.TryConnectToPreviouslyUsedAsync();
        }

        public async Task DisposeAsync() {
            await _interactiveWorkflow.RSession.StopHostAsync();
            _interactiveWorkflow.Dispose();
            _sessionProvider.Dispose();
        }

        public void Dispose() {
            
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
