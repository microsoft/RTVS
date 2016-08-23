// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Wpf;
using Microsoft.Common.Wpf.Collections;
using Microsoft.R.Components.ConnectionManager.ViewModel;
using Microsoft.R.Components.Extensions;

namespace Microsoft.R.Components.ConnectionManager.Implementation.ViewModel {
    internal sealed class ConnectionManagerViewModel : BindableBase, IConnectionManagerViewModel {
        private readonly IConnectionManager _connectionManager;
        private readonly ICoreShell _coreShell;
        private readonly BatchObservableCollection<object> _items;
        private IConnectionViewModel _selectedConnection;

        public ConnectionManagerViewModel(IConnectionManager connectionManager, ICoreShell coreShell) {
            _connectionManager = connectionManager;
            _coreShell = coreShell;
            _items = new BatchObservableCollection<object>();
            Items = new ReadOnlyObservableCollection<object>(_items);

            UpdateConnections();
        }

        public Task<int> Search(string searchString, CancellationToken cancellationToken) {
            throw new System.NotImplementedException();
        }

        public void Dispose() {}

        public ReadOnlyObservableCollection<object> Items { get; }

        public IConnectionViewModel SelectedConnection {
            get { return _selectedConnection; }
            private set { SetProperty(ref _selectedConnection, value); }
        }

        public void SelectConnection(IConnectionViewModel connection) {
            _coreShell.AssertIsOnMainThread();
            if (connection == _selectedConnection) {
                return;
            }

            if (SelectedConnection != null && SelectedConnection.HasChanges) {
                var dialogResult = _coreShell.ShowMessage(Resources.ConnectionManager_ChangedSelection_HasChanges, MessageButtons.YesNoCancel);
                switch (dialogResult) {
                    case MessageButtons.Yes:
                        SaveSelected();
                        break;
                    case MessageButtons.No:
                        CancelSelected();
                        break;
                    default:
                        return;
                }
            }

            SelectedConnection = connection;
        }

        public void AddNew() {
            SelectConnection(new ConnectionViewModel());
        }

        public void CancelSelected() {
            _coreShell.AssertIsOnMainThread();
            SelectedConnection?.Reset();
        }

        public void SaveSelected() {
            _coreShell.AssertIsOnMainThread();
            if (string.IsNullOrEmpty(SelectedConnection.Name)) {
                _coreShell.ShowMessage(Resources.ConnectionManager_ShouldHaveName, MessageButtons.OK);
                return;
            }

            if (string.IsNullOrEmpty(SelectedConnection.Path)) {
                _coreShell.ShowMessage(Resources.ConnectionManager_ShouldHavePath, MessageButtons.OK);
                return;
            }

            var connection = _connectionManager.AddOrUpdateConnection(
                SelectedConnection.Name,
                SelectedConnection.Path,
                SelectedConnection.RCommandLineArguments);

            if (connection.Id != SelectedConnection.Id && SelectedConnection.Id != null) {
                _connectionManager.TryRemove(SelectedConnection.Id);
            }

            UpdateConnections();
        }

        public async Task ConnectAsync(IConnectionViewModel connection) {
            _coreShell.AssertIsOnMainThread();
            await _connectionManager.ConnectAsync(SelectedConnection.Name, SelectedConnection.Path, SelectedConnection.RCommandLineArguments);
        }

        private void UpdateConnections() {
            _items.ReplaceWith(_connectionManager.RecentConnections.Select(c => new ConnectionViewModel(c)));
            if (Items.Count > 0) {
                SelectedConnection = (IConnectionViewModel)Items[0];
            }
        }
    }
}