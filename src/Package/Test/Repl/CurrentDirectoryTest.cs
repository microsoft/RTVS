// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.R.Components.History;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Components.Test.Fakes.Trackers;
using Microsoft.R.Host.Client;
using Microsoft.R.Support.Settings;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.R.Package.Repl;
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

        public CurrentDirectoryTest() {
            var sessionProvider = VsAppShell.Current.ExportProvider.GetExportedValue<IRSessionProvider>();
            var historyProvider = VsAppShell.Current.ExportProvider.GetExportedValue<IRHistoryProvider>();
            var activeTextViewTracker = new ActiveTextViewTrackerMock(string.Empty, string.Empty);
            var debuggerModeTracker = new TestDebuggerModeTracker();
            _interactiveWorkflow = new RInteractiveWorkflow(sessionProvider, historyProvider, activeTextViewTracker, debuggerModeTracker, null, VsAppShell.Current, RToolsSettings.Current, () => {});
        }

        public void Dispose() {
            _interactiveWorkflow.Dispose();
        }

        [Test]
        [Category.Repl]
        public void DefaultDirectoryTest() {
            string myDocs = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            string actual;
            using (new VsRHostScript()) {
                var cmd = new WorkingDirectoryCommand(_interactiveWorkflow);
                cmd.InitializationTask.Wait();
                cmd.UserDirectory.Should().BeEquivalentTo(myDocs);
                actual = cmd.GetRWorkingDirectoryAsync().Result;
            };

            actual.Should().Be(myDocs);
        }

        [Test]
        [Category.Repl]
        public void SetDirectoryTest() {
            string dir = "c:\\";
            string actual;
            using (new VsRHostScript()) {
                var cmd = new WorkingDirectoryCommand(_interactiveWorkflow);
                cmd.InitializationTask.Wait();
                cmd.SetDirectory(dir).Wait();
                actual = cmd.GetRWorkingDirectoryAsync().Result;
            }

            actual.Should().Be(dir);
        }

        [Test]
        [Category.Repl]
        public void GetFriendlyNameTest01() {
            string actual;
            using (new VsRHostScript()) {
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
            using (new VsRHostScript()) {
                WorkingDirectoryCommand cmd = new WorkingDirectoryCommand(_interactiveWorkflow);
                cmd.InitializationTask.Wait();
                actual = cmd.GetFriendlyDirectoryName("c:\\");
            };

            actual.Should().Be("c:/");
        }

        [Test]
        [Category.Repl]
        public void GetFullPathNameTest() {
            string dir;
            using (new VsRHostScript()) {
                var cmd = new WorkingDirectoryCommand(_interactiveWorkflow);
                cmd.InitializationTask.Wait();
                dir = cmd.GetFullPathName("~");
            }

            string actual = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            actual.Should().Be(dir);
        }
    }
}
