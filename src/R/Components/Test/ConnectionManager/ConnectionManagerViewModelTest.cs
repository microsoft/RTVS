// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using FluentAssertions;
using Microsoft.R.Components.ConnectionManager;
using Microsoft.R.Components.ConnectionManager.Implementation.ViewModel;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.UnitTests.Core.Mef;
using Microsoft.UnitTests.Core.Threading;
using Microsoft.UnitTests.Core.XUnit;
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

        [Test(ThreadType.UI)]
        public void Connect() {
            var connection = _cmvm.LocalConnections.First();
            _cmvm.Connect(connection, true);

            _cmvm.LocalConnections.Should().ContainSingle(c => c.Name == connection.Name)
                .Which.IsConnected.Should().BeTrue();
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
        public void AddRemote_Edit_ChangePort() {
            _cmvm.EditNew();

            _cmvm.EditedConnection.Path = "https://machine";
            _cmvm.EditedConnection.UpdatePath();
            _cmvm.Save(_cmvm.EditedConnection);

            var connection = _cmvm.RemoteConnections.Should().ContainSingle(c => c.Name == "machine").Which;

            _cmvm.Edit(connection);
            connection.Path = "https://machine:5444";
            _cmvm.Save(connection);

            _cmvm.RemoteConnections.Should().ContainSingle(c => c.Name == "machine")
                .Which.Path.Should().Be("https://machine:5444");
        }

        [Test(ThreadType.UI)]
        public void AddRemote_Edit_ChangeName() {
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