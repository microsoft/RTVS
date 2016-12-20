// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.R.Components.ConnectionManager;
using Microsoft.R.Components.ConnectionManager.Implementation.ViewModel;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Host.Client;
using Microsoft.UnitTests.Core.FluentAssertions;
using Microsoft.UnitTests.Core.Mef;
using Microsoft.UnitTests.Core.Threading;
using Microsoft.UnitTests.Core.XUnit;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.R.Components.Test.ConnectionManager {
    [ExcludeFromCodeCoverage]
    [Category.Connections]
    public sealed class ConnectionManagerViewModelTest : IDisposable {
        private readonly IExportProvider _exportProvider;
        private readonly IRInteractiveWorkflow _workflow;
        private readonly IConnectionManagerVisualComponent _cmvc;
        private readonly ConnectionManagerViewModel _cmvm;

        public ConnectionManagerViewModelTest(RComponentsMefCatalogFixture mefCatalogFixture) {
            _exportProvider = mefCatalogFixture.CreateExportProvider();
            _workflow = _exportProvider.GetExportedValue<IRInteractiveWorkflowProvider>().GetOrCreate();
            _cmvc = UIThreadHelper.Instance.Invoke(() => _workflow.Connections.GetOrCreateVisualComponent());
            _cmvm = UIThreadHelper.Instance.Invoke(() => (ConnectionManagerViewModel)_cmvc.Control.DataContext);
        }
        
        public void Dispose() {
            _cmvc.Dispose();
            _exportProvider.Dispose();
        }

        [CompositeTest(ThreadType.UI)]
        [InlineData(true)]
        [InlineData(false)]
        public void Connect(bool connectToEdited) {
            var connection = _cmvm.LocalConnections.First();
            _cmvm.Connect(connection, connectToEdited);

            _cmvm.LocalConnections.Should().ContainSingle(c => c.IsConnected)
                .And.ContainSingle(c => c.Name == connection.Name)
                .Which.IsConnected.Should().BeTrue();
        }

        [Test(ThreadType.UI)]
        public void Properties() {
            var connection = _cmvm.LocalConnections.First();
            _cmvm.Connect(connection, true);

            var conn = _cmvm.LocalConnections.First(c => c.Name == connection.Name);
            conn.IsConnected.Should().BeTrue();
            conn.IsRunning.Should().BeTrue();

            conn.IsRunning = false;
            conn.IsConnected.Should().BeTrue();
        }

        [Test(ThreadType.UI)]
        public async Task StopInteractiveWindowSession() {
            var connection = _cmvm.LocalConnections.First();
            _cmvm.Connect(connection, true);
            await _workflow.RSession.StopHostAsync().Should().BeCompletedAsync();

            var conn = _cmvm.LocalConnections.First(c => c.Name == connection.Name);
            conn.IsConnected.Should().BeTrue();
            conn.IsRunning.Should().BeFalse();
        }

        [Test(ThreadType.UI)]
        public async Task ResetInteractiveWindow() {
            var connection = _cmvm.LocalConnections.First();
            _cmvm.Connect(connection, true);
            await _workflow.Operations.ResetAsync().Should().BeCompletedAsync();

            var conn = _cmvm.LocalConnections.First(c => c.Name == connection.Name);
            conn.IsConnected.Should().BeTrue();
            conn.IsRunning.Should().BeTrue();
        }

        [CompositeTest(ThreadType.UI)]
        [InlineData(true)]
        [InlineData(false)]
        public void Connect_EditConnected_Connect(bool connectToEdited) {
            var connection = _cmvm.LocalConnections.First();
            var name = connection.Name;
            _cmvm.Connect(connection, true);

            connection = _cmvm.LocalConnections.Should().ContainSingle(c => c.Name == name).Which;
            _cmvm.Edit(connection);

            _cmvm.Connect(connection, connectToEdited);

            _cmvm.LocalConnections.Should().ContainSingle(c => c.Name == connection.Name)
                .Which.IsConnected.Should().BeTrue();
        }

        [Test(ThreadType.UI)]
        public async Task AddLocalWithCommandLine() {
            var connection = _cmvm.LocalConnections.First();
            _cmvm.Connect(connection, false);

            connection = _cmvm.LocalConnections.First(c => c.IsConnected);
            var name = connection.Name + Guid.NewGuid();

            _cmvm.EditNew();
            _cmvm.EditedConnection.Name = name;
            _cmvm.EditedConnection.Path = connection.Path;
            _cmvm.EditedConnection.UpdatePath();
            _cmvm.EditedConnection.RCommandLineArguments = "--args 5";
            _cmvm.Save(_cmvm.EditedConnection);

            connection = _cmvm.LocalConnections.First(c => c.Name == name);
            _cmvm.Connect(connection, false);

            var result = await _workflow.RSession.EvaluateAsync<JValue>("commandArgs(trailingOnly = TRUE)", REvaluationKind.Normal);

            result.Value.Should().Be("5");
        }

        [Test(ThreadType.UI)]
        public void AddRemote_Edit_NoChanges() {
            _cmvm.EditNew();

            _cmvm.EditedConnection.Path = "https://machine";
            _cmvm.EditedConnection.UpdatePath();
            _cmvm.Save(_cmvm.EditedConnection);

            var connection = _cmvm.RemoteConnections.Should().ContainSingle(c => c.Name == "machine").Which;
            _cmvm.Edit(connection);
            _cmvm.Save(connection);

            _cmvm.RemoteConnections.Should().ContainSingle(c => ReferenceEquals(c, connection))
                .Which.HasChanges.Should().BeFalse();
        }

        [Test(ThreadType.UI)]
        public void AddTwoRemotesWithFragments() {
            _cmvm.EditNew();
            _cmvm.EditedConnection.Path = "https://machine#123";
            _cmvm.EditedConnection.UpdatePath();
            _cmvm.Save(_cmvm.EditedConnection);

            _cmvm.EditNew();
            _cmvm.EditedConnection.Name = "machine2";
            _cmvm.EditedConnection.Path = "https://machine#456";
            _cmvm.EditedConnection.UpdatePath();
            _cmvm.Save(_cmvm.EditedConnection);

            _cmvm.RemoteConnections.Should().ContainSingle(c => c.Name == "machine")
                .Which.Path.Should().Be("https://machine:443#123");
            _cmvm.RemoteConnections.Should().ContainSingle(c => c.Name == "machine2")
                .Which.Path.Should().Be("https://machine:443#456");
        }

        [CompositeTest(ThreadType.UI)]
        [InlineData("http://machine", "machine", "http://machine:80")]
        [InlineData("https://machine:5444", "machine", "https://machine:5444")]
        [InlineData("https://machine#1234", "machine", "https://machine:443#1234")]
        [InlineData("https://machine2", "machine2", "https://machine2:443")]
        [InlineData("https://machine2:5444", "machine2", "https://machine2:5444")]
        public void AddRemote_Edit_ChangePath(string newPath, string expectedMachineName, string expectedPath) {
            _cmvm.EditNew();

            _cmvm.EditedConnection.Path = "https://machine";
            _cmvm.EditedConnection.UpdatePath();
            _cmvm.Save(_cmvm.EditedConnection);

            var connection = _cmvm.RemoteConnections.Should().ContainSingle(c => c.Name == "machine").Which;

            _cmvm.Edit(connection);
            connection.Path = newPath;
            _cmvm.EditedConnection.UpdatePath();
            _cmvm.Save(connection);

            _cmvm.RemoteConnections.Should().ContainSingle(c => c.Name == expectedMachineName)
                .Which.Path.Should().Be(expectedPath);
        }

        [Test(ThreadType.UI)]
        public void AddRemote_Edit_ChangeCommandLine() {
            _cmvm.EditNew();

            _cmvm.EditedConnection.Path = "https://machine";
            _cmvm.EditedConnection.UpdatePath();
            _cmvm.Save(_cmvm.EditedConnection);

            var connection = _cmvm.RemoteConnections.Should().ContainSingle(c => c.Name == "machine").Which;

            _cmvm.Edit(connection);
            connection.RCommandLineArguments = "--args 5";
            _cmvm.Save(connection);

            _cmvm.RemoteConnections.Should().ContainSingle(c => c.Name == "machine")
                .Which.RCommandLineArguments.Should().Be("--args 5");
        }

        [Test(ThreadType.UI)]
        public void AddRemote_Edit_Rename() {
            _cmvm.EditNew();

            _cmvm.EditedConnection.Path = "https://machine";
            _cmvm.EditedConnection.UpdatePath();
            _cmvm.Save(_cmvm.EditedConnection);

            var connection = (ConnectionViewModel)_cmvm.RemoteConnections.Should().ContainSingle(c => c.Name == "machine").Which;

            _cmvm.Edit(connection);
            connection.Name = "Custom Name";
            _cmvm.Save(connection);

            _cmvm.RemoteConnections.Should().NotContain(c => c.Name == "machine")
                .And.ContainSingle(c => c.Name == "Custom Name")
                .Which.Path.Should().Be("https://machine:443");
        }
    }
}