// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Services;
using Microsoft.R.Components.ConnectionManager;
using Microsoft.R.Components.ConnectionManager.Implementation.ViewModel;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Host.Client;
using Microsoft.UnitTests.Core.FluentAssertions;
using Microsoft.UnitTests.Core.Threading;
using Microsoft.UnitTests.Core.XUnit;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.R.Components.Test.ConnectionManager {
    [ExcludeFromCodeCoverage]
    [Category.Connections]
    public sealed class ConnectionManagerViewModelTest : IDisposable {
        private readonly IRInteractiveWorkflow _workflow;
        private readonly IConnectionManagerVisualComponent _cmvc;
        private readonly ConnectionManagerViewModel _cmvm;

        public ConnectionManagerViewModelTest(IServiceContainer services) {
            _workflow = services.GetService<IRInteractiveWorkflowProvider>().GetOrCreate();
            _cmvc = UIThreadHelper.Instance.Invoke(() => _workflow.Connections.GetOrCreateVisualComponent());
            _cmvm = UIThreadHelper.Instance.Invoke(() => (ConnectionManagerViewModel)_cmvc.Control.DataContext);
        }
        
        public void Dispose() {
            _cmvc.Dispose();
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

            await UIThreadHelper.Instance.DoEventsAsync();

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
            _cmvm.TryEdit(connection);

            _cmvm.Connect(connection, connectToEdited);

            _cmvm.LocalConnections.Should().ContainSingle(c => c.Name == connection.Name)
                .Which.IsConnected.Should().BeTrue();
        }

        [Test(ThreadType.UI)]
        public void AddLocalWithSameName() {
            var connection = _cmvm.LocalConnections.First();
            _cmvm.Connect(connection, false);

            connection = _cmvm.LocalConnections.First(c => c.IsConnected);
            var name = connection.Name;

            _cmvm.TryEditNew();
            _cmvm.EditedConnection.Name = name;
            _cmvm.EditedConnection.Path = connection.Path;
            _cmvm.EditedConnection.UpdatePath();
            _cmvm.Save(_cmvm.EditedConnection);

            _cmvm.LocalConnections.Should().ContainSingle(c => c.Name.EqualsOrdinal(name))
                .Which.IsConnected.Should().BeTrue();
        }

        [Test(ThreadType.UI)]
        public void AddLocalWithSameName_ConnectToSecond() {
            var connection = _cmvm.LocalConnections.First();
            var copyName = $"{connection.Name} Copy";

            _cmvm.TryEditNew();
            _cmvm.EditedConnection.Name = copyName;
            _cmvm.EditedConnection.Path = connection.Path;
            _cmvm.EditedConnection.UpdatePath();
            _cmvm.Save(_cmvm.EditedConnection);

            var connectionCopy = _cmvm.LocalConnections.First(c => c.Name == copyName);
            _cmvm.Connect(connectionCopy, true);

            _cmvm.LocalConnections.Should().ContainSingle(c => c.IsConnected)
                .Which.Name.Should().Be(copyName);
        }

        [Test(ThreadType.UI)]
        public void AddLocal_Connect_Rename() {
            var connection = _cmvm.LocalConnections.First();
            var copyName = $"{connection.Name} Copy";

            _cmvm.TryEditNew();
            _cmvm.EditedConnection.Name = copyName;
            _cmvm.EditedConnection.Path = connection.Path;
            _cmvm.EditedConnection.UpdatePath();
            _cmvm.Save(_cmvm.EditedConnection);

            var connectionCopy = _cmvm.LocalConnections.First(c => c.Name == copyName);
            _cmvm.Connect(connectionCopy, true);

            connectionCopy = _cmvm.LocalConnections.First(c => c.IsConnected);

            var copy2Name = $"{copyName} 2";
            _cmvm.TryEdit(connectionCopy);
            connectionCopy.Name = copy2Name;
            _cmvm.Save(connectionCopy);

            UIThreadHelper.Instance.DoEvents();

            _workflow.RSessions.IsConnected.Should().BeFalse();
            _cmvm.IsConnected.Should().BeFalse();
            _cmvm.LocalConnections.Should().NotContain(c => c.IsConnected)
                .And.Contain(c => c.Name == copy2Name)
                .And.NotContain(c => c.Name == copyName);
        }

        [Test(ThreadType.UI)]
        public async Task AddLocalWithCommandLine() {
            var connection = _cmvm.LocalConnections.First();
            _cmvm.Connect(connection, false);

            connection = _cmvm.LocalConnections.First(c => c.IsConnected);
            var name = connection.Name + Guid.NewGuid();

            _cmvm.TryEditNew();
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
        public void RenameLocalToExistingName() {
            var connection = _cmvm.LocalConnections.First();
            _cmvm.Connect(connection, false);

            connection = _cmvm.LocalConnections.First(c => c.IsConnected);
            var connectedName = connection.Name;
            var name = Guid.NewGuid().ToString();

            _cmvm.TryEditNew();
            _cmvm.EditedConnection.Name = name;
            _cmvm.EditedConnection.Path = connection.Path;
            _cmvm.EditedConnection.UpdatePath();
            _cmvm.Save(_cmvm.EditedConnection);

            var names = _cmvm.LocalConnections.Select(c => c.Name).ToList();

            connection = _cmvm.LocalConnections.First(c => c.Name.EqualsOrdinal(name));
            _cmvm.TryEdit(connection);
            connection.Name = connectedName;
            _cmvm.Save(connection);

            // Failed Save shouldn't cancel any changes
            connection.Name.Should().Be(connectedName);

            connection.Reset();

            _cmvm.LocalConnections.Should().Equal(names, (c, n) => c.Name.EqualsOrdinal(n));
        }

        [Test(ThreadType.UI)]
        public void AddRemote_Edit_NoChanges() {
            _cmvm.TryEditNew();

            _cmvm.EditedConnection.Path = "https://machine";
            _cmvm.EditedConnection.UpdatePath();
            _cmvm.Save(_cmvm.EditedConnection);

            var connection = _cmvm.RemoteConnections.Should().ContainSingle(c => c.Name == "machine").Which;
            _cmvm.TryEdit(connection);
            _cmvm.Save(connection);

            _cmvm.RemoteConnections.Should().ContainSingle(c => ReferenceEquals(c, connection))
                .Which.HasChanges.Should().BeFalse();
        }

        [Test(ThreadType.UI)]
        public void AddTwoRemotesWithFragments() {
            _cmvm.TryEditNew();
            _cmvm.EditedConnection.Path = "https://machine#123";
            _cmvm.EditedConnection.UpdatePath();
            _cmvm.Save(_cmvm.EditedConnection);

            _cmvm.TryEditNew();
            _cmvm.EditedConnection.Name = "machine2";
            _cmvm.EditedConnection.Path = "https://machine#456";
            _cmvm.EditedConnection.UpdatePath();
            _cmvm.Save(_cmvm.EditedConnection);

            _cmvm.RemoteConnections.Should().ContainSingle(c => c.Name == "machine")
                .Which.Path.Should().Be("https://machine:5444#123");
            _cmvm.RemoteConnections.Should().ContainSingle(c => c.Name == "machine2")
                .Which.Path.Should().Be("https://machine:5444#456");
        }

        [CompositeTest(ThreadType.UI)]
        [InlineData("http://machine", "machine", "https://machine:5444")]
        [InlineData("https://machine:5444", "machine", "https://machine:5444")]
        [InlineData("https://machine#1234", "machine", "https://machine:5444#1234")]
        [InlineData("https://machine2", "machine2", "https://machine2:5444")]
        [InlineData("https://machine2:5444", "machine2", "https://machine2:5444")]
        public void AddRemote_Edit_ChangePath(string newPath, string expectedMachineName, string expectedPath) {
            _cmvm.TryEditNew();

            _cmvm.EditedConnection.Path = "https://machine";
            _cmvm.EditedConnection.UpdatePath();
            _cmvm.Save(_cmvm.EditedConnection);

            var connection = _cmvm.RemoteConnections.Should().ContainSingle(c => c.Name == "machine").Which;

            _cmvm.TryEdit(connection);
            connection.Path = newPath;
            _cmvm.EditedConnection.UpdatePath();
            _cmvm.Save(connection);

            _cmvm.RemoteConnections.Should().ContainSingle(c => c.Name == expectedMachineName)
                .Which.Path.Should().Be(expectedPath);
        }

        [Test(ThreadType.UI)]
        public void AddRemote_Edit_ChangeCommandLine() {
            _cmvm.TryEditNew();

            _cmvm.EditedConnection.Path = "https://machine";
            _cmvm.EditedConnection.UpdatePath();
            _cmvm.Save(_cmvm.EditedConnection);

            var connection = _cmvm.RemoteConnections.Should().ContainSingle(c => c.Name == "machine").Which;

            _cmvm.TryEdit(connection);
            connection.RCommandLineArguments = "--args 5";
            _cmvm.Save(connection);

            _cmvm.RemoteConnections.Should().ContainSingle(c => c.Name == "machine")
                .Which.RCommandLineArguments.Should().Be("--args 5");
        }

        [Test(ThreadType.UI)]
        public void AddRemote_Edit_Rename() {
            _cmvm.TryEditNew();

            _cmvm.EditedConnection.Path = "https://machine";
            _cmvm.EditedConnection.UpdatePath();
            _cmvm.Save(_cmvm.EditedConnection);

            var connection = (ConnectionViewModel)_cmvm.RemoteConnections.Should().ContainSingle(c => c.Name == "machine").Which;

            _cmvm.TryEdit(connection);
            connection.Name = "Custom Name";
            _cmvm.Save(connection);

            _cmvm.RemoteConnections.Should().NotContain(c => c.Name == "machine")
                .And.ContainSingle(c => c.Name == "Custom Name")
                .Which.Path.Should().Be("https://machine:5444");
        }
    }
}